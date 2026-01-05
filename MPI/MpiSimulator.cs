using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPI;

namespace Mpi
{
    public static class MpiSimulator
    {
        public static void Run(string[] args, SimConfig cfg)
        {
            // 1. Initialize the MPI environment
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;

                // 2. Setup simulation locally
                // Since we use a fixed Seed in SimConfig, every rank generates 
                // the EXACT same initial state arrays. No need to send initial data!
                BodyState s = Initializers.CreateRandom(cfg);

                // 3. Determine my workload (Domain Decomposition)
                // If N=1000 and Size=4, chunk=250.
                // Rank 0: 0-250, Rank 1: 250-500, etc.
                int n = s.N;
                int chunkSize = n / comm.Size;
                int start = comm.Rank * chunkSize;
                int end = (comm.Rank == comm.Size - 1) ? n : start + chunkSize; // Handle remainder

                // Buffers for communication
                // We need to send OUR updated slice of X, Y, Z to everyone else
                double[] localX = new double[end - start];
                double[] localY = new double[end - start];
                double[] localZ = cfg.Use3D ? new double[end - start] : null;

                if (comm.Rank == 0) Console.WriteLine($"MPI Started: {comm.Size} workers. N={n}");

                // ===========================
                // MAIN SIMULATION LOOP
                // ===========================
                for (int step = 0; step < cfg.Steps; step++)
                {
                    // A. Compute Phase
                    // Calculate forces ONLY for my assigned bodies [start, end)
                    // But read from the GLOBAL s.X, s.Y (which are currently synced)
                    NBodyPhysics.ZeroAccelerationsRange(s, start, end, cfg.Use3D);
                    NBodyPhysics.ComputeAccelerationsRange(s, start, end, cfg.G, cfg.Softening, cfg.Use3D);

                    // B. Integrate Phase
                    // Update Velocity and Position for my assigned bodies
                    NBodyPhysics.IntegrateRange(s, start, end, cfg.Dt, cfg.Use3D);

                    // C. Copy my updated positions into a temp buffer to send
                    Array.Copy(s.X, start, localX, 0, end - start);
                    Array.Copy(s.Y, start, localY, 0, end - start);
                    if (cfg.Use3D) Array.Copy(s.Z, start, localZ, 0, end - start);

                    // ==========================================
                    // D. SYNCHRONIZATION (The Critical MPI Step)
                    // ==========================================
                    // We use Allgather. It takes my small chunk and gives back 
                    // an array of ALL chunks from everyone.
                    // Note: MPI.NET Allgather returns T[] (array of arrays).

                    //  
                    // Imagine everyone putting their piece of the puzzle on the table 
                    // so everyone can see the full picture.

                    double[][] gatheredX = comm.Allgather(localX);
                    double[][] gatheredY = comm.Allgather(localY);
                    double[][] gatheredZ = cfg.Use3D ? comm.Allgather(localZ) : null;

                    // E. Reconstruct the Global State
                    // Flatten the received chunks back into the main s.X array 
                    // so we are ready for the next step's force calculation.
                    Reassemble(s.X, gatheredX);
                    Reassemble(s.Y, gatheredY);
                    if (cfg.Use3D) Reassemble(s.Z, gatheredZ);

                    // F. Master Output (Optional)
                    // Only Rank 0 writes to disk or draws to screen
                    if (comm.Rank == 0)
                    {
                        if (cfg.SnapshotEvery > 0 && step % cfg.SnapshotEvery == 0)
                        {
                            string path = System.IO.Path.Combine(cfg.SnapshotFolder, $"snap_{step:D6}.csv");
                            Output.WriteSnapshotCsv(path, s, cfg.Use3D);
                            Console.WriteLine($"Step {step}: Saved snapshot.");
                        }
                    }
                }
            }
        }

        // Helper to flatten the array-of-arrays we get from MPI
        private static void Reassemble(double[] destination, double[][] chunks)
        {
            int offset = 0;
            foreach (var chunk in chunks)
            {
                Array.Copy(chunk, 0, destination, offset, chunk.Length);
                offset += chunk.Length;
            }
        }
    }
}
