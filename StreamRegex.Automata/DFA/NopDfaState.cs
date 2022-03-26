namespace StreamRegex.Lib.DFA;

public class NopDfaState : BaseDfaState
{
    public NopDfaState()
    {
        Success = this;
        Failure = this;
    }

    public override IDfaState Transition(char character) => this;
    public override bool Accepts(char character) => false;
}