namespace StreamRegex.Lib.NFA;

public class RepeatCharacterZeroPlusNFAState : BaseNFAState
{
    private readonly INFAState _previousState;
    public RepeatCharacterZeroPlusNFAState(INFAState previousState)
    {
        _previousState = previousState;
    }
    public override bool Accepts(char character)
    {
        return _previousState.Accepts(character);
    }
    
    // TODO: This needs backtracking for cases like '[ce]*c' matched against 'racecar'.
    // This will greedily match up to racec, and then be able to match the next c.
    public override IEnumerable<INFAState> Transition(char character)
    {
        var prev = _previousState.Accepts(character);
        var next = Success.Accepts(character);
        if (!prev && !next)
        {
            yield return Failure;
            yield break;
        }
        if (prev)
        {
            yield return this;
        }
        if (next)
        {
            foreach (var nextTransition in Success.Transition(character))
            {
                yield return nextTransition;
            }
        }
    }
    public override bool IsFinal { get; } = false;
}