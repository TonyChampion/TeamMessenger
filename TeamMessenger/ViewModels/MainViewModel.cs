using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.RemoteSystems;

namespace TeamMessenger.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            _initSessionManager = InitSessionManager();
        }

        private Task _initSessionManager;

        private async Task InitSessionManager()
        {
            App.SessionManager.SessionAdded += SessionManager_SessionAdded;
            await App.SessionManager.DiscoverSessions();
            App.SessionManager.ParticipantAdded += SessionManager_ParticipantAdded;

        }

        private void SessionManager_ParticipantAdded(object sender, RemoteSystemSessionParticipant e)
        {
            
        }

        private void SessionManager_SessionAdded(object sender, RemoteSystemSessionInfo e)
        {
            Sessions.Add(e);
        }

        public ObservableCollection<RemoteSystemSessionInfo> Sessions { get; } = new ObservableCollection<RemoteSystemSessionInfo>();
        public string JoinName { get; set; }

        public string SessionName { get; set; }

        public RemoteSystemSession SelectedSession { get; set; }
        public async void Start()
        {
            if(IsNewSession)
            {
                var result = await App.SessionManager.CreateSession(SessionName);
                if(result == SessionCreationResult.Success)
                {
                    SessionConnected(this, null);
                } else
                {
                    ErrorConnecting(this, result);
                }
            } else
            {

            }
        }

        public event EventHandler SessionConnected = delegate { };
        public event EventHandler<SessionCreationResult> ErrorConnecting = delegate { };


        public bool IsNewSession { get; set; } = true;
        public void CreateSession()
        {
            IsNewSession = true;
        }

        public void JoinSession()
        {
            IsNewSession = false;
        }
    }
}
