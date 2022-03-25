namespace StreamRegex.Lib.NFA;

public static class NFAStateMachineFactory
{
    /// <summary>
    /// Create a state machine given a regex pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static NFAStreamRegex CreateStateMachine(string pattern)
    {
        INFAState initialState = new InitialNfaState();
        Stack<INFAState> states = new();
        states.Push(initialState);
        for (int i = 0; i < pattern.Length; i++)
        {
            INFAState? toAdd;
            if (char.IsLetterOrDigit(pattern[i]))
            {
                toAdd = new ExactCharacterNFAState(pattern[i]);
            }
            else if (pattern[i] == '\\')
            {
                if (++i < pattern.Length)
                {
                    toAdd = new ExactCharacterNFAState(pattern[i]);
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
                    toAdd = new ComponentGroupNFAState(pattern.Substring(i+1,scoopedCharacters - (i+1)));
                    i = scoopedCharacters;
                }
                else
                {
                    throw new ArgumentException("Invalid expression. Missing ']' to close group.");
                }
            }
            else if (pattern[i].Equals('.'))
            {
                toAdd = new AnyCharacterNfaState();
            }
            else if (pattern[i].Equals('*'))
            {
                var last = states.Pop();
                toAdd = new RepeatCharacterZeroPlusNFAState(last);
            }
            else if (pattern[i].Equals('+'))
            {
                var last = states.Peek();
                toAdd = new RepeatCharacterZeroPlusNFAState(last);
            }
            else if (pattern[i].Equals('?'))
            {
                var last = states.Pop();
                toAdd = new OptionalNfaState(last);
            }
            else
            {
                throw new ArgumentException($"Invalid expression. {pattern[i]} is not currently supported.");
            }
                
            var peek = states.Peek();
            peek.Success = toAdd;
            toAdd.Failure = initialState;
            states.Push(toAdd);
        }
        var finalState = new FinalNFAState();
        states.Peek().Success = finalState;
        states.Push(finalState);
        return new NFAStreamRegex(states.Reverse().ToList());
    }
}