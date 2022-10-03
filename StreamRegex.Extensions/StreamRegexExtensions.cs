using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Extends the <see cref="Regex"/> class with functionality to check against <see cref="StreamReader"/>.
/// </summary>
public static class StreamRegexExtensions
{
    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches any of the <see cref="Regex"/> in a set.
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        var regexActions = new RegexMethods(engines);
        return toMatch.IsMatch(regexActions.RegexIsMatchFunction);
    }

    /// <summary>
    /// Find if a <see cref="Stream"/> matches any of the <see cref="Regex"/> in a set.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to check against the <see cref="Stream"/></param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this IEnumerable<Regex> engines, Stream toMatch, StreamRegexOptions? options = null)
    {
        var regexActions = new RegexMethods(engines);
        return toMatch.IsMatch(regexActions.RegexIsMatchFunction);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches a <see cref="Regex"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {

        return new[] {engine}.IsMatch(toMatch, options);
    }

    /// <summary>
    /// Find if a Stream matches a regular expression.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        return new[] {engine}.IsMatch(toMatch, options);
    }

    /// <summary>
    /// Find the first match for a <see cref="Regex"/> in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/>  to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexMatch GetFirstMatch(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(toMatch);
        return engine.GetFirstMatch(reader, options);
    }

    /// <summary>
    /// Find the first match for a <see cref="IEnumerable{Regex}"/> (for example, <seealso cref="RegexCache"/>) in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to check against the <see cref="Stream"/></param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexMatch GetFirstMatch(this IEnumerable<Regex> engines, Stream toMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(toMatch);
        return engines.GetFirstMatch(reader, options);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches a <see cref="Regex"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexMatch GetFirstMatch(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[]{engine}.GetFirstMatch(toMatch, options);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches any of a number of <see cref="Regex"/>.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to check against the Stream</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexMatch GetFirstMatch(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return (StreamRegexMatch)toMatch.GetFirstMatch(methods.RegexGetFirstMatchFunction);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> object representing all matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        return new[]{engine}.GetMatchCollection(new StreamReader(toMatch), options);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> object representing all matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[]{engine}.GetMatchCollection(toMatch, options);
    }

    /// <summary>
    /// Find all matches for the engines in the set of regexes, for example a <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use.</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> containing unique matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return toMatch.GetMatchCollection(methods.RegexGetMatchCollectionFunction, options);
    }
}