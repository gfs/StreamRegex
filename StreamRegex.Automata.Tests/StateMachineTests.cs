using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Unicode;
using Xunit;
using StreamRegex.Lib;
using StreamRegex.Lib.DFA;
using StreamRegex.Lib.NFA;

namespace StreamRegex.Tests;

[ExcludeFromCodeCoverage]
public class UnitTest1
{
    private const string ShortTestString = "12345rararaceeecarrrra";
    private const string ShortPattern = "[ra]*ce+c[ar]+";
    private const string OptionalPattern = "[ra]*?ce+c[ar]+";

    [Fact(Skip = "Ignored test")]
    public void TestShortTestString()
    {
        var stream = StringToStream(ShortTestString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(ShortPattern);
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }
    
    [Fact(Skip = "Ignored test")]
    public void TestStateMachineValidation()
    {
        var stateMachine = StateMachineFactory.CreateStateMachine(ShortPattern);
        Assert.True(stateMachine.ValidateStateMachine());
    }
    
    [Fact(Skip = "Ignored test")]
    public void TestOptionalTestString()
    {
        var stream = StringToStream(ShortTestString);
        var stateMachine = NfaStateMachineFactory.CreateStateMachine(OptionalPattern);
        Assert.Equal(5, stateMachine.GetFirstMatchPosition(stream));
    }

    private Stream StringToStream(string str)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }
}