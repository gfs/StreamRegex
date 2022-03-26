namespace StreamRegex.Lib.NFA;

/// <summary>
/// Accepts exactly one character.
/// </summary>
public class ExactCharacterNfaState : BaseNfaState
{
    private readonly char _character;
    public ExactCharacterNfaState(char character)
    {
        _character = character;
    }
    public override INfaState[] Transition(char character) => DefaultTransition(character);

    public override bool Accepts(char character)
    {
        return character.Equals(_character);
    }
}