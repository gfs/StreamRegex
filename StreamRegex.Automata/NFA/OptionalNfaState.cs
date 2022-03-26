namespace StreamRegex.Lib.NFA;

/// <summary>
/// NFA State representing the ? symbol.
/// </summary>
public class OptionalNfaState : BaseNfaState
{
    private readonly INfaState _optionalState;
    private INfaState[]? _returnOnBoth;

    public OptionalNfaState(INfaState toMakeOptional)
    {
        _optionalState = toMakeOptional;
        Success = this;
        Failure = this;
    }

    public override INfaState[] Transition(char character)
    {
        ReturnOnSuccess ??= new[] {Success};

        bool optionalAccept = _optionalState.Accepts(character);
        bool successAccept = Success.Accepts(character);
        if (optionalAccept && successAccept)
        {
            _returnOnBoth ??= Success.Transition(character).Append(Success).ToArray();
        }
        if (optionalAccept)
        {
            return ReturnOnSuccess ??= new[] {Success};
        }
        if (successAccept)
        {
            return Success.Transition(character);
        }

        return ReturnOnFailure ??= new[] {Failure};
    }

    public override bool Accepts(char character)
    {
        return _optionalState.Accepts(character);
    }
}