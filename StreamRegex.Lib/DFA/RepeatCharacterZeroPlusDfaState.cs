using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class RepeatCharacterZeroPlusDfaState : BaseDfaState
{
    private readonly IDfaState _previousDfaState;
    public RepeatCharacterZeroPlusDfaState(IDfaState previousDfaState)
    {
        _previousDfaState = previousDfaState;
    }
    public override bool Accepts(char character)
    {
        return _previousDfaState.Accepts(character);
    }
    
    // TODO: This needs backtracking for cases like '[ce]*c' matched against 'racecar'.
    // This will greedily match up to racec, and then be able to match the next c.
    public override IDfaState Transition(char character)
    {
        if (_previousDfaState.Accepts(character))
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