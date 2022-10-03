using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using StreamRegex.Extensions;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Benchmarks;
[MemoryDiagnoser]
public class PerformanceVsStandard
{
    private readonly Regex _compiled = new Regex(Pattern, RegexOptions.Compiled);
    private const string Pattern = "racecar";
    //private Stream _stream = new MemoryStream();
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
    [Params(zeroPadding, midPadding, longPadding)]
    public int numberPaddingSegmentsBefore { get; set; }
    [Params(zeroPadding, midPadding, longPadding)]
    public int numberPaddingSegmentsAfter { get; set; }


    [BenchmarkCategory("Regex")]
    [Benchmark(Baseline = true)]
    public void CompiledRegex()
    {
        _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true).ReadToEnd();
        if (!_compiled.IsMatch(content))
        {
            throw new Exception($"The regex didn't match.");
        }
    }
    
    [BenchmarkCategory("Regex")]
    [Benchmark]
    public void RegexExtension()
    {
        _streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)].Position = 0;
        var content = new StreamReader(_streams[(numberPaddingSegmentsBefore, numberPaddingSegmentsAfter, paddingSegmentLength)], leaveOpen: true);
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