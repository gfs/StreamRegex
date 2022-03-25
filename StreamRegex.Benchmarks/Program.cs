// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using BenchmarkDotNet.Running;
using StreamRegex.Benchmarks;
using StreamRegex.Lib;

var summary = BenchmarkRunner.Run<PerformanceVsStandard>();

// string _pattern = "[ra]*ce+c[ar]+";
// Stream _stream = File.OpenRead("TargetStart.txt");
// var _streaming = new RegexStreamMatcher();
// var _stateMachine = StateMachineFactory.CreateStateMachine(_pattern);
// Console.WriteLine(_stateMachine.GetMatchPosition(_stream));
