namespace StreamRegex.Lib;

[Flags]
public enum RegexComponentResult
{
    None = 0,
    Passed = 1,
    CanProceed = 2,
    MustProceed = 4
}