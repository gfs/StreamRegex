using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRegex.Extensions.StringMethods
{
    internal class StringMatcher
    {
        private string _target;
        private StringComparison _comparison;

        internal StringMatcher(string target, StringComparison stringComparison)
        {
            _target = target;
            _comparison = stringComparison;
        }

        public SlidingBufferMatch GetFirstMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions delegateOptions)
        {
            var idx = chunk.IndexOf(_target, _comparison);
            if (idx != -1)
            {
                return new SlidingBufferMatch(true, idx, idx + _target.Length, delegateOptions.CaptureValues ? _target : null);
            }

            return new SlidingBufferMatch();
        }
        public bool IsMatchDelegate(ReadOnlySpan<char> chunk, DelegateOptions _) => chunk.Contains(_target, _comparison);
    }
}
