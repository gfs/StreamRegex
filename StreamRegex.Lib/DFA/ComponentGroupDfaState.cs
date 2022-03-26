using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class ComponentGroupDfaState : BaseDfaState
{
    private readonly string _characters;
    
    public ComponentGroupDfaState(string characters)
    {
        _characters = characters;
    }
    public override IDfaState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return _characters.Contains(character);
    }
}