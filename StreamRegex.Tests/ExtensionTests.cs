using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using StreamRegex.Extensions;
using StreamRegex.Extensions.Core;
using StreamRegex.Extensions.RegexExtensions;
using StreamRegex.Extensions.StringMethods;

namespace StreamRegex.Tests;

public class ExtensionTests
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [Fact]
    public void TestBasicFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        Assert.True(compiled.IsMatch(StringToStream(str)));
    }
    
    [Fact]
    public void MatchCollectionWithStreamDirectCallDelegate()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var offset = 10000;
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", offset));
        var target = "racecar";
        var str = $"{prefix}{target}{prefix}";
        var stream = StringToStream(str);
        Assert.True(compiled.IsMatch(stream));
        stream.Position = 0;
        var firstMatch = compiled.GetFirstMatch(stream);
        Assert.Equal(offset,firstMatch.Index);
        stream.Position = 0;
        var matches = stream.GetMatchCollection(((chunk, options) =>
        {
            SlidingBufferMatchCollection<SlidingBufferMatch> collection = new SlidingBufferMatchCollection<SlidingBufferMatch>();
            var idx = chunk.IndexOf(target, StringComparison.Ordinal);
            if (idx != -1)
            {
                collection.Add(new SlidingBufferMatch(true, idx, target.Length));
            }

            return collection;
        }));
        Assert.Equal(offset, matches.First().Index);
        Assert.Equal(1, matches.Count);
        stream.Position = 0;
        Assert.True(stream.Contains(target));
        stream.Position = 0;
        Assert.Equal(offset, stream.IndexOf(target));
        stream.Position = 0;
    }

    [Fact]
    public void StreamLeaveOpen()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var offset = 10000;
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", offset));
        var target = "racecar";
        var str = $"{prefix}{target}{prefix}";
        var stream = StringToStream(str);
        Assert.True(compiled.IsMatch(stream));
        stream.Position = 0;
        var firstMatch = compiled.GetFirstMatch(stream);
        Assert.Equal(offset,firstMatch.Index);
        stream.Position = 0;
        var matches = compiled.GetMatchCollection(stream);
        Assert.Equal(offset, matches.First().Index);
        Assert.Equal(1, matches.Count);
        stream.Position = 0;
        Assert.True(stream.Contains(target));
        stream.Position = 0;
        Assert.Equal(offset, stream.IndexOf(target));
        stream.Position = 0;
    }
    
    [Fact]
    public void TestBufferOverlap()
    {
        var compiled = new Regex("45", RegexOptions.Compiled);
        var stream = StringToStream(ShortTestString);
        var res = compiled.GetFirstMatch(stream, new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        });
        Assert.True(res.Success);
        stream.Position = res.Index;
        var reader = new StreamReader(stream);
        var first = (char)reader.Read();
        var second = (char)reader.Read();
        Assert.Equal("45", $"{first}{second}");
    }
    
    /// <summary>
    /// This test ensures that the collection properly dedupes the double match that will be found
    /// </summary>
    [Fact]
    public void TestBufferOverlapDedupe()
    {
        var compiled = new Regex("34", RegexOptions.Compiled);
        var opts = new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        var collection = compiled.GetMatchCollection(StringToStream("123456"), opts);
        Assert.Equal(1, collection.Count());
        Assert.Equal(2, collection.First().Index);
        Assert.Equal(2, collection.First().Length);
    }
    
    [Fact]
    public void TestIndexOf()
    {
        Stream content = StringToStream("123456");
        Assert.Equal(2, content.IndexOf("3", StringComparison.InvariantCultureIgnoreCase));
        // This won't be found. The previous read read to the end of the buffer
        Assert.Equal(-1, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase));
        // If we reset the stream we find it properly
        content.Position = 0;
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        Assert.Equal(5, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [Fact]
    public void TestIndexOfTooSmallOverlap()
    {
        Stream content = StringToStream("123456");
        Assert.Equal(2, content.IndexOf("3", StringComparison.InvariantCultureIgnoreCase));
        // This won't be found. The previous read read to the end of the buffer
        Assert.Equal(-1, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase));
        // If we reset the stream we find it properly
        content.Position = 0;
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 1
        };
        Assert.Equal(5, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [Fact]
    public void TestIndexOfTooSmallOverlapWithReader()
    {
        Stream content = StringToStream("123456");
        StreamReader reader = new StreamReader(content);
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 1
        };
        Assert.Equal(4, reader.IndexOf("56",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [Fact]
    public void TestIsMatchFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = compiled.IsMatch(StringToStream(str));
        Assert.True(res);
    }
    
    [Fact]
    public void TestIsMatchFunctionalityToSmallOverlap()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = compiled.IsMatch(StringToStream(str));
        Assert.True(res);
    }
    
    [Fact]
    public void TestMatchFunctionality()
    {
        var targetMatch = "racecar";
        var offset = 10000;
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", offset));
        var str = $"{prefix}{targetMatch}{prefix}";
        var stream = StringToStream(str);
        var res = compiled.GetFirstMatch(stream);
        Assert.True(res.Success);
        Assert.Equal(offset, res.Index);
        Assert.Equal(targetMatch.Length, res.Length);
        Assert.Equal(null, res.Value);
    }
    
    [Fact]
    public void TestMatchFunctionalityWithCapture()
    {
        var targetMatch = "racecar";
        var offset = 10000;
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", offset));
        var str = $"{prefix}{targetMatch}{prefix}";
        var stream = StringToStream(str);
        var res = compiled.GetFirstMatch(stream, new StreamRegexOptions(){DelegateOptions = new DelegateOptions(){CaptureValues = true}});
        Assert.True(res.Success);
        Assert.Equal(offset, res.Index);
        Assert.Equal(targetMatch.Length, res.Length);
        Assert.Equal(targetMatch, res.Value);
    }

    [Fact]
    public void TestPositionFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = compiled.GetFirstMatch(StringToStream(str));
        var res2 = compiled.Match(str);
        Assert.True(res.Success);
        Assert.Equal(res.Index, res2.Index);
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}