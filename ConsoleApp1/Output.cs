using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Output
    {
        /// <summary>
        /// Writes a CSV snapshot: id,x,y,z,vx,vy,vz,mass
        /// </summary>
        public static void WriteSnapshotCsv(string path, BodyState s, bool use3D)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var sb = new StringBuilder(capacity: s.N * 64);
            sb.AppendLine("id,x,y,z,vx,vy,vz,mass");

            for (int i = 0; i < s.N; i++)
            {
                sb.Append(i).Append(',')
                  .Append(F(s.X[i])).Append(',')
                  .Append(F(s.Y[i])).Append(',')
                  .Append(F(use3D ? s.Z[i] : 0.0)).Append(',')
                  .Append(F(s.Vx[i])).Append(',')
                  .Append(F(s.Vy[i])).Append(',')
                  .Append(F(use3D ? s.Vz[i] : 0.0)).Append(',')
                  .Append(F(s.Mass[i]))
                  .AppendLine();
            }

            File.WriteAllText(path, sb.ToString());
        }

        private static string F(double v) => v.ToString("R", CultureInfo.InvariantCulture);
    }
}
