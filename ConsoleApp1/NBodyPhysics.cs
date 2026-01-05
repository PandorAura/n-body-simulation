using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class NBodyPhysics
    {
        /// <summary>
        /// Computes acceleration for bodies in [start, end).
        /// Reads positions/masses for all bodies.
        /// Writes only Ax/Ay/Az for i in [start,end).
        /// </summary>
        public static void ComputeAccelerationsRange(
            BodyState s,
            int start,
            int end,
            double G,
            double softening,
            bool use3D)
        {
            int n = s.N;
            if (start < 0 || end > n || start > end) throw new ArgumentOutOfRangeException();

            double eps2 = softening * softening;

            // Compute a_i = sum_j G*m_j*(rj-ri)/(|r|^2+eps^2)^(3/2)
            for (int i = start; i < end; i++)
            {
                double xi = s.X[i], yi = s.Y[i];
                double zi = use3D ? s.Z[i] : 0.0;

                double ax = 0, ay = 0, az = 0;

                for (int j = 0; j < n; j++)
                {
                    if (j == i) continue;

                    double dx = s.X[j] - xi;
                    double dy = s.Y[j] - yi;
                    double dz = use3D ? (s.Z[j] - zi) : 0.0;

                    double dist2 = dx * dx + dy * dy + dz * dz + eps2;
                    double invDist = 1.0 / Math.Sqrt(dist2);
                    double invDist3 = invDist * invDist * invDist;

                    double scale = G * s.Mass[j] * invDist3;
                    ax += dx * scale;
                    ay += dy * scale;
                    if (use3D) az += dz * scale;
                }

                s.Ax[i] = ax;
                s.Ay[i] = ay;
                s.Az[i] = use3D ? az : 0.0;
            }
        }

        /// <summary>
        /// Semi-implicit (symplectic) Euler:
        /// v += a*dt; x += v*dt
        /// Updates bodies in [start, end).
        /// </summary>
        public static void IntegrateRange(
            BodyState s,
            int start,
            int end,
            double dt,
            bool use3D)
        {
            int n = s.N;
            if (start < 0 || end > n || start > end) throw new ArgumentOutOfRangeException();

            for (int i = start; i < end; i++)
            {
                s.Vx[i] += s.Ax[i] * dt;
                s.Vy[i] += s.Ay[i] * dt;
                if (use3D) s.Vz[i] += s.Az[i] * dt;

                s.X[i] += s.Vx[i] * dt;
                s.Y[i] += s.Vy[i] * dt;
                if (use3D) s.Z[i] += s.Vz[i] * dt;
            }
        }

        public static void ZeroAccelerationsRange(BodyState s, int start, int end, bool use3D)
        {
            int n = s.N;
            if (start < 0 || end > n || start > end) throw new ArgumentOutOfRangeException();

            Array.Fill(s.Ax, 0.0, start, end - start);
            Array.Fill(s.Ay, 0.0, start, end - start);
            if (use3D) Array.Fill(s.Az, 0.0, start, end - start);
            else
            {
                // Ensure z-acc is 0 if running 2D
                Array.Fill(s.Az, 0.0, start, end - start);
            }
        }
    }
}
