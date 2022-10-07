using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Extensions.Indexing;

namespace StreamRegex.Tests;

[TestClass]
public class GetRangeTests
{
    [TestMethod]
    public void GetRangeStreamOutOfRange()
    {
        var content = "racecar";
        Range toFetch = 2..1000;
        var bytesOfContent = new Span<byte>(new byte[content.Length]);
        var idx = 0;
        foreach (var character in content)
        {
            bytesOfContent[idx] = Convert.ToByte(character);
            idx++;
        }
        var streamOfContent = StringToStream(content);
        Assert.ThrowsException<IndexOutOfRangeException>(() => streamOfContent.GetRange(toFetch));
    }
    
    [TestMethod]
    public void GetRangeStream()
    {
        var content = "racecar";
        Range toFetch = 2..4;
        var bytesOfContent = new Span<byte>(new byte[content.Length]);
        var idx = 0;
        foreach (var character in content)
        {
            bytesOfContent[idx] = Convert.ToByte(character);
            idx++;
        }
        var streamOfContent = StringToStream(content);
        var fetched = streamOfContent.GetRange(toFetch);
        Assert.AreEqual(Encoding.UTF8.GetString(bytesOfContent[toFetch]), Encoding.UTF8.GetString(fetched));
    }
    
    [TestMethod]
    public void GetRangeStreamReader()
    {
        var content = "racecar";
        Range toFetch = 2..4;
        var charsOfContent = content.AsSpan()[toFetch];
        var streamOfContent = StringToStream(content);
        var streamReaderOfContent = new StreamReader(streamOfContent);
        var fetched = streamReaderOfContent.GetRange(toFetch);
        Assert.AreEqual(charsOfContent.ToString(), fetched.ToString());
    }
    
    [TestMethod]
    public void GetRangeStreamReaderOutOfRange()
    {
        var content = "racecar";
        Range toFetch = 2..1000;
        var streamOfContent = StringToStream(content);
        var streamReaderOfContent = new StreamReader(streamOfContent);
        Assert.ThrowsException<IndexOutOfRangeException>(() => streamReaderOfContent.GetRange(toFetch));
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}