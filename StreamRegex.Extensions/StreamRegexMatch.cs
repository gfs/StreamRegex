﻿using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Holds a from a <see cref="Regex"/>.
/// </summary>
/// <param name="Engine">The <see cref="Regex"/> which produced this match, or null if <paramref name="Success"/> is false</param>
/// <inheritdoc cref="SlidingBufferMatch"/>
public record StreamRegexMatch(Regex? Engine = null, bool Success = false, long Index = -1, string? Value = null) : SlidingBufferMatch(Success, Index, Value)
{
    /// <summary>The <see cref="Regex"/> which produced this match, or null if <see cref="Success"/> is false</summary>
    public Regex? Engine { get; } = Engine;
}