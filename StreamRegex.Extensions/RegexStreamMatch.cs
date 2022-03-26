using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Holds a match.
/// </summary>
/// <param name="Engine">The <see cref="Regex"/> which produced this match</param>
/// <param name="Matches">If the match was successful</param>
/// <param name="MatchPosition">The position offset in the stream relative to the start when provided</param>
/// <param name="MatchContent">The content that was matched by the <paramref name="Engine"/></param>
public record RegexStreamMatch(Regex Engine, bool Matches = false, long MatchPosition = -1, string? MatchContent = null)
{
    public long MatchPosition { get; internal set; } = MatchPosition;
}