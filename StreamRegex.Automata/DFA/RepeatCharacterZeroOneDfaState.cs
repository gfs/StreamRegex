namespace StreamRegex.Lib.DFA;

public class RepeatCharacterZeroOneDfaState : BaseDfaState
{
    private readonly IDfaState _previousDfaState;

    public RepeatCharacterZeroOneDfaState(IDfaState previousDfaState)
    {
        _previousDfaState = previousDfaState;
    }

    public override bool Accepts(char character)
    {
        return _previousDfaState.Accepts(character);
    }

    public override IDfaState Transition(char character)
    {
        if (_previousDfaState.Accepts(character))
        {
            return Success.Accepts(character) ? Success.Transition(character) : Success;
        }

        if (Success.Accepts(character))
        {
            return Success.Transition(character);
        }

        return Failure;
    }
}
