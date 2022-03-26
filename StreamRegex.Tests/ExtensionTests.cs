using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Lib.RegexStreamExtensions;


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
    public void TestMatchFunctinality()
    {
        var compiled = new Regex(ShortPattern, RegexOptions.Compiled);
        StreamReader content = new StreamReader(File.OpenRead("TargetMiddle.txt"));
        var res = compiled.GetFirstMatch(content);
        Assert.IsTrue(res.matches);
        Assert.AreEqual("racecar",res.match);
    }
    
    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}