namespace StreamRegex.Lib;

public class RepeatCharacterZeroPlusState : BaseState
{
    private readonly IState _previousState;
    public RepeatCharacterZeroPlusState(IState previousState)
    {
        _previousState = previousState;
    }
    public override bool Accepts(char character)
    {
        return _previousState.Accepts(character);
    }
    public override IState Transition(char character)
    {
        if (_previousState.Accepts(character))
        {
            return this;
        }
        if (Success.Accepts(character))
        {
            return Success.Transition(character);
        }

        return Failure;
    }
    public override bool IsFinal { get; } = false;
}