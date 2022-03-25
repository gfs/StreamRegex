namespace StreamRegex.Lib;

public class NopState : BaseState
{
    public NopState()
    {
        Success = this;
        Failure = this;
    }

    public override IState Transition(char character) => this;
    public override bool Accepts(char character) => false;

    public override bool IsFinal { get; } = false;
}