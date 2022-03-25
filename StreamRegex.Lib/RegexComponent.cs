namespace StreamRegex.Lib;

public abstract class RegexComponent
{
    public abstract (bool doesMatch, bool canAdvance, bool mustAdvance) IsMatch(char character);
}