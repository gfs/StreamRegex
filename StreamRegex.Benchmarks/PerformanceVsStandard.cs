using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Lib;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
public class PerformanceVsStandard
{
    private readonly Regex _compiled;
    private const string _pattern = "[ra]*ce*c[ar]*";
    private Stream _stream;
    public PerformanceVsStandard()
    {
        _compiled = new Regex(_pattern, RegexOptions.Compiled);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _stream = File.OpenRead(TestFileName);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _stream.Dispose();
    }
    
    // [Params("TargetStart.txt","TargetMiddle.txt","TargetEnd.txt")]
    [Params("175MB.txt")]
    public string TestFileName { get; set; }
    
    [Benchmark(Baseline = true)]
    public void CompiledRegex()
    {
        var content = new StreamReader(_stream).ReadToEnd();
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }

    [Benchmark]
    public void StateMachine()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine(_pattern);
        if (stateMachine.GetMatchPosition(_stream) == -1)
        {
            throw new Exception("The regex didn't match");
        }
    }
}