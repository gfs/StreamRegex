using StreamRegex.Lib.DFA;

namespace StreamRegex.Lib;

public class AnyCharacterDfaState : BaseDfaState
{
    public override bool Accepts(char character)
    {
        return true; // TODO: There are certain things . does not match
    }
    public override IDfaState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }

    public override bool IsFinal { get; } = false;
}