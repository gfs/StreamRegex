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
public class DfaStateMachineTests
{
    [Fact]
    public void TestSimpleLiteralMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        Assert.Equal(6, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestSimpleLiteralNoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        Assert.Equal(-1, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestCharacterClass()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]");
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestZeroOrMoreQuantifier()
    {
        var stream = StringToStream("aaaab");
        var stateMachine = StateMachineFactory.CreateStateMachine("a*b");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestOneOrMoreQuantifier()
    {
        var stream = StringToStream("baaab");
        var stateMachine = StateMachineFactory.CreateStateMachine("a+b");
        Assert.Equal(1, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestAnyCharacter()
    {
        var stream = StringToStream("hello");
        var stateMachine = StateMachineFactory.CreateStateMachine("h.llo");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestEscapedCharacter()
    {
        var stream = StringToStream("a.b");
        var stateMachine = StateMachineFactory.CreateStateMachine("a\\.b");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestStateMachineValidation()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.True(stateMachine.ValidateStateMachine());
    }

    [Fact]
    public void TestMatch_ReturnsCorrectMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        var match = stateMachine.Match(stream);
        Assert.NotNull(match);
        Assert.Equal("world", match.matched);
        Assert.Equal(6, match.position);
    }

    [Fact]
    public void TestMatch_NoMatch_ReturnsNull()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        var match = stateMachine.Match(stream);
        Assert.Null(match);
    }

    [Fact]
    public void TestIsMatch_MatchExists()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("world");
        Assert.True(stateMachine.IsMatch(stream));
    }

    [Fact]
    public void TestIsMatch_NoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = StateMachineFactory.CreateStateMachine("xyz");
        Assert.False(stateMachine.IsMatch(stream));
    }

    [Fact]
    public void TestComplexPattern()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = StateMachineFactory.CreateStateMachine("[ra]+c");
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMatchAtBeginning()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = StateMachineFactory.CreateStateMachine("abc");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMatchAtEnd()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = StateMachineFactory.CreateStateMachine("def");
        Assert.Equal(3, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMultipleCharacterClass()
    {
        var stream = StringToStream("hello");
        var stateMachine = StateMachineFactory.CreateStateMachine("[helo]+");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}

[ExcludeFromCodeCoverage]
public class NfaStateMachineTests
{
    [Fact]
    public void TestSimpleLiteralMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        Assert.Equal(6, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestSimpleLiteralNoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        Assert.Equal(-1, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestCharacterClass()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]");
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestZeroOrMoreQuantifier()
    {
        var stream = StringToStream("aaaab");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a*b");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestOneOrMoreQuantifier()
    {
        var stream = StringToStream("baaab");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a+b");
        Assert.Equal(1, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestAnyCharacter()
    {
        var stream = StringToStream("hello");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("h.llo");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestEscapedCharacter()
    {
        var stream = StringToStream("a.b");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a\\.b");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact(Skip = "Ignored test")]
    public void TestShortTestString()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestStateMachineValidation()
    {
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]*ce+c[ar]+");
        Assert.True(stateMachine.ValidateStateMachine());
    }

    [Fact]
    public void TestOptionalQuantifier()
    {
        var stream = StringToStream("abc");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("ab?c");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestOptionalQuantifierNoOptionalChar()
    {
        var stream = StringToStream("ac");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("ab?c");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMatch_ReturnsCorrectMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        var match = stateMachine.Match(stream);
        Assert.NotNull(match);
        Assert.Equal("world", match.matched);
        Assert.Equal(6, match.position);
    }

    [Fact]
    public void TestMatch_NoMatch_ReturnsNull()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        var match = stateMachine.Match(stream);
        Assert.Null(match);
    }

    [Fact]
    public void TestIsMatch_MatchExists()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("world");
        Assert.True(stateMachine.IsMatch(stream));
    }

    [Fact]
    public void TestIsMatch_NoMatch()
    {
        var stream = StringToStream("hello world");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("xyz");
        Assert.False(stateMachine.IsMatch(stream));
    }

    [Fact]
    public void TestComplexPattern()
    {
        var stream = StringToStream("12345rararaceeecarrrra");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[ra]+c");
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMatchAtBeginning()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("abc");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMatchAtEnd()
    {
        var stream = StringToStream("abcdef");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("def");
        Assert.Equal(3, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestMultipleCharacterClass()
    {
        var stream = StringToStream("hello");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("[helo]+");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestRepeatedPattern()
    {
        var stream = StringToStream("aaabbbccc");
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("a+b+c+");
        Assert.Equal(0, stateMachine.GetFirstMatchPosition(stream));
    }

    [Fact]
    public void TestLargeStream()
    {
        var largeString = new string('x', 10000) + "target" + new string('x', 10000);
        var stream = StringToStream(largeString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine("target");
        Assert.Equal(10000, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}