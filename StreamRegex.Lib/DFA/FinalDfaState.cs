using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class FinalDfaState : BaseDfaState
{
    public FinalDfaState()
    {
        Success = this;
        Failure = this;
    }

    public override bool Accepts(char character) => true;
    public override IDfaState Transition(char character) => this;
    public override bool IsFinal { get; } = true;
}