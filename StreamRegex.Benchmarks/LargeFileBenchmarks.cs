using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Extensions.RegexExtensions;
using StreamRegex.Extensions.StringMethods;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]

// Tests checking for the string "racecar" that only occurs at the end of a very large file.
public class LargeFileBenchmarks
{
    private readonly Regex _compiled;
    private const string Pattern = "racecar";
    private Stream _stream = new MemoryStream();
    private const int _paddingLength = 1024 * 1024 * 100; // 100 MB
    private StringBuilder _testData = new StringBuilder();
    public LargeFileBenchmarks()
    {
        while (_testData.Length < _paddingLength)
        {
            _testData.Append(Enumerable.Repeat("a", 1024));
        }

        _testData.Append(Pattern);
        _compiled = new Regex(Pattern, RegexOptions.Compiled);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(_testData.ToString()));
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _stream.Dispose();
    }
    
    [BenchmarkCategory("Regex")]
    [Benchmark]
    [ExcludeFromCodeCoverage]
    public void CompiledRegex()
    {
        var content = new StreamReader(_stream).ReadToEnd();
        var match = _compiled.Match(content);
        if (!match.Success || match.Index != _paddingLength)
        {
            throw new Exception($"The regex didn't match {match.Index}.");
        }
    }
    
    [BenchmarkCategory("Regex")]
    [Benchmark]
    public void RegexExtension()
    {
        var content = new StreamReader(_stream);
        var match = _compiled.GetFirstMatch(content);
        if (!match.Success || match.Index != _paddingLength)
        {
            throw new Exception($"The regex didn't match {match.Index}.");
        }
    }
    
    [BenchmarkCategory("Contains")]
    [Benchmark(Baseline = true)]

    public void SimpleString()
    {
        var match = new StreamReader(_stream).ReadToEnd().IndexOf("racecar");
        if (match != _paddingLength)
        {
            throw new Exception($"The regex didn't match {match}.");
        }
    }
    
    [BenchmarkCategory("Contains")]
    [Benchmark]
    public void StringExtension()
    {
        var content = new StreamReader(_stream);
        var match = content.IndexOf("racecar");
        if (match != _paddingLength)
        {
            throw new Exception($"The regex didn't match {match}.");
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
}