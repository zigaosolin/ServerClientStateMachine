using System;
using System.Collections.Generic;
using System.Text;

namespace ServerClientStateMachine
{
    public class StateMachineException : Exception
    {
        public StateMachineException()
        {
        }

        public StateMachineException(string message)
            : base(message)
        {
        }
    }
}
