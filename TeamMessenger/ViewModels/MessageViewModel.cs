using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamMessenger.Models;
using Windows.System.RemoteSystems;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TeamMessenger.ViewModels
{
    public class MessageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event EventHandler MessageAdded = delegate { };


        public ObservableCollection<UserMessage> Messages { get; private set; }

        public ObservableCollection<User> Users { get; private set; }

        private string _newMessage;
        public string NewMessage {
            get { return _newMessage; }
            set
            {
                _newMessage = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(NewMessage)));
            }
        }

        public MessageViewModel()
        {
            Users = App.SessionManager.Users;

            Messages = new ObservableCollection<UserMessage>();
            App.SessionManager.StartReceivingMessages();
            App.SessionManager.MessageReceived += OnMessageRecieved;
            RegisterUser();
        }

        private void OnMessageRecieved(object sender, MessageReceivedEventArgs e)
        {
            if(e.Message is UserMessage)
            {
                Messages.Add(e.Message as UserMessage);
                MessageAdded(this, null);
            }
        }

        private async void RegisterUser()
        {
            if(!App.SessionManager.IsHost) 
                await App.SessionManager.SendMessage(App.SessionManager.CurrentUser,
                                                        App.SessionManager.Host);
        }

        public async void SubmitMessage()
        {
            var msg = new UserMessage()
            {
                User = App.SessionManager.CurrentUser,
                DateTimeStamp = DateTime.Now,
                Message = NewMessage
            };

            await App.SessionManager.BroadCastMessage(msg);

            NewMessage = "";
            MessageAdded(this, null);
        }
    }
}
