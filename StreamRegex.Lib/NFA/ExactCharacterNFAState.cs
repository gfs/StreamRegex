namespace StreamRegex.Lib.NFA;

public class ExactCharacterNFAState : BaseNFAState
{
    private readonly char _character;
    public ExactCharacterNFAState(char character)
    {
        _character = character;
    }
    public override IEnumerable<INFAState> Transition(char character)
    {
        yield return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return character.Equals(_character);
    }

    public override bool IsFinal { get; } = false;
}