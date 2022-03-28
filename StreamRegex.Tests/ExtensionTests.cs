using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Extensions;


namespace StreamRegex.Tests;

[TestClass]
public class ExtensionTests
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [TestMethod]
    public void TestBasicFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        StreamReader content = new StreamReader(File.OpenRead("TargetMiddle.txt"));
        Assert.IsTrue(compiled.IsMatch(content));
    }
    
    [TestMethod]
    public void TestBufferOverlap()
    {
        var compiled = new Regex("45", RegexOptions.Compiled);
        StreamReader content = new StreamReader(StringToStream(ShortTestString));
        var res = compiled.GetFirstMatch(content, new StreamRegexOptions()
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
    public void TestBufferOverlapDedupe()
    {
        var compiled = new Regex("34", RegexOptions.Compiled);
        StreamReader content = new StreamReader(StringToStream("123456"));
        var opts = new StreamRegexOptions()
        {
            BufferSize = 4,
            OverlapSize = 2
        };
        var collection = compiled.GetMatchCollection(content, opts);
        Assert.AreEqual(1, collection.Count());
        Assert.AreEqual(2, collection.First().Index);
        Assert.AreEqual("34", collection.First().Value);
    }
    
    [TestMethod]
    public void TestIndexOf()
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
        Assert.AreEqual(5, content.IndexOf("6",StringComparison.InvariantCultureIgnoreCase, smallReadOptions));
    }
    
    [TestMethod]
    public void TestMatchFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        StreamReader content = new StreamReader(File.OpenRead("TargetMiddle.txt"));
        var res = compiled.GetFirstMatch(content);
        Assert.IsTrue(res.Success);
        Assert.AreEqual("racecar",res.Value);
    }

    [TestMethod]
    public void TestPositionFunctionality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        string contentString = File.ReadAllText("TargetMiddle.txt");
        StreamReader content = new StreamReader(File.OpenRead("TargetMiddle.txt"));
        var res = compiled.GetFirstMatch(content);
        var res2 = compiled.Match(contentString);
        Assert.IsTrue(res.Success);
        Assert.AreEqual("racecar",res.Value);
        Assert.AreEqual(res.Index, res2.Index);
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}