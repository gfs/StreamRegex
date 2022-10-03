using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Extensions;


namespace StreamRegex.Tests;

[TestClass]
public class BenchmarksBehavior
{
    private readonly Regex _compiled;
    private const string Pattern = "racecar";
    //private Stream _stream = new MemoryStream();
    private Dictionary<(int, int, int), Stream> _streams = new();

    // Does not contain e so cannot match racecar
    string chars = "abcdfghijklmnopqrstuvwxyz123456789";
    Random random = new Random();

    const int paddingSegmentLength = 100;

    [DataRow(zeroPadding, longPadding, paddingSegmentLength)]
    [DataRow(midPadding, midPadding, paddingSegmentLength)]
    [DataRow(longPadding, zeroPadding, paddingSegmentLength)]
    [DataTestMethod]
    public void TestStreamCreate(int paddingStart, int paddingEnd, int paddingLength)
    {
        if (!_streams.ContainsKey((paddingStart, paddingEnd, paddingLength)))
        {
            var _stream = new MemoryStream();
            var streamWriter = new StreamWriter(_stream, leaveOpen: true);
            for (int i = 0; i < paddingStart; i++)
            {
                streamWriter.Write(Enumerable.Repeat(0, paddingLength).Select(_ => chars[random.Next(chars.Length)]));
            }

            streamWriter.Write(Pattern);

            for (int i = 0; i < paddingEnd; i++)
            {
                streamWriter.Write(Enumerable.Repeat(0, paddingLength).Select(_ => chars[random.Next(chars.Length)]));
            }
            streamWriter.Close();
            _stream.Position = 0;
            _streams[(paddingStart, paddingEnd, paddingLength)] = _stream;
        }
        else
        {
            _streams[(paddingStart, paddingEnd, paddingLength)].Position = 0;
        }
        var reader = new StreamReader(_streams[(paddingStart, paddingEnd, paddingLength)], leaveOpen: true);
        var stringversion = reader.ReadToEnd();
    }

    const int zeroPadding = 0;

    // 50 MB
    const int midPadding = 50;

    // 100 MB
    const int longPadding = 100;
}