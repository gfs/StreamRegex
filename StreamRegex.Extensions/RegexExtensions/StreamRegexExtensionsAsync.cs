using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StreamRegex.Extensions.Core;

namespace StreamRegex.Extensions.RegexExtensions;

/// <summary>
/// Extends the <see cref="Regex"/> class with functionality to <see cref="Regex"/> asynchronously against <see cref="Stream"/> and <see cref="StreamReader"/>.
/// </summary>
public static class StreamRegexExtensionsAsync
{
    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches any of the <see cref="IEnumerable{Regex}"/>.
    /// <see cref="RegexCache"/> is a recommended to use with this extension method.
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match otherwise false.</returns>
    public static async Task<bool> IsMatchAsync(this IEnumerable<Regex> engines, StreamReader streamReaderToMatch, StreamRegexOptions? options = null)
    {
        var methods = new RegexMethods(engines);
        return await streamReaderToMatch.IsMatchAsync(methods.RegexIsMatchFunction, options);
    }

    /// <summary>
    /// Find if a <see cref="Stream"/> matches any of the <see cref="IEnumerable{Regex}"/>.
    /// <see cref="RegexCache"/> is a recommended to use with this extension method.
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="streamToMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static async Task<bool> IsMatchAsync(this IEnumerable<Regex> engines, Stream streamToMatch, StreamRegexOptions? options = null)
    {
        var reader = new StreamReader(streamToMatch);
        return await engines.IsMatchAsync(reader, options);
    }

    /// <summary>
    /// Find if a <see cref="Stream"/> matches a <see cref="Regex"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="streamToMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>True if there is at least one match.</returns>
    public static async Task<bool> IsMatchAsync(this Regex engine, Stream streamToMatch, StreamRegexOptions? options = null)
    {
        var reader = new StreamReader(streamToMatch);
        return await engine.IsMatchAsync(reader, options);
    }

    /// <summary>
    /// Find if a <see cref="StreamReader"/> matches a regular expression.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match, or lack of match.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static async Task<bool> IsMatchAsync(this Regex engine, StreamReader streamReaderToMatch, StreamRegexOptions? options = null)
    {
        var engines = new[]
        {
            engine
        };
        return await engines.IsMatchAsync(streamReaderToMatch, options);
    }

    /// <summary>
    /// Find the first <see cref="StreamRegexMatch"/> for a <see cref="Regex"/> in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="streamToMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match, or lack of match.</returns>
    public static async Task<StreamRegexMatch> GetFirstMatchAsync(this Regex engine, Stream streamToMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(streamToMatch);
        return await engine.GetFirstMatchAsync(reader, options);
    }

    /// <summary>
    /// Find the first <see cref="StreamRegexMatch"/> for any of the <see cref="IEnumerable{Regex}"/> in a <see cref="Stream"/>.
    /// <see cref="RegexCache"/> is a recommended to use with this extension method.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to operate with</param>
    /// <param name="streamToMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match, or lack of match.</returns>
    public static async Task<StreamRegexMatch> GetFirstMatchAsync(this IEnumerable<Regex> engines, Stream streamToMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(streamToMatch);
        return await engines.GetFirstMatchAsync(reader, options);
    }

    /// <summary>
    /// Find the first <see cref="StreamRegexMatch"/> for a <see cref="Regex"/> in a <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match, or lack of match.</returns>
    public static async Task<StreamRegexMatch> GetFirstMatchAsync(this Regex engine, StreamReader streamReaderToMatch, StreamRegexOptions? options = null)
    {
        return await new[] { engine }.GetFirstMatchAsync(streamReaderToMatch, options);
    }

    /// <summary>
    /// Find the first <see cref="StreamRegexMatch"/> for any of the <see cref="IEnumerable{Regex}"/> in a <see cref="StreamReader"/>.
    /// <see cref="RegexCache"/> is a recommended to use with this extension method.
    /// Find the first <see cref="StreamRegexMatch"/> for a <see cref="Regex"/> in a <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="engines">The <see cref="IEnumerable{Regex}"/> to operate with</param>
    /// <param name="streamReaderToMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match, or lack of match.</returns>
    public static async Task<StreamRegexMatch> GetFirstMatchAsync(this IEnumerable<Regex> engines, StreamReader streamReaderToMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return (StreamRegexMatch)await streamReaderToMatch.GetFirstMatchAsync(methods.RegexGetFirstMatchFunction);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> object representing all matches. This object will be empty if there are no matches.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        return await new[] { engine }.GetMatchCollectionAsync(new StreamReader(toMatch), options);
    }

    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> object representing all matches. This object will be empty if there are no matches.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return await new[] { engine }.GetMatchCollectionAsync(toMatch, options);
    }

    /// <summary>
    /// Find all matches for the engines in the set of regexes, for example a <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The Regexes to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use.</param>
    /// <returns>A <see cref="SlidingBufferMatchCollection{StreamRegexMatch}"/> containing unique matches. This object will be empty if there are no matches.</returns>
    public static async Task<SlidingBufferMatchCollection<SlidingBufferMatch>> GetMatchCollectionAsync(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return await toMatch.GetMatchCollectionAsync(methods.RegexGetMatchCollectionFunction, options);
    }
}