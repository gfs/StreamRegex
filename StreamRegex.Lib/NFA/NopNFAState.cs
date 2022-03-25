namespace StreamRegex.Lib.NFA;

public class NopNFAState : BaseNFAState
{
    public NopNFAState()
    {
        Success = this;
        Failure = this;
    }

    public override IEnumerable<INFAState> Transition(char character)
    {
        yield return this;
    }

    public override bool Accepts(char character) => false;

    public override bool IsFinal { get; } = false;
}