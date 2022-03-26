namespace StreamRegex.Lib.NFA;

/// <summary>
/// Represents the . operator. Accepts anything except '\n'.
/// </summary>
public class AnyCharacterNfaState : BaseNfaState
{
    public override INfaState[] Transition(char character) => DefaultTransition(character);
    public override bool Accepts(char character)
    {
        return !character.Equals('\n');
    }
}