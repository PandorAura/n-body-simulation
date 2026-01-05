using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class BaselineSimulator
    {
        /// <summary>
        /// Single-thread reference simulator. This defines correctness.
        /// </summary>
        public static void Run(BodyState s, SimConfig cfg, Action<int, BodyState>? onStep = null)
        {
            int n = s.N;

            for (int step = 0; step < cfg.Steps; step++)
            {
                // Phase 0: clear accel
                NBodyPhysics.ZeroAccelerationsRange(s, 0, n, cfg.Use3D);

                // Phase 1: compute accel
                NBodyPhysics.ComputeAccelerationsRange(s, 0, n, cfg.G, cfg.Softening, cfg.Use3D);

                // Phase 2: integrate
                NBodyPhysics.IntegrateRange(s, 0, n, cfg.Dt, cfg.Use3D);

                onStep?.Invoke(step, s);
            }
        }
    }
}
