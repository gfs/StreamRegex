namespace StreamRegex.Lib.NFA;

public abstract class BaseNfaState : INfaState
{
    public abstract bool Accepts(char character);
    public abstract INfaState[] Transition(char character);
    public bool IsFinal { get; protected init; } = false;
    public bool IsInitial { get; protected init; } = false;
    public INfaState Success { get; set; } = NopNfaState.Instance;
    public INfaState Failure { get; set; } = NopNfaState.Instance;
    public int Index { get; set; } = -1;

    internal INfaState[]? ReturnOnSuccess = null;
    internal INfaState[]? ReturnOnFailure = null;
    public INfaState[] DefaultTransition(char character)
    {
        if (ReturnOnFailure is null)
        {
            ReturnOnFailure = new[] { Failure };
        }
        if (ReturnOnSuccess is null)
        {
            ReturnOnSuccess = new[] { Success };
        }
        return Accepts(character) ? ReturnOnSuccess : ReturnOnFailure;
    }
}