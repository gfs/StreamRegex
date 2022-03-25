namespace StreamRegex.Lib.NFA;

public class FinalNFAState : BaseNFAState
{
    public FinalNFAState()
    {
        Success = this;
        Failure = this;          
    }
    public override IEnumerable<INFAState> Transition(char character)
    {
        yield return this;
    }
    public override bool Accepts(char character)
    {
        return true;
    }

    public override bool IsFinal { get; } = true;
}