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
    
    // TODO: This needs backtracking for cases like '[ce]*c' matched against 'racecar'.
    // This will greedily match up to racec, and then be able to match the next c.
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