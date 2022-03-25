namespace StreamRegex.Lib.NFA;

public interface INFAState
{
    public bool Accepts(char character);
    public IEnumerable<INFAState> Transition(char character);
    public bool IsFinal { get; }
    public INFAState Success { get; set; }
    public INFAState Failure { get; set; }
}