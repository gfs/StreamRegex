namespace StreamRegex.Lib.NFA;

/// <summary>
/// The final state.
/// </summary>
public class FinalNfaState : BaseNfaState
{
    public FinalNfaState()
    {
        Success = this;
        Failure = this;
        IsFinal = true;
    }
    public override INfaState[] Transition(char character) => DefaultTransition(character);

    public override bool Accepts(char character)
    {
        return true;
    }
}