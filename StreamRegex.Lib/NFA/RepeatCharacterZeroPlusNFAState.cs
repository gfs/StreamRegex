namespace StreamRegex.Lib.NFA;

public class RepeatCharacterZeroPlusNfaState : BaseNfaState
{
    private readonly INfaState _previousState;
    private RepeatCharacterZeroPlusNfaState[]? _returnOnPrev;
    private INfaState[]? _returnOnBoth;

    public RepeatCharacterZeroPlusNfaState(INfaState previousState)
    {
        _previousState = previousState;
    }
    public override bool Accepts(char character)
    {
        return _previousState.Accepts(character);
    }
    
    public override INfaState[] Transition(char character)
    {
        var prev = _previousState.Accepts(character);
        var next = Success.Accepts(character);

        if (prev && next)
        {
            _returnOnBoth ??= Success.Transition(character).Append(Success).ToArray();
        }
        if (prev)
        {
            return _returnOnPrev ??= new[] {this};
        }
        if (next)
        {
            return Success.Transition(character);
        }
        return ReturnOnFailure ??= new[] {Failure};
    }
}