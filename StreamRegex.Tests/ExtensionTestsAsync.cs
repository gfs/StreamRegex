﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Extensions;


namespace StreamRegex.Tests;

[TestClass]
public class AsyncExtensionTests
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [TestMethod]
    public async Task TestBasicFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        Assert.IsTrue(await compiled.IsMatchAsync(StringToStream(str)));
    }
    
    [TestMethod]
    public async Task TestBufferOverlapAsync()
    {
        var compiled = new Regex("45", RegexOptions.Compiled);
        var res = await compiled.GetFirstMatchAsync(StringToStream(ShortTestString), new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        });
        Assert.IsTrue(res.Success);
        Assert.AreEqual("45",res.Value);
    }
    
    /// <summary>
    /// This test ensures that the collection properly dedupes the double match that will be found
    /// </summary>
    [TestMethod]
    public async Task TestBufferOverlapDedupeAsync()
    {
        var compiled = new Regex("34", RegexOptions.Compiled);
        var opts = new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        var collection = await compiled.GetMatchCollectionAsync(StringToStream("123456"), opts);
        Assert.AreEqual(1, collection.Count());
        Assert.AreEqual(2, collection.First().Index);
        Assert.AreEqual("34", collection.First().Value);
    }
    
    [TestMethod]
    public async Task TestIndexOfAsync()
    {
        StreamReader content = new StreamReader(StringToStream("123456"));
        Assert.AreEqual(2, content.IndexOf("3", StringComparison.InvariantCultureIgnoreCase));
        // This won't be found. The previous read read to the end of the buffer
        Assert.AreEqual(-1, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase));
        // If we reset the stream we find it properly
        content.BaseStream.Position = 0;
        var smallReadOptions = new SlidingBufferOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        Assert.AreEqual(5, await content.IndexOfAsync("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [TestMethod]
    public async Task TestIsMatchFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.IsMatchAsync(StringToStream(str));
        Assert.IsTrue(res);
    }
    
    [TestMethod]
    public async Task TestMatchFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.GetFirstMatchAsync(StringToStream(str));
        Assert.IsTrue(res.Success);
        Assert.AreEqual("racecar",res.Value);
    }

    [TestMethod]
    public async Task TestPositionFunctionalityAsync()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        var prefix = string.Join(string.Empty,Enumerable.Repeat("z", 10000));
        var str = $"{prefix}racecar{prefix}";
        var res = await compiled.GetFirstMatchAsync(StringToStream(str));
        var res2 = compiled.Match(str);
        Assert.IsTrue(res.Success);
        Assert.AreEqual("racecar",res.Value);
        Assert.AreEqual(res.Index, res2.Index);
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}