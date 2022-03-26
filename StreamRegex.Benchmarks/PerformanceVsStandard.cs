using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Extensions;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
public class PerformanceVsStandard
{
    private readonly Regex _compiled;
    private const string Pattern = "racecar";
    private Stream _stream = new MemoryStream();
    public PerformanceVsStandard()
    {
        _compiled = new Regex(Pattern, RegexOptions.Compiled);
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
    
    //[Params("TargetStart.txt","TargetMiddle.txt","TargetEnd.txt")]
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

    // [Benchmark]
    public void StateMachine()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine(Pattern);
        if (stateMachine.GetFirstMatchPosition(_stream) == -1)
        {
            throw new Exception("The regex didn't match");
        }
    }
    
    // [Benchmark]
    public void NFAStateMachine()
    {
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(Pattern);
        var match = stateMachine.Match(_stream);
        if (match is null)
        {
            throw new Exception("The regex didn't match");
        }
    }
    
    [Benchmark]
    public void RegexExtension()
    {
        var content = new StreamReader(_stream);
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }
}