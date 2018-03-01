using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TeamMessenger.Models;
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
        public object Message { get; set; }
    }

    public class RemoteSessionManager
    {
        private RemoteSystemSessionController _controller;
        private RemoteSystemSession _currentSession;
        private RemoteSystemSessionWatcher _watcher;
        private RemoteSystemSessionMessageChannel _messageChannel;
        private RemoteSystemSessionParticipantWatcher _participantWatcher;

        public event EventHandler<RemoteSystemSessionParticipant> ParticipantJoined = delegate { };
        public event EventHandler<RemoteSystemSessionDisconnectedEventArgs> SessionDisconnected = delegate { };
        public event EventHandler<RemoteSystemSessionInfo> SessionAdded = delegate { };
        public event EventHandler<RemoteSystemSessionInfo> SessionRemoved = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<RemoteSystemSessionParticipant> ParticipantAdded = delegate { };
        public event EventHandler<RemoteSystemSessionParticipant> ParticipantRemoved = delegate { };

        public bool IsHost { get; private set; }
        public User CurrentUser { get; private set; }
        public RemoteSystemSessionParticipant Host { get; private set; }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        private async void OnJoinRequested(RemoteSystemSessionController sender, RemoteSystemSessionJoinRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            args.JoinRequest.Accept();
            ParticipantJoined(this, args.JoinRequest.Participant);

            deferral.Complete();
        }

        public async Task<SessionCreationResult> CreateSession(string sessionName, string displayName)
        {
            SessionCreationResult status = SessionCreationResult.Success;

            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();
            if (accessStatus != RemoteSystemAccessStatus.Allowed)
            {
                return SessionCreationResult.PermissionError;
            }

            if (_controller == null)
            {
                _controller = new RemoteSystemSessionController(sessionName);
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

                CurrentUser = new User() { Id = _currentSession.DisplayName, DisplayName = displayName };
                Users.Add(CurrentUser);

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
            _participantWatcher.Start();
        }

        private void OnParticipantAdded(RemoteSystemSessionParticipantWatcher watcher, RemoteSystemSessionParticipantAddedEventArgs args)
        {
            if(args.Participant.RemoteSystem.DisplayName == _currentSession.ControllerDisplayName)
            {
                Host = args.Participant;
            }

            ParticipantAdded(this, args.Participant);
        }

        private async void OnParticipantRemoved(RemoteSystemSessionParticipantWatcher watcher, RemoteSystemSessionParticipantRemovedEventArgs args)
        {
            var qry = Users.Where(u => u.Id == args.Participant.RemoteSystem.DisplayName);
            if(qry.Count() > 0)
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                    () => { Users.Remove(qry.First()); });
            }
            
            ParticipantRemoved(this, args.Participant);
        }

        public void StartReceivingMessages()
        {
            _messageChannel = new RemoteSystemSessionMessageChannel(_currentSession, "OpenChannel");
            _messageChannel.ValueSetReceived += OnValueSetReceived;
        }

        private object DeserializeMessage(ValueSet valueSet)
        {
            Type serialType;
            object data;

            if(valueSet.ContainsKey("user"))
            {
                serialType = typeof(User);
                data = valueSet["user"];

            } else if (valueSet.ContainsKey("users"))
            {
                serialType = typeof(List<User>);
                data = valueSet["users"];

            } else
            {
                serialType = typeof(UserMessage);
                data = valueSet["message"];
            }

            object value;

            using (var stream = new MemoryStream((byte[])data))
            {
                value = new DataContractJsonSerializer(serialType).ReadObject(stream);
            }

            return value;
        }
        private async void OnValueSetReceived(RemoteSystemSessionMessageChannel sender, RemoteSystemSessionValueSetReceivedEventArgs args)
        {

            var data = DeserializeMessage(args.Message);

            if (data is User)
            {
                var user = data as User;

                if (!Users.Contains(user))
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                        () => { Users.Add(user); });
                }

                await BroadCastMessage("users", Users.ToList());
            }
            else if (data is List<User>)
            {
                var users = data as List<User>;
                Users.Clear();
                foreach(var user in users)
                {
                    Users.Add(user);
                }
            }
            else
            {
                MessageReceived(this, new MessageReceivedEventArgs()
                {
                    Participant = args.Sender,
                    Message = data
                });

            }
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
                CurrentUser = new User() { Id = _currentSession.DisplayName, DisplayName = name };
            }
            else
            {
                status = false;
            }

            InitParticipantWatcher();

            return status;
        }

        public async Task<bool> BroadCastMessage(string key, object message)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                byte[] data = stream.ToArray();

                ValueSet msg = new ValueSet();
                msg.Add(key, data);
                await _messageChannel.BroadcastValueSetAsync(msg);
            }

            return true;
        }

        public async Task<bool> SendMessage(string key, object message, RemoteSystemSessionParticipant participant)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                byte[] data = stream.ToArray();

                ValueSet msg = new ValueSet();
                msg.Add(key, data);
                await _messageChannel.SendValueSetAsync(msg, participant);
            }

            return true;
        }

    }
}
