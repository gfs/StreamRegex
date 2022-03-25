using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Lib;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
public class PerformanceVsStandard
{
    private readonly Regex _compiled;
    private readonly Regex _notCompiled;
    private RegexStreamReader _streamingPreCompiled;
    private RegexStreamReader _streaming;
    private const string _pattern = "[ra]*ce+c[ar]+";
    private Stream _stream;
    public PerformanceVsStandard()
    {
        _compiled = new Regex(_pattern, RegexOptions.Compiled);
        _notCompiled = new Regex(_pattern);
        
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _stream = File.OpenRead(TestFileName);
        _streamingPreCompiled = new RegexStreamReader(_stream);
        _streamingPreCompiled.CompileAndCachePattern(_pattern);
        _streaming = new RegexStreamReader(_stream);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
    }
    
    // [Params("BenchmarkDataStart.txt","BenchmarkDataMiddle.txt","BenchmarkDataEnd.txt")]
    [Params("BenchmarkDataStart.txt")]
    public string TestFileName { get; set; }
    
    // [Benchmark(Baseline = true)]
    public void StandardRegex()
    {
        _notCompiled.IsMatch(new StreamReader(_stream).ReadToEnd());
    }
    
    // [Benchmark]
    [Benchmark(Baseline = true)]
    public void CompiledRegex()
    {
        _compiled.IsMatch(new StreamReader(_stream).ReadToEnd());
    }
    
    // [Benchmark]
    public void StreamRegex()
    {
        _streaming.GetFirstMatchPosition(_pattern);
    }
    
    [Benchmark]
    public void StreamRegexPreCompiled()
    {
        _streamingPreCompiled.GetFirstMatchPosition(_pattern);
    }
}