using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using StreamRegex.Extensions.Core;
using StreamRegex.Extensions.RegexExtensions;

namespace StreamRegex.Extensions.RegexExtensions;

/// <summary>
/// Extends the <see cref="Regex"/> class with functionality to check against <see cref="StreamReader"/> and <see cref="Stream"/>.
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
        return toMatch.IsMatch(regexActions.RegexIsMatchDelegate, options);
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
        return toMatch.IsMatch(regexActions.RegexIsMatchDelegate, options);
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
        return new[] { engine }.IsMatch(toMatch, options);
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
        return new[] { engine }.IsMatch(toMatch, options);
    }

    /// <summary>
    /// Find the first match for a <see cref="Regex"/> in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/>  to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexValueMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexValueMatch GetFirstMatch(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(toMatch, Encoding.UTF8, true, 4096, true);
        return engine.GetFirstMatch(reader, options);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches a <see cref="Regex"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexValueMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexValueMatch GetFirstMatch(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[] { engine }.GetFirstMatch(toMatch, options);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches any of a number of <see cref="Regex"/>.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to check against the Stream</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexValueMatch"/> object representing the first, or lack of, Match.</returns>
    public static StreamRegexValueMatch GetFirstMatch(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        if (toMatch.GetFirstMatch(methods.RegexGetFirstMatchDelegate, options) is StreamRegexValueMatch srvm)
        {
            return srvm;
        }

        return new StreamRegexValueMatch();
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexValueMatch}"/> object representing all matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferValueMatchCollection<StreamRegexValueMatch> GetMatchCollection(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        return new[] { engine }.GetMatchCollection(new StreamReader(toMatch), options);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexValueMatch}"/> object representing all matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferValueMatchCollection<StreamRegexValueMatch> GetMatchCollection(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[] { engine }.GetMatchCollection(toMatch, options);
    }

    /// <summary>
    /// Find all matches for the engines in the set of regexes, for example a <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use.</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexValueMatch}"/> containing unique matches. This collection will be empty if there are no matches.</returns>
    public static SlidingBufferValueMatchCollection<StreamRegexValueMatch> GetMatchCollection(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        SlidingBufferValueMatchCollection<StreamRegexValueMatch> regexMatches = new();
        var untypedCollection = toMatch.GetMatchCollection(methods.RegexGetMatchCollectionDelegate, options);
        foreach(var match in untypedCollection)
        {
            if (match is StreamRegexValueMatch srvm)
            {
                regexMatches.AddMatch(srvm);
            }
        }
        return regexMatches;
    }
}