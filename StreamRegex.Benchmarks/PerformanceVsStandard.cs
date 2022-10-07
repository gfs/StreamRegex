using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using StreamRegex.Extensions.RegexExtensions;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[ExcludeFromCodeCoverage]
public class PerformanceVsStandard
{
    private readonly Regex _compiled = new Regex(Pattern, RegexOptions.Compiled);
    private const string Pattern = "racecar";
    //private Stream _stream = new MemoryStream();
    private Dictionary<(int, int, int), Stream> _streams = new();
    // Does not contain e so cannot match racecar
    string _chars = "abcdfghijklmnopqrstuvwxyz123456789";
    Random _random = new Random();

    [IterationSetup]
    public void IterationSetup()
    {
        if (!_streams.ContainsKey((NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)))
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream, leaveOpen: true);
            for (int i = 0; i < NumberPaddingSegmentsBefore; i++)
            {
                streamWriter.Write(GetRandomString(PaddingSegmentLength));
            }

            streamWriter.Write(Pattern);

            for (int i = 0; i < NumberPaddingSegmentsAfter; i++)
            {
                streamWriter.Write(GetRandomString(PaddingSegmentLength));
            }
            streamWriter.Close();
            _streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)] = stream;
        }
        _streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)].Position = 0;
    }

    private string GetRandomString(int numCharacters)
    {
        return new string(Enumerable.Repeat(0, numCharacters).Select(_ => _chars[_random.Next(_chars.Length)]).ToArray());
    }
    
    const int ZeroPadding = 0;

    // 100 MB
    const int MidPadding = 1000 * 100;

    // 200 MB
    const int LongPadding = 1000 * 200;


    [Params(1000)]
    public int PaddingSegmentLength { get; set; }
    [Params(ZeroPadding, MidPadding, LongPadding)]
    public int NumberPaddingSegmentsBefore { get; set; }
    [Params(ZeroPadding, MidPadding, LongPadding)]
    public int NumberPaddingSegmentsAfter { get; set; }


    [BenchmarkCategory("Regex")]
    [Benchmark(Baseline = true)]
    public void CompiledRegex()
    {
        _streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)], leaveOpen: true).ReadToEnd();
#if NET7_0_OR_GREATER
        var matches = _compiled.EnumerateMatches(content.AsSpan());
        if (!matches.MoveNext())
        {
            throw new Exception($"The regex didn't match {Pattern}.");
        }
#else
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
#endif
    }
    
    [BenchmarkCategory("Regex")]
    [Benchmark]
    public void RegexExtension()
    {
        _streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(NumberPaddingSegmentsBefore, NumberPaddingSegmentsAfter, PaddingSegmentLength)], leaveOpen: true);
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }
    //
    // [BenchmarkCategory("IndexOf")]
    // [Benchmark]
    //
    // public void ReadThenStringIndexOf()
    // {
    //     _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
    //     var match = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true).ReadToEnd().IndexOf("racecar");
    //     if (match == -1)
    //     {
    //         throw new Exception($"The IndexOf didn't match.");
    //     }
    // }
    //
    // [BenchmarkCategory("IndexOf")]
    // [Benchmark]
    // public void IndexOfExtension()
    // {
    //     _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
    //     var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true);
    //     var match = content.IndexOf("racecar");
    //     if (match == -1)
    //     {
    //         throw new Exception($"The IndexOf didn't match.");
    //     }
    // }
    //
    // [BenchmarkCategory("Contains")]
    // [Benchmark]
    //
    // public void ReadThenStringContains()
    // {
    //     _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
    //     var match = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true).ReadToEnd().Contains("racecar");
    //     if (!match)
    //     {
    //         throw new Exception($"The Contains didn't match.");
    //     }
    // }
    //
    // [BenchmarkCategory("Contains")]
    // [Benchmark]
    // public void ContainsExtension()
    // {
    //     _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
    //     var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true);
    //     var match = content.Contains("racecar");
    //     if (!match)
    //     {
    //         throw new Exception($"The Contains didn't match.");
    //     }
    // }


    //// [Benchmark]
    //public void StateMachine()
    //{
    //    var stateMachine = StateMachineFactory.CreateStateMachine(Pattern);
    //    if (stateMachine.GetFirstMatchPosition(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)]) == -1)
    //    {
    //        throw new Exception("The regex didn't match");
    //    }
    //}
    
    //// [Benchmark]
    //public void NFAStateMachine()
    //{
    //    var stateMachine = NfaStateMachineFactory.CreateStateMachine(Pattern);
    //    var match = stateMachine.Match(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)]);
    //    if (match is null)
    //    {
    //        throw new Exception("The regex didn't match");
    //    }
    //}
}