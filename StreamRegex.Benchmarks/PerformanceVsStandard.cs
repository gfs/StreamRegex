using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Lib;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
public class PerformanceVsStandard
{
    private readonly Regex _compiled;
    private readonly Regex _notCompiled;
    private RegexStreamMatcher _streamingPreCompiled;
    private RegexStreamMatcher _streaming;
    private const string _pattern = "[ra]*ce*c[ar]*";
    private Stream _stream;
    public PerformanceVsStandard()
    {
        _compiled = new Regex(_pattern, RegexOptions.Compiled);
        _notCompiled = new Regex(_pattern);
        
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _streamingPreCompiled = new RegexStreamMatcher();
        _streaming = new RegexStreamMatcher();
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
    
    [Params("TargetStart.txt","TargetMiddle.txt","TargetEnd.txt")]
    //[Params("BenchmarkDataStart.txt")]
    public string TestFileName { get; set; }
    
    // [Benchmark(Baseline = true)]
    // public void StandardRegex()
    // {
    //     var content = new StreamReader(_stream).ReadToEnd();
    //
    //     if (!_notCompiled.IsMatch(content))
    //     {
    //         throw new Exception($"The regex didn't match. First characters '{content.Length}'");
    //     }
    // }
    
    // [Benchmark]
    [Benchmark(Baseline = true)]
    public void CompiledRegex()
    {
        var content = new StreamReader(_stream).ReadToEnd();
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }
    // [Benchmark(Baseline = true)]
    // [Benchmark]
    public void StreamRegex()
    {
        _streaming.GetFirstMatchPosition(_pattern, _stream);
    }
    
    [Benchmark]
    public void StreamRegexPreCompiled()
    {
        if (_streamingPreCompiled.GetFirstMatchPosition(_pattern, _stream) == -1)
        {
            throw new Exception("The regex didn't match");
        }
    }
}