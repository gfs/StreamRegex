using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamRegex.Lib;

public class RegexStreamMatcher
{
    private const int _bufferSize = 8192;
    public RegexStreamMatcher()
    {
    }
    
    private List<RegexComponent> ParsePattern(string pattern)
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

        return stack.Reverse().ToList();
    }
    
    public long GetFirstMatchPosition(string pattern, Stream stream)
    {
        byte[] buffer = new byte[_bufferSize];
        var components = ParsePattern(pattern);
        long resultPosition = stream.Position;
        bool[] defaultPositions = Enumerable.Repeat(false, components.Count).ToArray();
        defaultPositions[0] = true;
        BitArray regexPositions = new BitArray(defaultPositions);
        BitArray newPositions = new BitArray(defaultPositions);
        
        var numBytes = stream.Read(buffer, 0,_bufferSize);
        while (numBytes != -1)
        {
            foreach (byte character in buffer)
            {
                resultPosition++;
                for (int i = 0; i < components.Count; i++)
                {
                    if (!regexPositions.Get(i)) continue;
                    if (!SetNextPositions(components, i, (char) character, newPositions))
                    {
                        return resultPosition;
                    }
                }

                (regexPositions, newPositions) = (newPositions, regexPositions);
                // newPositions.SetAll(false);
            }
            numBytes = stream.Read(buffer, 0,_bufferSize);
        }
        return -1;
    }
    
    private bool SetNextPositions(List<RegexComponent> components, int position, char character, BitArray positionArray)
    {
        var result = components[position].IsMatch(character);
        if (result.Overlaps(RegexComponentResult.Passed))
        {
            if (result.Overlaps(RegexComponentResult.CanProceed))
            {
                if (position + 1 == components.Count)
                {
                    return false;
                }
                if (!result.Overlaps(RegexComponentResult.MustProceed))
                {
                    positionArray.Set(position, true);
                }

                positionArray.Set(position + 1, true);
            }
            else
            {
                // It doesn't match, but we may advance the regex
                // Our new positions should thus be the positions based on the next component
                SetNextPositions(components, position + 1, character, positionArray);
            }
        }
        positionArray.Set(0, true);
        return true;
    }
}