using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public abstract class BaseDfaState : IDfaState
{
    public abstract bool Accepts(char character);
    public abstract IDfaState Transition(char character);
    public IDfaState Success { get; set; }
    public IDfaState Failure { get; set; }
    public abstract bool IsFinal { get; }
    public bool IsInitial { get; }
}