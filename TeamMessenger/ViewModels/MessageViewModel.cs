using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamMessenger.Models;
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
            Users = new ObservableCollection<User>();
            Messages = new ObservableCollection<UserMessage>();

            Users.Add(new User() { DisplayName = "Tony Champion" });
            Users.Add(new User() { DisplayName = "Shannon Champion" });

            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message. This is a weird thigns...", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message. This is a weird thigns...", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message. This is a weird thigns...", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message. This is a weird thigns...", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            Messages.Add(new UserMessage() { User = Users.Skip(1).First(), Message = "This is a test message.", DateTimeStamp = DateTime.Now });
            MessageAdded(this, null);
        }

        public void SubmitMessage()
        {
            Messages.Add(new UserMessage() { User = Users.First(), DateTimeStamp = DateTime.Now, Message = NewMessage });
            NewMessage = "";
            MessageAdded(this, null);
        }
    }
}
