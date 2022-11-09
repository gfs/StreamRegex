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
    public AsyncVsSyncBenchmarks()
    {
        _compiled = new Regex(Pattern, RegexOptions.Compiled);
    }

    private readonly Regex _compiled;
    private const string Pattern = "racecar";
    private Dictionary<(int, int, int), Stream> _streams = new();

    // Does not contain e so cannot match racecar
    string chars = "abcdfghijklmnopqrstuvwxyz123456789";
    Random random = new Random();

    [IterationSetup]
    public void IterationSetup()
    {
        if (!_streams.ContainsKey((numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)))
        {
            var _stream = new MemoryStream();
            var streamWriter = new StreamWriter(_stream, leaveOpen: true);
            for (int i = 0; i < numberPaddingSegmentsBefore; i++)
            {
                streamWriter.Write(getRandomString(paddingSegmentLength));
            }

            streamWriter.Write(Pattern);

            for (int i = 0; i < numberPaddingSegmentsAfter; i++)
            {
                streamWriter.Write(getRandomString(paddingSegmentLength));
            }
            streamWriter.Close();
            _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)] = _stream;
        }
        _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
    }

    private string getRandomString(int numCharacters)
    {
        return new string(Enumerable.Repeat(0, numCharacters).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
    
    const int zeroPadding = 0;

    // 100 MB
    const int midPadding = 1000 * 100;

    // 200 MB
    const int longPadding = 1000 * 200;


    [Params(1000)]
    public int paddingSegmentLength { get; set; }
    // [Params(zeroPadding, midPadding, longPadding)]
    [Params(longPadding)]
    public int numberPaddingSegmentsBefore { get; set; }
    // [Params(zeroPadding, midPadding, longPadding)]
    [Params(longPadding)]
    public int numberPaddingSegmentsAfter { get; set; }

    [BenchmarkCategory("AsyncVsSync")]
    [Benchmark(Baseline = true)]
    public void RegexExtension()
    {
        _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true);
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }
    
    [BenchmarkCategory("AsyncVsSync")]
    [Benchmark]
    public async Task RegexExtensionAsync()
    {
        _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true);
        var match = await _compiled.IsMatchAsync(content);
        if (!match)
        {
            throw new Exception($"The regex didn't match.");
        }
    }
}