namespace StreamRegex.Lib;

public class ExactCharacterComponent : RegexComponent
{
    private readonly char _characterToMatch;
    public ExactCharacterComponent(char characterToMatch)
    {
        _characterToMatch = characterToMatch;
    }

    public override (bool doesMatch, bool canAdvance, bool mustAdvance) IsMatch(char character)
    {
        bool result = character.Equals(_characterToMatch);
        return (result, result, result);
    }
}