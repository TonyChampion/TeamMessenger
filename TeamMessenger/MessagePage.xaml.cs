using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TeamMessenger.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TeamMessenger
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessagePage : Page
    {
        public MessagePage()
        {
            this.InitializeComponent();
            ViewModel.MessageAdded += OnMessageAdded;
        }

        private void OnMessageAdded(object sender, EventArgs e)
        {
            lvMessages.ScrollIntoView(ViewModel.Messages.Last());
        }

        public MessageViewModel ViewModel { get; } = new MessageViewModel();
    }
}
