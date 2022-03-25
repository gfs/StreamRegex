namespace StreamRegex.Lib.NFA;

public class InitialNfaState : BaseNFAState
{
    public InitialNfaState()
    {      
    }
    public override IEnumerable<INFAState> Transition(char character)
    {
        return Success.Transition(character);
    }
    public override bool Accepts(char character)
    {
        return Success.Accepts(character);
    }

    public override bool IsFinal { get; } = false;
}