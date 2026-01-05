using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public sealed class BodyState
    {
        public int N { get; }

        // Positions
        public double[] X { get; }
        public double[] Y { get; }
        public double[] Z { get; } // used only if Use3D

        // Velocities
        public double[] Vx { get; }
        public double[] Vy { get; }
        public double[] Vz { get; }

        // Accelerations (computed each step)
        public double[] Ax { get; }
        public double[] Ay { get; }
        public double[] Az { get; }

        public double[] Mass { get; }

        public BodyState(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            N = n;

            X = new double[n];
            Y = new double[n];
            Z = new double[n];

            Vx = new double[n];
            Vy = new double[n];
            Vz = new double[n];

            Ax = new double[n];
            Ay = new double[n];
            Az = new double[n];

            Mass = new double[n];
        }

        public BodyState CloneDeep()
        {
            var c = new BodyState(N);
            Array.Copy(X, c.X, N);
            Array.Copy(Y, c.Y, N);
            Array.Copy(Z, c.Z, N);

            Array.Copy(Vx, c.Vx, N);
            Array.Copy(Vy, c.Vy, N);
            Array.Copy(Vz, c.Vz, N);

            Array.Copy(Ax, c.Ax, N);
            Array.Copy(Ay, c.Ay, N);
            Array.Copy(Az, c.Az, N);

            Array.Copy(Mass, c.Mass, N);
            return c;
        }
    }
}
