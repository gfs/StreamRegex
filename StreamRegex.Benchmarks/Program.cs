using BenchmarkDotNet.Running;
using StreamRegex.Benchmarks;

var summary = BenchmarkRunner.Run<PerformanceVsStandard>();
