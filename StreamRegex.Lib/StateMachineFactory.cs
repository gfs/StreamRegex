namespace StreamRegex.Lib;

public static class StateMachineFactory
{
    /// <summary>
    /// Create a state machine given a regex pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static StateMachine CreateStateMachine(string pattern)
    {
        Stack<IState> states = new();
        IState initialState = new NopState();
        for (int i = 0; i < pattern.Length; i++)
        {
            IState? toAdd;
            if (char.IsLetterOrDigit(pattern[i]))
            {
                toAdd = new ExactCharacterState(pattern[i]);
            }
            else if (pattern[i] == '\\')
            {
                if (++i < pattern.Length)
                {
                    toAdd = new ExactCharacterState(pattern[i]);
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
                    toAdd = new ComponentGroupState(pattern.Substring(i+1,scoopedCharacters - i));
                    i = scoopedCharacters;
                }
                else
                {
                    throw new ArgumentException("Invalid expression. Missing ']' to close group.");
                }
            }
            else if (pattern[i].Equals('.'))
            {
                toAdd = new AnyCharacterState();
            }
            else if (pattern[i].Equals('*'))
            {
                var last = states.Pop();
                toAdd = new RepeatCharacterZeroPlusState(last);
            }
            else if (pattern[i].Equals('+'))
            {
                var last = states.Peek();
                toAdd = new RepeatCharacterZeroPlusState(last);
            }
            else
            {
                throw new ArgumentException($"Invalid expression. {pattern[i]} is not currently supported.");
            }
            
            if (!states.Any())
            {
                initialState = toAdd;
            }
            else
            {
                var peek = states.Peek();
                peek.Success = toAdd;
            }
            toAdd.Failure = initialState;
            states.Push(toAdd);
        }

        states.Peek().Success = new FinalState();
        
        return new StateMachine(states.Reverse().ToList());
    }
}