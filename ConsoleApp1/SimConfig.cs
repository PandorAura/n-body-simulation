using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public sealed class SimConfig
    {
        public int N { get; init; } = 1000;
        public int Steps { get; init; } = 1000;

        public double Dt { get; init; } = 0.01;
        public double G { get; init; } = 1.0;
        public double Softening { get; init; } = 1e-3; // eps
        public bool Use3D { get; init; } = false;

        public int Seed { get; init; } = 12345;

        public int SnapshotEvery { get; init; } = 0; // 0 => no snapshots
        public string SnapshotFolder { get; init; } = "out";
    }
}
