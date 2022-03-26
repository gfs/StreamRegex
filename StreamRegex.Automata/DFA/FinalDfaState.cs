namespace StreamRegex.Lib.DFA;

public class FinalDfaState : BaseDfaState
{
    public FinalDfaState()
    {
        Success = this;
        Failure = this;
        IsFinal = true;
    }

    public override bool Accepts(char character) => true;
    public override IDfaState Transition(char character) => this;
}