using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// A sliding buffer value match from a <see cref="Regex"/>.
/// </summary>
/// <param name="Engine">The <see cref="Regex"/> which produced this match, or null if <paramref name="Success"/> is false</param>
/// <param name="Success">If them match was successful.</param>
/// <param name="Index">If <paramref name="Success"/> is true, the index in the Stream where the match was found. Otherwise -1.</param>
/// <param name="Length">If <paramref name="Success"/> is true, the length of the value in the Stream that was matched. Otherwise -1.</param>
public record StreamRegexValueMatch(Regex? Engine = null, bool Success = false, long Index = -1, long Length = -1) : SlidingBufferValueMatch(Success, Index, Length)
{
}