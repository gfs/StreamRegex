namespace StreamRegex.Lib.NFA;

public abstract class BaseNFAState : INFAState
{
    public abstract bool Accepts(char character);
    public abstract IEnumerable<INFAState> Transition(char character);
    public abstract bool IsFinal { get; }
    public INFAState Success { get; set; }
    public INFAState Failure { get; set; }
}