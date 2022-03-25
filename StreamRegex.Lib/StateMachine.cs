using System.Collections;

namespace StreamRegex.Lib;

public class StateMachine
{
    private List<IState> _states;
    private int _bufferSize = 4096;
    
    public StateMachine(List<IState> states)
    {
        _states = states;
    }

    public long GetMatchPosition(Stream toMatch)
    {
        var curState = _states[0]; 
        byte[] buffer = new byte[_bufferSize];
        long resultPosition = toMatch.Position;
        long counter = 0;
        var numBytes = toMatch.Read(buffer, 0, _bufferSize);
        while (numBytes != -1)
        {
            foreach (byte character in buffer)
            {
                counter++;
                var nextState = curState.Transition((char) character);
                if (nextState == _states[0] && !curState.Accepts((char)character))
                {
                    resultPosition += counter;
                    counter = 0;
                }
                curState = nextState;
                if (curState.IsFinal)
                {
                    return resultPosition;
                }
            }

            numBytes = toMatch.Read(buffer, 0, _bufferSize);
        }

        return -1;
    }
}