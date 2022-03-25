namespace StreamRegex.Lib.NFA;

public class OptionalNfaState : BaseNFAState
{
    private INFAState _optionalState;
    public OptionalNfaState(INFAState toMakeOptional)
    {
        _optionalState = toMakeOptional;
        Success = this;
        Failure = this;
    }
    
    public override IEnumerable<INFAState> Transition(char character)
    {
        if (_optionalState.Accepts(character))
        {
            yield return Success;
        }

        if (Success.Accepts(character))
        {
            foreach (var transition in Success.Transition(character))
            {
                yield return transition;
            }
        }

        yield return Failure;
    }

    public override bool Accepts(char character)
    {
        return _optionalState.Accepts(character);
    }

    public override bool IsFinal { get; } = false;
}