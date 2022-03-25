using System.Text.RegularExpressions;

namespace StreamRegex.Lib;

public class RepeatCharacterComponent : RegexComponent
{
    private readonly RegexComponent _toRepeat;
    private readonly bool _requireOne;

    public RepeatCharacterComponent(RegexComponent toRepeat, bool requireOne)
    {
        _toRepeat = toRepeat;
        _requireOne = requireOne;
    }

    public override RegexComponentResult IsMatch(char character)
    {
        var result = _toRepeat.IsMatch(character);
        if (result.Overlaps(RegexComponentResult.Passed))
        {
            return RegexComponentResult.Passed | RegexComponentResult.CanProceed | RegexComponentResult.MustProceed;
        }
        else
        {
            if (_requireOne)
            {
                return RegexComponentResult.None;
            }
            else
            {
                return RegexComponentResult.CanProceed | RegexComponentResult.MustProceed;
            }
        }
    }
}