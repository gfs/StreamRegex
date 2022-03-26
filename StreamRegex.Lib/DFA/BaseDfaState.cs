namespace StreamRegex.Lib.DFA;

public abstract class BaseDfaState : IDfaState
{
    public abstract bool Accepts(char character);
    public abstract IDfaState Transition(char character);
    public IDfaState Success { get; set; } = new NopDfaState();
    public IDfaState Failure { get; set; } = new NopDfaState();
    public bool IsFinal { get; init; } = false;
    public bool IsInitial { get; init; } = false;
}