// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using BenchmarkDotNet.Running;
using StreamRegex.Benchmarks;
using StreamRegex.Lib;

// var summary = BenchmarkRunner.Run<PerformanceVsStandard>();

string _pattern = "[ra]*ce+c[ar]+";
Stream _stream = File.OpenRead("TargetEnd.txt");
var _streaming = new RegexStreamMatcher();

_streaming.GetFirstMatchPosition(_pattern, _stream);