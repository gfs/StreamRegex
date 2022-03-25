namespace StreamRegex.Lib.NFA;

public class ComponentGroupNFAState : BaseNFAState
{
    private readonly string _characters;
    
    public ComponentGroupNFAState(string characters)
    {
        _characters = characters;
    }
    public override IEnumerable<INFAState> Transition(char character)
    {
        yield return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return _characters.Contains(character);
    }

    public override bool IsFinal { get; } = false;
}