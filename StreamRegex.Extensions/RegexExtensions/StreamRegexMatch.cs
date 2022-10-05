using System.Text.RegularExpressions;

namespace StreamRegex.Extensions.RegexExtensions;

/// <summary>
/// A sliding buffer match from a <see cref="Regex"/>.
/// </summary>
/// <param name="Engine">The <see cref="Regex"/> which produced this match, or null if <paramref name="Success"/> is false</param>
/// <param name="Success">If them match was successful.</param>
/// <param name="Index">If <paramref name="Success"/> is true, the index in the Stream where the match was found. Otherwise -1.</param>
/// <param name="Length">If <paramref name="Success"/> is true, the length of the match in the Stream Otherwise -1.</param>
/// <param name="Value">The content that was matched, or null if <paramref name="Success"/> is false or if <see cref="DelegateOptions.CaptureValues"/> is false.</param>
public record StreamRegexMatch(Regex? Engine = null, bool Success = false, long Index = -1, long Length = -1, string? Value = null) : SlidingBufferMatch(Success, Index, Length, Value)
{
}