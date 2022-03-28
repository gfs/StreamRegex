using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamRegex.Extensions;

/// <summary>
/// Extends the <see cref="Regex"/> class with functionality to check against <see cref="StreamReader"/>.
/// </summary>
public static class StreamRegexExtensions
{
    /// <summary>
    /// Find if a StreamReader matches any of the Regex in a set.
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return engines.GetFirstMatch(toMatch, options).Success;
    }

    /// <summary>
    /// Find if a Stream matches any of the Regex in a set.
    /// </summary>
    /// <param name="engines">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this IEnumerable<Regex> engines, Stream toMatch, StreamRegexOptions? options = null)
    {
        return engines.GetFirstMatch(toMatch, options).Success;
    }
    
    /// <summary>
    /// Find if a StreamReader matches a regular expression.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="StreamReader"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return engine.GetFirstMatch(toMatch, options).Success;
    }

    /// <summary>
    /// Find if a Stream matches a regular expression.
    /// </summary>
    /// <param name="engine">The <see cref="Regex"/> to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    /// <returns>True if there is at least one match.</returns>
    public static bool IsMatch(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        return engine.GetFirstMatch(toMatch, options).Success;
    }
    
    /// <summary>
    /// Find the first match for a <see cref="Regex"/> in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    public static StreamRegexMatch GetFirstMatch(this Regex engine, Stream toMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(toMatch);
        return engine.GetFirstMatch(reader, options);
    }
    
    /// <summary>
    /// Find the first match for a <see cref="RegexCache"/> in a <see cref="Stream"/>.
    /// </summary>
    /// <param name="engines">The Regex to operate with</param>
    /// <param name="toMatch">The <see cref="Stream"/> to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    public static StreamRegexMatch GetFirstMatch(this IEnumerable<Regex>? engines, Stream toMatch, StreamRegexOptions? options = null)
    {
        using var reader = new StreamReader(toMatch);
        return engines.GetFirstMatch(reader, options);
    }
    
    /// <summary>
    /// Find if a StreamReader matches a regular expression.
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    public static StreamRegexMatch GetFirstMatch(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[]{engine}.GetFirstMatch(toMatch, options);
    }
    
    /// <summary>
    /// Find if a StreamReader matches a regular expression.
    /// </summary>
    /// <param name="engines">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatch"/> object representing the first match. If there is no match, the <see cref="StreamRegexMatch.Success"/> will be false, the <see cref="StreamRegexMatch.Value"/> will be null.</returns>
    public static StreamRegexMatch GetFirstMatch(this IEnumerable<Regex>? engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return (StreamRegexMatch)toMatch.GetFirstMatch(methods.RegexGetFirstMatchFunction);
    }
    
    /// <summary>
    /// Find all matches for a given <see cref="Regex"/>
    /// </summary>
    /// <param name="engine">The Regex to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use</param>
    /// <returns>A <see cref="StreamRegexMatchCollection"/> object representing all matches. This object will be empty if there are no matches.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this Regex engine, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        return new[]{engine}.GetMatchCollection(toMatch, options);
    }
    
    /// <summary>
    /// Find all matches for the engines in the set of regexes, for example a <see cref="RegexCache"/>
    /// </summary>
    /// <param name="engines">The Regexes to operate with</param>
    /// <param name="toMatch">The StreamReader to match</param>
    /// <param name="options">The <see cref="StreamRegexOptions"/> to use.</param>
    /// <returns>A <see cref="StreamRegexMatchCollection"/> containing unique matches. This object will be empty if there are no matches.</returns>
    public static SlidingBufferMatchCollection<SlidingBufferMatch> GetMatchCollection(this IEnumerable<Regex> engines, StreamReader toMatch, StreamRegexOptions? options = null)
    {
        RegexMethods methods = new RegexMethods(engines);
        return toMatch.GetMatchCollection(methods.RegexGetMatchCollectionFunction, options);
    }

    internal class RegexMethods
    {
        private readonly RegexCache _engines;

        internal RegexMethods(IEnumerable<Regex> engines)
        {
            _engines = engines is RegexCache cache ? cache : new RegexCache(engines);
        }
        
        public IEnumerable<StreamRegexMatch> RegexGetMatchCollectionFunction(string arg)
        {
            foreach (var engine in _engines)
            {
                MatchCollection matches = engine.Matches(arg);
                foreach (Match match in matches)
                {
                    yield return new StreamRegexMatch(engine, true, match.Index, match.Value);
                }
            }
        }

        public StreamRegexMatch RegexGetFirstMatchFunction(string arg)
        {
            foreach (var engine in _engines)
            {
                var match = engine.Match(arg);
                if (match.Success)
                {
                    return new StreamRegexMatch(engine, true, match.Index, match.Value);
                }
            }

            return new StreamRegexMatch(null);
        }
        
        public bool RegexIsMatchFunction(string arg)
        {
            foreach (var engine in _engines)
            {
                var match = engine.Match(arg);
                if (match.Success)
                {
                    return true;
                }
            }

            return false;
        }
    }
}