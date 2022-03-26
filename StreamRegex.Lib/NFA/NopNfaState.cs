namespace StreamRegex.Lib.NFA;

/// <summary>
/// No-op NFA state.
/// </summary>
public class NopNfaState : BaseNfaState
{
    public static readonly NopNfaState Instance = new NopNfaState();

    private NopNfaState()
    {
        Success = this;
        Failure = this;
    }

    public override INfaState[] Transition(char character) => DefaultTransition(character);

    public override bool Accepts(char character) => false;
}