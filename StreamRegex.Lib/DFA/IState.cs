namespace StreamRegex.Lib;

public interface IState
{
    public bool Accepts(char character);
    public IState Transition(char character);
    public bool IsFinal { get; }
    public IState Success { get; set; }
    public IState Failure { get; set; }
}