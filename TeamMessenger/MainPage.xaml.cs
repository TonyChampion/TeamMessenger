using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TeamMessenger.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TeamMessenger
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ViewModel.SessionConnected += ViewModel_SessionConnected;
            ViewModel.ErrorConnecting += ViewModel_ErrorConnecting;
        }

        private async void ViewModel_ErrorConnecting(object sender, SessionCreationResult e)
        {
            var dialog = new MessageDialog("Error connecting to a session");
            await dialog.ShowAsync();
        }

        private void ViewModel_SessionConnected(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(MessagePage));

        }

        public MainViewModel ViewModel { get; } = new MainViewModel();
    }
}
