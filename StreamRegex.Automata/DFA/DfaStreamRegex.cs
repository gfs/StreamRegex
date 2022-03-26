using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StreamRegex.Lib.DFA;

public class DfaStreamRegex : IStreamRegex
{
    private List<IDfaState> _states;
    private int _bufferSize = 4096;
    private readonly ILogger _logger;

    // TODO: Add method to validate state machine can finish
    internal DfaStreamRegex(List<IDfaState> states, ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<DfaStreamRegex>() ?? NullLogger<DfaStreamRegex>.Instance;
        _states = states;
    }

    public bool ValidateStateMachine()
    {
        if (!_states.Any(x => x.IsFinal))
        {
            _logger.LogError("State machine does not contain a final state.");
            return false;
        }
        
        var states = _states.Where(x => !x.IsFinal).Select(x => x.Success);;
        bool tooLoopy = true;
        // The minimum viable match should be at most N state transitions
        for (int i = 0; i < _states.Count; i++)
        {
            if (states.Any(x => x.IsFinal))
            {
                tooLoopy = false;
                break;
            }
            states = states.Select(x => x.Success);
        }

        if (tooLoopy)
        {
            _logger.LogError("The state machine does not appear to terminate.");
            return false;
        }

        return true;
    }
    public bool IsMatch(Stream toMatch)
    {
        return GetFirstMatchPosition(toMatch) != -1;
    }
    
    public StreamRegexMatch? Match(Stream toMatch)
    {
        var curState = _states[0]; 
        byte[] buffer = new byte[_bufferSize];
        long resultPosition = toMatch.Position;
        long counter = 0;
        var numBytes = toMatch.Read(buffer, 0, _bufferSize);
        StringBuilder match = new();
        while (numBytes > 0)
        {
            for (int byteNum = 0; byteNum < numBytes; byteNum++)
            {
                char character = (char) buffer[byteNum];
                counter++;
                var nextState = curState.Transition(character);
                if (nextState == _states[0] && !curState.Accepts(character))
                {
                    resultPosition += counter;
                    counter = 0;
                    match.Clear();
                }
                else
                {
                    match.Append(character);
                }
                curState = nextState;
                if (curState.IsFinal)
                {
                    return new StreamRegexMatch(match.ToString(), resultPosition);
                }
            }

            numBytes = toMatch.Read(buffer, 0, _bufferSize);
        }

        return null;
    }
    
    public long GetFirstMatchPosition(Stream toMatch)
    {
        return Match(toMatch)?.position ?? -1;
    }
}