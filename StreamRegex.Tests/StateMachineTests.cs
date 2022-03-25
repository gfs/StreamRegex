using System.IO;
using System.Text;
using System.Text.Unicode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Lib;

namespace StreamRegex.Tests;

[TestClass]
public class UnitTest1
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    
    [TestMethod]
    public void TestShortTestString()
    {
        var stream = StringToStream(ShortTestString);
        var stateMachine = StateMachineFactory.CreateStateMachine(ShortPattern);
        Assert.AreEqual(5, stateMachine.GetMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}