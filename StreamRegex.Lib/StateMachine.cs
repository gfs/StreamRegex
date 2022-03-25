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
        var numBytes = toMatch.Read(buffer, 0, _bufferSize);
        while (numBytes != -1)
        {
            foreach (byte character in buffer)
            {
                resultPosition++;
                var lastState = curState;
                curState = curState.Transition((char)character);
                if (curState.IsFinal)
                {
                    return resultPosition;
                }
            }
        }

        return -1;
    }
}