// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using StreamRegex.Benchmarks;

var summary = BenchmarkRunner.Run<PerformanceVsStandard>();