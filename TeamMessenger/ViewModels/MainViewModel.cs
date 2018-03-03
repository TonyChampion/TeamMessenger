using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
            App.SessionManager.SessionAdded += OnSessionAdded;
            App.SessionManager.SessionRemoved += OnSessionRemoved;
            await App.SessionManager.DiscoverSessions();
        }

        private async void OnSessionAdded(object sender, RemoteSystemSessionInfo e)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () => { Sessions.Add(e); });
        }

        private async void OnSessionRemoved(object sender, RemoteSystemSessionInfo e)
        {
            if (Sessions.Contains(e))
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                    () => { Sessions.Remove(e); });
            }
        }

        public string JoinName { get; set; }

        public string SessionName { get; set; }

        public object SelectedSession { get; set; }

        public ObservableCollection<RemoteSystemSessionInfo> Sessions { get; } = new ObservableCollection<RemoteSystemSessionInfo>();

        public async void Start()
        {
            if(IsNewSession)
            {
                var result = await App.SessionManager.CreateSession(SessionName, JoinName);
                if(result == SessionCreationResult.Success)
                {
                    SessionConnected(this, null);
                } else
                {
                    ErrorConnecting(this, result);
                }
            } else
            {
                if(SelectedSession != null)
                {
                    var result = await App.SessionManager.JoinSession(SelectedSession as RemoteSystemSessionInfo, JoinName);
                    if(result)
                    {
                        SessionConnected(this, null);
                    } else
                    {
                        ErrorConnecting(this, SessionCreationResult.Failure);
                    }
                }
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
