namespace StreamRegex.Lib;

public interface IStreamRegex
{
    public bool ValidateStateMachine();
    public StreamRegexMatch? Match(Stream toMatch);
    public long GetFirstMatchPosition(Stream toMatch);
    public bool IsMatch(Stream toMatch);
}