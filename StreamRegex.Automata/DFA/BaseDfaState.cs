namespace StreamRegex.Lib.DFA;

public abstract class BaseDfaState : IDfaState
{
    public abstract bool Accepts(char character);
    public abstract IDfaState Transition(char character);
    public IDfaState Success { get; set; } = NopDfaState.Instance;
    public IDfaState Failure { get; set; } = NopDfaState.Instance;
    public bool IsFinal { get; init; } = false;
    public bool IsInitial { get; init; } = false;
}
