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

    public override (bool doesMatch, bool canAdvance, bool mustAdvance) IsMatch(char character)
    {
        var result = _toRepeat.IsMatch(character);
        if (result.doesMatch)
        {
            return (true, true, false);
        }
        else
        {
            if (_requireOne)
            {
                return (false, false, false);
            }
            else
            {
                return (false, true, true);
            }
        }
    }
}