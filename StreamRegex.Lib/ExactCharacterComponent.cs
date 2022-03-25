namespace StreamRegex.Lib;

public class ExactCharacterComponent : RegexComponent
{
    private readonly char _characterToMatch;
    public ExactCharacterComponent(char characterToMatch)
    {
        _characterToMatch = characterToMatch;
    }

    public override RegexComponentResult IsMatch(char character)
    {
        return character.Equals(_characterToMatch) ? RegexComponentResult.Passed | RegexComponentResult.CanProceed | RegexComponentResult.MustProceed : RegexComponentResult.None;
    }
}