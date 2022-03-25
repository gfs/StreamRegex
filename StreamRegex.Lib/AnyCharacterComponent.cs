namespace StreamRegex.Lib;

public class AnyCharacterComponent : RegexComponent
{
    public AnyCharacterComponent()
    {
    }

    public override (bool doesMatch, bool canAdvance, bool mustAdvance) IsMatch(char character)
    {
        return (true, true, true);
    }
}