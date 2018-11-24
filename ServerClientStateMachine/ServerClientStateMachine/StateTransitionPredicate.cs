using System;
using System.Collections.Generic;
using System.Text;

namespace ServerClientStateMachine
{
    internal class StateTransitionPredicate<TEnum>
        where TEnum : Enum
    {
        public TEnum From { get; }
        public TEnum To { get; }
        public Func<bool> Predicate { get; }

        public StateTransitionPredicate(TEnum from, TEnum to, Func<bool> action)
        {
            From = from;
            To = to;
            Predicate = action;
        }

    }
}
