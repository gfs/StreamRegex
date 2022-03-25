namespace StreamRegex.Lib;

public class AnyCharacterComponent : RegexComponent
{
    public AnyCharacterComponent()
    {
    }

    public override RegexComponentResult IsMatch(char character)
    {
        return RegexComponentResult.Passed | RegexComponentResult.CanProceed | RegexComponentResult.MustProceed;
    }
}