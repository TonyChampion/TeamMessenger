using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;

namespace TeamMessenger
{
    public enum SessionCreationResult
    {
        Success,
        PermissionError,
        TooManySessions,
        Failure
    }

    public class MessageReceivedEventArgs
    {
        public RemoteSystemSessionParticipant Participant { get; set; }
        public string Message { get; set; }
    }

    public class RemoteSessionManager
    {
        private RemoteSystemSessionController _controller;
        private RemoteSystemSession _currentSession;
        private RemoteSystemSessionWatcher _watcher;
        private RemoteSystemSessionMessageChannel _messageChannel;
        private RemoteSystemSessionParticipant _host;
        private RemoteSystemSessionParticipantWatcher _participantWatcher;

        public event EventHandler<RemoteSystemSessionParticipant> ParticipantJoined = delegate { };
        public event EventHandler<RemoteSystemSessionDisconnectedEventArgs> SessionDisconnected = delegate { };
        public event EventHandler<RemoteSystemSessionInfo> SessionAdded = delegate { };
        public event EventHandler<RemoteSystemSessionInfo> SessionRemoved = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<RemoteSystemSessionParticipant> ParticipantAdded = delegate { };
        public event EventHandler<RemoteSystemSessionParticipant> ParticipantRemoved = delegate { };

        public bool IsHost { get; private set; }
        public RemoteSystemSessionParticipant CurrentUser { get; private set; }

        public ObservableCollection<RemoteSystemSessionParticipant> Participants { get; } = new ObservableCollection<RemoteSystemSessionParticipant>();

        private async void OnJoinRequested(RemoteSystemSessionController sender, RemoteSystemSessionJoinRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            args.JoinRequest.Accept();
            ParticipantJoined(this, args.JoinRequest.Participant);

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () => { Participants.Add(args.JoinRequest.Participant); });
            
            deferral.Complete();
        }

        public async Task<SessionCreationResult> CreateSession(string name)
        {
            SessionCreationResult status = SessionCreationResult.Success;

            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus != RemoteSystemAccessStatus.Allowed)
            {
                return SessionCreationResult.PermissionError;
            }

            if (_controller == null)
            {
                _controller = new RemoteSystemSessionController(name);
                _controller.JoinRequested += OnJoinRequested;
            }


            RemoteSystemSessionCreationResult createResult = await _controller.CreateSessionAsync();

           
            if (createResult.Status == RemoteSystemSessionCreationStatus.Success)
            {
                _currentSession = createResult.Session;
                
                _currentSession.Disconnected += (sender, args) =>
                {
                    SessionDisconnected(sender, args);
                };

                InitParticipantWatcher();

                IsHost = true;
            }
            else if(createResult.Status == RemoteSystemSessionCreationStatus.SessionLimitsExceeded)
            {
                status = SessionCreationResult.TooManySessions;
            } else
            {
                status = SessionCreationResult.Failure;
            }

            return status;
        }

        private void InitParticipantWatcher()
        {
            _participantWatcher = _currentSession.CreateParticipantWatcher();
            _participantWatcher.Added += OnParticipantAdded;
            _participantWatcher.Removed += OnParticipantRemoved;
        }

        private void OnParticipantAdded(RemoteSystemSessionParticipantWatcher watcher, RemoteSystemSessionParticipantAddedEventArgs args)
        {
            if(args.Participant.RemoteSystem.DisplayName == _currentSession.ControllerDisplayName)
            {
                _host = args.Participant;
            }

            ParticipantAdded(this, args.Participant);
        }

        private void OnParticipantRemoved(RemoteSystemSessionParticipantWatcher watcher, RemoteSystemSessionParticipantRemovedEventArgs args)
        {
            ParticipantRemoved(this, args.Participant);
        }

        public void StartReceivingMessages()
        {
            _messageChannel = new RemoteSystemSessionMessageChannel(_currentSession, "SessionActions");
            _messageChannel.ValueSetReceived += OnValueSetReceived;
        }

        private void OnValueSetReceived(RemoteSystemSessionMessageChannel sender, RemoteSystemSessionValueSetReceivedEventArgs args)
        {
            MessageReceived(this, new MessageReceivedEventArgs() {
                Participant = args.Sender,
                Message = args.Message["msg"].ToString() });
        }

        public async Task<bool> DiscoverSessions()
        {
            RemoteSystemAccessStatus status = await RemoteSystem.RequestAccessAsync();
            if (status != RemoteSystemAccessStatus.Allowed)
            {
                return false;
            }

            _watcher = RemoteSystemSession.CreateWatcher();
            _watcher.Added += (sender, args) =>
            {
                SessionAdded(sender, args.SessionInfo);
            };

            _watcher.Removed += (sender, args) =>
            {
                SessionRemoved(sender, args.SessionInfo);
            };
            
            _watcher.Start();

            return true;
        }

        public async Task<bool> JoinSession(RemoteSystemSessionInfo session, string name)
        {
            bool status = true;

            RemoteSystemSessionJoinResult joinResult = await session.JoinAsync();
          
            if (joinResult.Status == RemoteSystemSessionJoinStatus.Success)
            {
                _currentSession = joinResult.Session;
            }
            else
            {
                status = false;
            }

            return status;
        }

        public async Task<bool> BroadCastMessage(string message)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                byte[] data = stream.ToArray();

                ValueSet msg = new ValueSet();
                msg.Add("msg", data);
                await _messageChannel.BroadcastValueSetAsync(msg);
            }

            return true;
        }

        public async Task<bool> SendMessage(object message)
        {

            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                byte[] data = stream.ToArray();

                ValueSet msg = new ValueSet();
                msg.Add("Key", data);
                await _messageChannel.SendValueSetAsync(msg, _host);
            }

            return true;
        }

    }
}
