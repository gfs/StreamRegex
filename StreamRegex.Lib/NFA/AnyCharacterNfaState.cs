namespace StreamRegex.Lib.NFA;

public class AnyCharacterNfaState : BaseNFAState
{
    /// <summary>
    /// Represents the . operator
    /// </summary>
    public AnyCharacterNfaState()
    {
    }
    public override IEnumerable<INFAState> Transition(char character)
    {
        yield return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return !character.Equals('\n');
    }

    public override bool IsFinal { get; } = false;
}