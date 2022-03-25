namespace StreamRegex.Lib;

public class ExactCharacterState : BaseState
{
    private readonly char _character;
    public ExactCharacterState(char character)
    {
        _character = character;
    }
    public override IState Transition(char character)
    {
        return Accepts(character) ? Success : Failure;
    }
    public override bool Accepts(char character)
    {
        return character.Equals(_character);
    }

    public override bool IsFinal { get; } = false;
}