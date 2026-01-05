using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Validation
    {
        /// <summary>
        /// Returns max absolute difference across positions & velocities (and z if use3D).
        /// Useful for checking threaded/MPI outputs against baseline.
        /// </summary>
        public static double MaxAbsDiff(BodyState a, BodyState b, bool use3D)
        {
            if (a.N != b.N) throw new ArgumentException("State sizes differ");
            int n = a.N;

            double max = 0;

            for (int i = 0; i < n; i++)
            {
                max = Max(max, Abs(a.X[i] - b.X[i]));
                max = Max(max, Abs(a.Y[i] - b.Y[i]));
                if (use3D) max = Max(max, Abs(a.Z[i] - b.Z[i]));

                max = Max(max, Abs(a.Vx[i] - b.Vx[i]));
                max = Max(max, Abs(a.Vy[i] - b.Vy[i]));
                if (use3D) max = Max(max, Abs(a.Vz[i] - b.Vz[i]));
            }

            return max;
        }

        private static double Abs(double x) => x < 0 ? -x : x;
        private static double Max(double a, double b) => a > b ? a : b;
}
}
