using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class ExactCharacterDfaState : BaseDfaState
{
    private readonly char _character;
    public ExactCharacterDfaState(char character)
    {
        _character = character;
    }
    public override IDfaState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return character.Equals(_character);
    }

    public override bool IsFinal { get; } = false;
}