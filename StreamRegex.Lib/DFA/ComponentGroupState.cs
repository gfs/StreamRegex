namespace StreamRegex.Lib;

public class ComponentGroupState : BaseState
{
    private readonly string _characters;
    
    public ComponentGroupState(string characters)
    {
        _characters = characters;
    }
    public override IState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return _characters.Contains(character);
    }

    public override bool IsFinal { get; } = false;
}