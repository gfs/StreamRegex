using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class NopDfaState : BaseDfaState
{
    public NopDfaState()
    {
        Success = this;
        Failure = this;
    }

    public override IDfaState Transition(char character) => this;
    public override bool Accepts(char character) => false;

    public override bool IsFinal { get; } = false;
}