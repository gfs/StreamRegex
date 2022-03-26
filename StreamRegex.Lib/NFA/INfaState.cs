namespace StreamRegex.Lib.NFA;

public interface INfaState : IState
{
    public INfaState[] Transition(char character);
    public INfaState Success { get; set; }
    public INfaState Failure { get; set; }
    public int Index { get; set; }
}