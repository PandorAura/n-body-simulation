using Core;
using System;
using System.Diagnostics;
using System.IO;
using Threaded;

class Program
{
    static void Main()
    {
        var cfg = new SimConfig
        {
            N = 100,
            Steps = 200,
            Dt = 0.01,
            G = 1.0,
            Softening = 1e-3,
            Use3D = false,
            Seed = 12345
        };

        int snapEvery = 20; 
        string outDir = "out";
        Directory.CreateDirectory(outDir);

        var init = Initializers.CreateRandom(cfg);

        // ----------------------------
        // Baseline (single-thread)
        // ----------------------------
        var baseline = init.CloneDeep();
        var sw = Stopwatch.StartNew();

        BaselineSimulator.Run(baseline, cfg, (step, state) =>
        {
            if (step % snapEvery == 0)
            {
                var path = Path.Combine(outDir, $"baseline_{step:D6}.csv");
                Output.WriteSnapshotCsv(path, state, cfg.Use3D);
            }
        });

        sw.Stop();
        Console.WriteLine($"Baseline: {sw.ElapsedMilliseconds} ms");
        Output.WriteSnapshotCsv(Path.Combine(outDir, "baseline_final.csv"), baseline, cfg.Use3D);

        // ----------------------------
        // Tasks / Futures version
        // ----------------------------
        var parallel = init.CloneDeep();
        sw.Restart();

        TaskRangeSimulator.Run(parallel, cfg, workers: Environment.ProcessorCount, onStep: (step, state) =>
        {
            if (step % snapEvery == 0)
            {
                var path = Path.Combine(outDir, $"tasks_{step:D6}.csv");
                Output.WriteSnapshotCsv(path, state, cfg.Use3D);
            }
        });

        sw.Stop();
        Console.WriteLine($"Tasks:    {sw.ElapsedMilliseconds} ms");
        Output.WriteSnapshotCsv(Path.Combine(outDir, "tasks_final.csv"), parallel, cfg.Use3D);

        // ----------------------------
        // Compare results
        // ----------------------------
        double diff = Validation.MaxAbsDiff(baseline, parallel, cfg.Use3D);
        Console.WriteLine($"MaxAbsDiff: {diff:e3}");
    }
}
