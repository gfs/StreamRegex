using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using StreamRegex.Extensions;
using StreamRegex.Extensions.RegexExtensions;
using StreamRegex.Extensions.StringMethods;

namespace StreamRegex.Tests;

public class AsyncExtensionTests
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [Fact]
    public async Task TestBasicFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        Assert.True(await compiled.IsMatchAsync(StringToStream(str)));
    }
    
    [Fact]
    public async Task TestBufferOverlapAsync()
    {
        var compiled = new Regex("45", RegexOptions.Compiled);
        var res = await compiled.GetFirstMatchAsync(StringToStream(ShortTestString), new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2,
            DelegateOptions = new DelegateOptions(){ CaptureValues = true }
        });
        Assert.True(res.Success);
        Assert.Equal("45",res.Value);
    }
    
    /// <summary>
    /// This test ensures that the collection properly dedupes the double match that will be found
    /// </summary>
    [Fact]
    public async Task TestBufferOverlapDedupeAsync()
    {
        var compiled = new Regex("34", RegexOptions.Compiled);
        var opts = new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2,
            DelegateOptions = new DelegateOptions(){CaptureValues = true}

        };
        var collection = await compiled.GetMatchCollectionAsync(StringToStream("123456"), opts);
        Assert.Equal(1, collection.Count());
        Assert.Equal(2, collection.First().Index);
        Assert.Equal("34", collection.First().Value);
    }
    
    [Fact]
    // Test that the Stream isn't closed by the implicit stream reader
    public async Task StreamLeaveOpen()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var offset = 10000;
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", offset));
        var target = "racecar";
        var str = $"{prefix}{target}{prefix}";
        var stream = StringToStream(str);
        Assert.True(await compiled.IsMatchAsync(stream));
        stream.Position = 0;
        var firstMatch = await compiled.GetFirstMatchAsync(stream);
        Assert.Equal(offset,firstMatch.Index);
        stream.Position = 0;
        var matches = await compiled.GetMatchCollectionAsync(stream);
        Assert.Equal(offset, matches.First().Index);
        Assert.Equal(1, matches.Count);
        stream.Position = 0;
        Assert.True(await stream.ContainsAsync(target));
        stream.Position = 0;
        Assert.Equal(offset, await stream.IndexOfAsync(target));
        stream.Position = 0;
    }
    
    [Fact]
    public async Task TestIndexOfAsync()
    {
        StreamReader content = new StreamReader(StringToStream("123456"));
        Assert.Equal(2, await content.IndexOfAsync("3", StringComparison.InvariantCultureIgnoreCase));
        // This won't be found. The previous read read to the end of the buffer
        Assert.Equal(-1, await content.IndexOfAsync("6",StringComparison.InvariantCultureIgnoreCase));
        // If we reset the stream we find it properly
        content.BaseStream.Position = 0;
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        Assert.Equal(5, await content.IndexOfAsync("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [Fact]
    public async Task TestIndexOfAsyncTooSmallOverlap()
    {
        StreamReader content = new StreamReader(StringToStream("123456"));
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 1
        };
        Assert.Equal(5, await content.IndexOfAsync("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }

    [Fact]
    public async Task TestIsMatchFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.IsMatchAsync(StringToStream(str));
        Assert.True(res);
    }
    
    [Fact]
    public async Task TestMatchFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.GetFirstMatchAsync(StringToStream(str), new StreamRegexOptions(){DelegateOptions = new DelegateOptions(){CaptureValues = true}});
        Assert.True(res.Success);
        Assert.Equal("racecar",res.Value);
        res = await compiled.GetFirstMatchAsync(StringToStream(str));
        Assert.True(res.Success);
        Assert.Null(res.Value);

    }

    [Fact]
    public async Task TestPositionFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.GetFirstMatchAsync(StringToStream(str), new StreamRegexOptions(){DelegateOptions = new DelegateOptions(){CaptureValues = true}});
        var res2 = compiled.Match(str);
        Assert.True(res.Success);
        Assert.Equal("racecar",res.Value);
        Assert.Equal(res.Index, res2.Index);
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}