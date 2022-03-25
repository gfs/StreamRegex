using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamRegex.Lib;

public class RegexStreamReader
{
    private readonly Stream _stream;
    private const int _bufferSize = 4096;
    private readonly Queue<char> _characterQueue = new();
    private readonly ConcurrentDictionary<string, List<RegexComponent>> _regexCache = new();
    public RegexStreamReader(Stream stream)
    {
        _stream = stream;
    }

    public void CompileAndCachePattern(string pattern)
    {
        if (!_regexCache.ContainsKey(pattern))
        {
            _regexCache[pattern] = new List<RegexComponent>(ParsePattern(pattern));
        }
    }
    
    private IEnumerable<RegexComponent> ParsePattern(string pattern)
    {
        Stack<RegexComponent> stack = new();
        for (int i = 0; i < pattern.Length; i++)
        {
            if (char.IsLetterOrDigit(pattern[i]))
            {
                stack.Push(new ExactCharacterComponent(pattern[i]));
            }
            else if (pattern[i] == '\\')
            {
                if (++i < pattern.Length)
                {
                    stack.Push(new ExactCharacterComponent(pattern[i]));
                }
                else
                {
                    throw new ArgumentException("Invalid expression. Escape character ends the string.");
                }
            }
            else if (pattern[i].Equals('['))
            {
                var scoopedCharacters = pattern.IndexOf(']', i);
                if (scoopedCharacters != -1)
                {
                    var scoopedList = new List<RegexComponent>();
                    for (int j = i+1; j < scoopedCharacters; j++)
                    {
                        scoopedList.Add(new ExactCharacterComponent(pattern[j]));
                    }
                    stack.Push(new ComponentGroupComponent(scoopedList));
                    i = scoopedCharacters;
                }
                else
                {
                    throw new ArgumentException("Invalid expression. Missing ']' to close group.");
                }
            }
            else if (pattern[i].Equals('.'))
            {
                stack.Push(new AnyCharacterComponent());
            }
            else if (pattern[i].Equals('*'))
            {
                var last = stack.Pop();
                stack.Push(new RepeatCharacterComponent(last, false));
            }
            else if (pattern[i].Equals('+'))
            {
                var last = stack.Pop();
                stack.Push(new RepeatCharacterComponent(last, true));
            }
            else
            {
                throw new ArgumentException($"Invalid expression. {pattern[i]} is not currently supported.");
            }
        }

        return stack.Reverse();
    }

    private char? GetNextCharacter()
    {
        if (!_characterQueue.Any())
        {
            byte[] buffer = new byte[_bufferSize];
            var numBytes = _stream.Read(buffer, 0,_bufferSize);
            if (numBytes > 0)
            {
                foreach (var character in buffer)
                {
                    _characterQueue.Enqueue((char)character);
                }
            }
        }

        if (!_characterQueue.Any())
        {
            return null;
        }
        return _characterQueue.Dequeue();
    }
    
    public long GetFirstMatchPosition(string pattern)
    {
        CompileAndCachePattern(pattern);

        long resultPosition = _stream.Position;
        HashSet<int> regexPositions = new(){0};
        HashSet<int> newRegexPositions = new(){0};
        while (GetNextCharacter() is char character)
        {
            resultPosition++;
            foreach (int regexPosition in regexPositions)
            {
                var result = GetNextPositions(pattern, regexPosition, character);
                if (!result.Any())
                {
                    return resultPosition;
                }
                newRegexPositions.UnionWith(result);
            }

            // Swap new and old to avoid allocating a new hashset
            (regexPositions, newRegexPositions) = (newRegexPositions, regexPositions);
            // Clear the old positions
            newRegexPositions.Clear();
            // Always allow to start over
            newRegexPositions.Add(0);
        }
        return -1;
    }

    private HashSet<int> GetNextPositions(string pattern, int position, char character)
    {
        HashSet<int> newPositions = new() {0};
        var result = _regexCache[pattern][position].IsMatch(character);
        if (result.canAdvance)
        {
            if (result.doesMatch)
            {
                if (position + 1 == _regexCache[pattern].Count)
                {
                    return new();
                }
                if (!result.mustAdvance)
                {
                    newPositions.Add(position);
                }
                newPositions.Add(position + 1);
            }
            else
            {
                // It doesn't match, but we may advance the regex
                // Our new positions should thus be the positions based on the next component
                newPositions = GetNextPositions(pattern, position + 1, character);
            }
        }

        return newPositions;
    }
}