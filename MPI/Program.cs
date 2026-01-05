using System;
using Core;
using Mpi;

namespace MPI
{
    class Program
    {
        // 1. Notice string[] args here
        static void Main(string[] args)
        {
            // Create the configuration
            var cfg = new SimConfig
            {
                N = 1000,
                Steps = 1000,
                Dt = 0.01,
                G = 1.0,
                Softening = 1e-3,
                Use3D = false,
                SnapshotEvery = 20
            };

            // 2. Pass 'args' into the Run method
            MpiSimulator.Run(args, cfg);
        }
    }
}