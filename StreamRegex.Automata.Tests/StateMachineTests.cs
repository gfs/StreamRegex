using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Unicode;
using Xunit;
using StreamRegex.Lib;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Automata.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class DfaStateMachineTests
{
    [TestMethod]
    public void TestSimpleLiteralMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        Assert.AreEqual(6, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestSimpleLiteralNoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        Assert.AreEqual(-1, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestCharacterClass()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]");
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestZeroOrMoreQuantifier()
    {
        var stream = StringToStream("aaaab");
        var stateMachine = StateMachineFactory.CreateStateMachine("a*b");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestOneOrMoreQuantifier()
    {
        var stream = StringToStream("baaab");
        var stateMachine = StateMachineFactory.CreateStateMachine("a+b");
        Assert.AreEqual(1, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestAnyCharacter()
    {
        var stream = StringToStream("hello");
        var stateMachine = StateMachineFactory.CreateStateMachine("h.llo");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestEscapedCharacter()
    {
        var stream = StringToStream("a.b");
        var stateMachine = StateMachineFactory.CreateStateMachine("a\\.b");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestStateMachineValidation()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.IsTrue(stateMachine.ValidateStateMachine());
    }

    [TestMethod]
    public void TestMatch_ReturnsCorrectMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        var match = stateMachine.Match(stream);
        Assert.IsNotNull(match);
        Assert.AreEqual("world", match.matched);
        Assert.AreEqual(6, match.position);
    }

    [TestMethod]
    public void TestMatch_NoMatch_ReturnsNull()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        var match = stateMachine.Match(stream);
        Assert.IsNull(match);
    }

    [TestMethod]
    public void TestIsMatch_MatchExists()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        Assert.IsTrue(stateMachine.IsMatch(stream));
    }

    [TestMethod]
    public void TestIsMatch_NoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        Assert.IsFalse(stateMachine.IsMatch(stream));
    }

    [TestMethod]
    public void TestComplexPattern()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]+c");
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMatchAtBeginning()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = StateMachineFactory.CreateStateMachine("abc");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMatchAtEnd()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = StateMachineFactory.CreateStateMachine("def");
        Assert.AreEqual(3, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMultipleCharacterClass()
    {
        var stream = StringToStream("hello");
        var stateMachine = StateMachineFactory.CreateStateMachine("[helo]+");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}

[ExcludeFromCodeCoverage]
[TestClass]
public class NfaStateMachineTests
{
    [TestMethod]
    public void TestSimpleLiteralMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        Assert.AreEqual(6, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestSimpleLiteralNoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        Assert.AreEqual(-1, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestCharacterClass()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]");
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestZeroOrMoreQuantifier()
    {
        var stream = StringToStream("aaaab");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a*b");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestOneOrMoreQuantifier()
    {
        var stream = StringToStream("baaab");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a+b");
        Assert.AreEqual(1, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestAnyCharacter()
    {
        var stream = StringToStream("hello");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("h.llo");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestEscapedCharacter()
    {
        var stream = StringToStream("a.b");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a\\.b");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact(Skip = "Ignored test")]
    public void TestShortTestString()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestStateMachineValidation()
    {
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.IsTrue(stateMachine.ValidateStateMachine());
    }

    [TestMethod]
    public void TestOptionalQuantifier()
    {
        var stream = StringToStream("abc");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("ab?c");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestOptionalQuantifierNoOptionalChar()
    {
        var stream = StringToStream("ac");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("ab?c");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMatch_ReturnsCorrectMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        var match = stateMachine.Match(stream);
        Assert.IsNotNull(match);
        Assert.AreEqual("world", match.matched);
        Assert.AreEqual(6, match.position);
    }

    [TestMethod]
    public void TestMatch_NoMatch_ReturnsNull()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        var match = stateMachine.Match(stream);
        Assert.IsNull(match);
    }

    [TestMethod]
    public void TestIsMatch_MatchExists()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        Assert.IsTrue(stateMachine.IsMatch(stream));
    }

    [TestMethod]
    public void TestIsMatch_NoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        Assert.IsFalse(stateMachine.IsMatch(stream));
    }

    [TestMethod]
    public void TestComplexPattern()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]+c");
        Assert.AreEqual(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMatchAtBeginning()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("abc");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMatchAtEnd()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("def");
        Assert.AreEqual(3, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestMultipleCharacterClass()
    {
        var stream = StringToStream("hello");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[helo]+");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestRepeatedPattern()
    {
        var stream = StringToStream("aaabbbccc");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a+b+c+");
        Assert.AreEqual(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [TestMethod]
    public void TestLargeStream()
    {
        var largeString = new string('x', 10000) + "target" + new string('x', 10000);
        var stream = StringToStream(largeString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("target");
        Assert.AreEqual(10000, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}