using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Holds a match from <see cref="StreamRegex.Lib.RegexStreamExtensions.GetFirstMatch"/>
/// </summary>
public record RegexStreamMatch(Regex Engine, bool Matches = false, long MatchPosition = -1, string? MatchContent = null)
{
    public long MatchPosition { get; set; } = MatchPosition;
}