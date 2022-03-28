using System.IO;
using System.Text;
using System.Text.Unicode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamRegex.Lib;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Tests;

[Ignore]
[TestClass]
public class UnitTest1
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [TestMethod]
    public void TestShortTestString()
    {
        var stream = StringToStream(ShortTestString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(ShortPattern);
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }
    
    [TestMethod]
    public void TestStateMachineValidation()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine(ShortPattern);
        Assert.IsTrue(stateMachine.ValidateStateMachine());
    }
    
    [TestMethod]
    public void TestOptionalTestString()
    {
        var stream = StringToStream(ShortTestString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(OptionalPattern);
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}