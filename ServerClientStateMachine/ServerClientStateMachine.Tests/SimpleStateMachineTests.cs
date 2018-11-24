using System;
using Xunit;

namespace ServerClientStateMachine.Tests
{

    public class SimpleStateMachineTests
    {
        public enum States
        {
            Idle,
            Running,
            EndResult,
            Stopped
        }

        [Fact]
        public void SimpleTransition()
        {
            var builder = new StateMachineBuilder<States>()
                .ServerPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult)
                .ServerPermit(States.EndResult, States.Stopped)
                .ServerPermit(States.Running, States.Stopped)
                .ServerPermit(States.Idle, States.Stopped);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            Assert.Equal(States.Idle, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            StateMachine<States>.Sync(server, client);

            Assert.Equal(States.Idle, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            server.SetState(States.Running);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            StateMachine<States>.Sync(server, client);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Running, client.State);
            Assert.Equal(States.Running, client.RemoteState);

            StateMachine<States>.Sync(server, client);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Running, server.RemoteState);
            Assert.Equal(States.Running, client.State);
            Assert.Equal(States.Running, client.RemoteState);


        }
    }
}
