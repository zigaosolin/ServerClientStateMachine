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
        public void SimpleTransition_Server()
        {
            var builder = new StateMachineBuilder<States>()
                .ServerPermit(States.Idle, States.Running);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            Assert.Equal(States.Idle, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.Idle, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            server.SetState(States.Running);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Idle, client.State);
            Assert.Equal(States.Idle, client.RemoteState);

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Idle, server.RemoteState);
            Assert.Equal(States.Running, client.State);
            Assert.Equal(States.Running, client.RemoteState);

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.Running, server.State);
            Assert.Equal(States.Running, server.RemoteState);
            Assert.Equal(States.Running, client.State);
            Assert.Equal(States.Running, client.RemoteState);
        }

        [Fact]
        public void SimpleTransition_Client()
        {
            var builder = new StateMachineBuilder<States>()
                .ServerPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            StateTransitionFailReason reason;
            Assert.False(client.TrySetState(States.Running, out reason));
            Assert.Equal(StateTransitionFailReason.NoRule, reason);

            server.SetState(States.Running);

            // Client still not in running state here
            Assert.False(client.TrySetState(States.Running, out reason));

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.False(server.TrySetState(States.EndResult, out reason));
            Assert.Equal(StateTransitionFailReason.NoRule, reason);

            // Client is in running here
            Assert.True(client.TrySetState(States.EndResult, out reason));
            Assert.Equal(StateTransitionFailReason.Success, reason);

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.EndResult, server.State);
            Assert.Equal(States.EndResult, server.RemoteState);
            Assert.Equal(States.EndResult, client.State);
            Assert.Equal(States.EndResult, client.RemoteState);

        }

        [Fact]
        public void SimpleTransition_FailMismatchStates()
        {
            var builder = new StateMachineBuilder<States>()
                .ServerPermit(States.Idle, States.Running)
                .ClientPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            client.SetState(States.Running);

            // client running, server idle
            StateTransitionFailReason reason;
            Assert.False(client.TrySetState(States.EndResult, out reason));
            Assert.Equal(StateTransitionFailReason.RuleServerClientStateMismatch, reason);

            // server syncs immediatelly
            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.True(client.TrySetState(States.EndResult));

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.EndResult, server.State);
        }

        [Fact]
        public void SimpleTransition_AllowMismatchStates()
        {
            var builder = new StateMachineBuilder<States>()
                .ClientPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult, TransitionMatching.ClientAndServerCanMismatch);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            client.SetState(States.Running);

            // Can set the state here as there is rule for mismatch
            Assert.True(client.TrySetState(States.EndResult));

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            // There are no direct transitions, so server won't match
            Assert.Equal(States.Idle, server.State);
        }

        [Fact]
        public void SimpleTransition_AllowMismatchStates_WithDirectTransition()
        {
            var builder = new StateMachineBuilder<States>()
                .ClientPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult, TransitionMatching.ClientAndServerCanMismatch)
                .ClientPermit(States.Idle, States.EndResult);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            client.SetState(States.Running);

            // Can set the state here as there is rule for mismatch
            Assert.True(client.TrySetState(States.EndResult));

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            // Direct transition available (server never gets Running State)
            Assert.Equal(States.EndResult, server.State);
        }

        [Fact]
        public void SimpleTransition_MustMismatchStates()
        {
            var builder = new StateMachineBuilder<States>()
                .ClientPermit(States.Idle, States.Running)
                .ClientPermit(States.Running, States.EndResult, TransitionMatching.ClientAndServerMismatch)
                .ClientPermit(States.Idle, States.EndResult);

            var client = builder.BuildClient();
            var server = builder.BuildServer();

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            client.SetState(States.Running);

            Assert.True(client.TrySetState(States.EndResult));

            server.ReportRemoteState(client.State);
            client.ReportRemoteState(server.State);

            Assert.Equal(States.EndResult, server.State);
        }

        
    }
}
