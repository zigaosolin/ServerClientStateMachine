using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServerClientStateMachine.Tests
{
    public class GameTransitionTests
    {
        public enum States
        {
            NotAvailable,
            Preparation,
            Running,
            Result,
            ResultConfirmed,
            End
        }

        [Fact]
        public void FullStateMachine()
        {
            var builder = new StateMachineBuilder<States>()
                .ServerPermit(States.NotAvailable, States.Preparation)
                .ServerPermit(States.Preparation, States.NotAvailable, TransitionMatching.ClientAndServerCanMismatch)
                .ServerPermit(States.Preparation, States.Running)
                .ClientPermit(States.Running, States.Result)
                .ServerPermit(States.Result, States.ResultConfirmed)
                .ServerPermit(States.Result, States.End)
                .ServerPermit(States.ResultConfirmed, States.End);
                

        }

    }
}
