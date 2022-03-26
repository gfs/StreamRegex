namespace StreamRegex.Lib;

public interface IState
{
    public bool Accepts(char character);
    public bool IsFinal { get; }
    public bool IsInitial { get; }
}