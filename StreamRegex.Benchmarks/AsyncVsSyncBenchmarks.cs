using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Extensions.RegexExtensions;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
[ExcludeFromCodeCoverage]
public class AsyncVsSyncBenchmarks
{
    private readonly Regex _compiled;
    private const string Pattern = "racecar";
    private Stream _stream = new MemoryStream();
    public AsyncVsSyncBenchmarks()
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

    [BenchmarkCategory("AsyncVsSync")]
    [Benchmark(Baseline = true)]
    public void RegexExtension()
    {
        var content = new StreamReader(_stream);
        var match = _compiled.GetFirstMatch(content);
        if (match.Success)
        {
            //
        }
    }
    
    [BenchmarkCategory("AsyncVsSync")]
    [Benchmark]
    public async Task RegexExtensionAsync()
    {
        var content = new StreamReader(_stream);
        var match = await _compiled.GetFirstMatchAsync(content);
        if (match.Success)
        {
            //
        }
    }
}