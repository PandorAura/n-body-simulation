using Core;

namespace Threaded
{
    public static class TaskRangeSimulator
    {
        public static void Run(
            BodyState s,
            SimConfig cfg,
            int? workers = null,
            Action<int, BodyState>? onStep = null)
        {
            int n = s.N;
            int T = Math.Min(workers ?? Environment.ProcessorCount, n);

            for (int step = 0; step < cfg.Steps; step++)
            {
                // Phase 0: clear acceleration
                RunRanges(T, n, (start, end) =>
                    NBodyPhysics.ZeroAccelerationsRange(s, start, end, cfg.Use3D));

                // Phase 1: compute accelerations
                RunRanges(T, n, (start, end) =>
                    NBodyPhysics.ComputeAccelerationsRange(s, start, end, cfg.G, cfg.Softening, cfg.Use3D));

                // Phase 2: integrate
                RunRanges(T, n, (start, end) =>
                    NBodyPhysics.IntegrateRange(s, start, end, cfg.Dt, cfg.Use3D));

                onStep?.Invoke(step, s);
            }
        }

        private static void RunRanges(int T, int n, Action<int, int> work)
        {
            var tasks = new Task[T];

            for (int t = 0; t < T; t++)
            {
                int start = t * n / T;
                int end = (t + 1) * n / T;

                tasks[t] = Task.Run(() => work(start, end));
            }

            // Barrier: wait all workers in this phase
            Task.WaitAll(tasks);
        }
    }
}
