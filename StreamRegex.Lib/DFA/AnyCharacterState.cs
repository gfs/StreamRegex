namespace StreamRegex.Lib;

public class AnyCharacterState : BaseState
{
    public override bool Accepts(char character)
    {
        return true; // TODO: There are certain things . does not match
    }
    public override IState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }

    public override bool IsFinal { get; } = false;
}