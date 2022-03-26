namespace StreamRegex.Lib.NFA;

/// <summary>
/// Represents a character group inside []
/// </summary>
public class ComponentGroupNfaState : BaseNfaState
{
    private readonly string _characters;
    public ComponentGroupNfaState(string characters)
    {
        _characters = characters;
    }
    public override INfaState[] Transition(char character) => DefaultTransition(character);

    public override bool Accepts(char character)
    {
        return _characters.Contains(character);
    }
}