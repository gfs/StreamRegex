namespace StreamRegex.Lib.NFA;

/// <summary>
/// The initial state used for base case testing. Passes Transition and Accepts to its Success param.
/// </summary>
public class InitialNfaState : BaseNfaState
{
    public InitialNfaState()
    {
        IsInitial = true;
    }

    public override INfaState[] Transition(char character) => Success.Transition(character);
    public override bool Accepts(char character) => Success.Accepts(character);
}