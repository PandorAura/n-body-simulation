using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Initializers
    {
        /// <summary>
        /// Creates a deterministic random initial state.
        /// Positions in [-posRange, posRange], velocities in [-velRange, velRange], masses in [mMin, mMax].
        /// Also recenters momentum so total momentum ~ 0 (nice for stability).
        /// </summary>
        public static BodyState CreateRandom(SimConfig cfg,
            double posRange = 1.0,
            double velRange = 0.1,
            double mMin = 0.5,
            double mMax = 5.0)
        {
            var rng = new Random(cfg.Seed);
            var s = new BodyState(cfg.N);

            for (int i = 0; i < cfg.N; i++)
            {
                s.X[i] = RandUniform(rng, -posRange, posRange);
                s.Y[i] = RandUniform(rng, -posRange, posRange);
                s.Z[i] = cfg.Use3D ? RandUniform(rng, -posRange, posRange) : 0.0;

                s.Vx[i] = RandUniform(rng, -velRange, velRange);
                s.Vy[i] = RandUniform(rng, -velRange, velRange);
                s.Vz[i] = cfg.Use3D ? RandUniform(rng, -velRange, velRange) : 0.0;

                s.Mass[i] = RandUniform(rng, mMin, mMax);
            }

            RemoveNetMomentum(s, cfg.Use3D);
            return s;
        }

        private static double RandUniform(Random rng, double a, double b)
            => a + (b - a) * rng.NextDouble();

        private static void RemoveNetMomentum(BodyState s, bool use3D)
        {
            double px = 0, py = 0, pz = 0, mTotal = 0;

            for (int i = 0; i < s.N; i++)
            {
                double m = s.Mass[i];
                mTotal += m;
                px += m * s.Vx[i];
                py += m * s.Vy[i];
                if (use3D) pz += m * s.Vz[i];
            }

            if (mTotal <= 0) return;

            double vxOff = px / mTotal;
            double vyOff = py / mTotal;
            double vzOff = use3D ? (pz / mTotal) : 0.0;

            for (int i = 0; i < s.N; i++)
            {
                s.Vx[i] -= vxOff;
                s.Vy[i] -= vyOff;
                if (use3D) s.Vz[i] -= vzOff;
            }
        }
    }
}
