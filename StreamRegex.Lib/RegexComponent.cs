namespace StreamRegex.Lib;

public abstract class RegexComponent
{
    public abstract RegexComponentResult IsMatch(char character);
}