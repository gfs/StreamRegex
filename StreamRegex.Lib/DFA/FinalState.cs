namespace StreamRegex.Lib;

public class FinalState : BaseState
{
    public FinalState()
    {
        Success = this;
        Failure = this;
    }

    public override bool Accepts(char character) => true;
    public override IState Transition(char character) => this;
    public override bool IsFinal { get; } = true;
}