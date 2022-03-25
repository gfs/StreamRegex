namespace StreamRegex.Lib;

public abstract class BaseState : IState
{
    public abstract bool Accepts(char character);
    public abstract IState Transition(char character);
    public abstract bool IsFinal { get; }
    public IState Success { get; set; }
    public IState Failure { get; set; }
}