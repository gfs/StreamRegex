using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StreamRegex.Extensions.RegexExtensions;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
[ExcludeFromCodeCoverage]
public class AutomataBenchmarks
{
    private readonly Regex _compiled = new Regex(Pattern, RegexOptions.Compiled);
    private const string Pattern = "racecar";
    private MemoryStream? _stream;

    // Simple literal pattern
    private const int StreamSizeSmall = 1000;
    private const int StreamSizeMedium = 100000;
    private const int StreamSizeLarge = 1000000;

    [Params(StreamSizeSmall, StreamSizeMedium, StreamSizeLarge)]
    public int StreamSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create a stream with the pattern at 50% position
        var content = new string('x', StreamSize / 2) + Pattern + new string('x', StreamSize / 2);
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _stream!.Position = 0;
    }

    [Benchmark(Baseline = true)]
    public void StandardRegexWithStreamReader()
    {
        _stream!.Position = 0;
        var content = new StreamReader(_stream, leaveOpen: true);
        if (!_compiled.IsMatch(content))
        {
            throw new Exception("The regex didn't match.");
        }
    }

    [Benchmark]
    public void DfaAutomata()
    {
        _stream!.Position = 0;
        var stateMachine = StateMachineFactory.CreateStateMachine(Pattern);
        if (!stateMachine.IsMatch(_stream))
        {
            throw new Exception("The regex didn't match");
        }
    }

    [Benchmark]
    public void NfaAutomata()
    {
        _stream!.Position = 0;
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(Pattern);
        if (!stateMachine.IsMatch(_stream))
        {
            throw new Exception("The regex didn't match");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stream?.Dispose();
    }
}
