namespace StreamRegex.Lib.DFA;

public static class StateMachineFactory
{
    /// <summary>
    /// Create a state machine given a regex pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static DfaStreamRegex CreateStateMachine(string pattern)
    {
        Stack<IDfaState> states = new();
        IDfaState initialDfaState = new NopDfaState();
        for (int i = 0; i < pattern.Length; i++)
        {
            IDfaState? toAdd;
            if (char.IsLetterOrDigit(pattern[i]))
            {
                toAdd = new ExactCharacterDfaState(pattern[i]);
            }
            else if (pattern[i] == '\\')
            {
                if (++i < pattern.Length)
                {
                    toAdd = new ExactCharacterDfaState(pattern[i]);
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
                    toAdd = new ComponentGroupDfaState(pattern.Substring(i+1,scoopedCharacters - (i+1)));
                    i = scoopedCharacters;
                }
                else
                {
                    throw new ArgumentException("Invalid expression. Missing ']' to close group.");
                }
            }
            else if (pattern[i].Equals('.'))
            {
                toAdd = new AnyCharacterDfaState();
            }
            else if (pattern[i].Equals('*'))
            {
                var last = states.Pop();
                toAdd = new RepeatCharacterZeroPlusDfaState(last);
            }
            else if (pattern[i].Equals('+'))
            {
                var last = states.Peek();
                toAdd = new RepeatCharacterZeroPlusDfaState(last);
            }
            else
            {
                throw new ArgumentException($"Invalid expression. {pattern[i]} is not currently supported.");
            }
            
            if (!states.Any())
            {
                initialDfaState = toAdd;
            }
            else
            {
                var peek = states.Peek();
                peek.Success = toAdd;
            }
            toAdd.Failure = initialDfaState;
            states.Push(toAdd);
        }
        var finalState = new FinalDfaState();
        states.Peek().Success = finalState;
        states.Push(finalState);
        return new DfaStreamRegex(states.Reverse().ToList());
    }
}