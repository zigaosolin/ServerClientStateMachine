using System;
using System.Collections.Generic;
using System.Text;

namespace ServerClientStateMachine
{
    internal class StateTransitionRule<TEnum>
        where TEnum : Enum
    {
        public TEnum From { get; }
        public TEnum To { get; }
        public TransitionMatching Matching { get; }

        public StateTransitionRule(TEnum from, TEnum to, TransitionMatching matching)
        {
            From = from;
            To = to;
            Matching = matching;
        }

    }
}
