namespace StreamRegex.Lib.DFA;

public interface IDfaState : IState
{
    public IDfaState Transition(char character);
    public IDfaState Success { get; set; }
    public IDfaState Failure { get; set; }
}