using System.Windows.Input;
using Xamarin.Forms;

namespace StoreUserApp
{
    public class MainPageViewModel : BindableObject
    {
        private string _userName;
        private bool _loggedIn;

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public bool LoggedIn
        {
            get => _loggedIn;
            set
            {
                _loggedIn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NotLoggedIn));
            }
        }

        public bool NotLoggedIn => !LoggedIn;

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get;}

        public MainPageViewModel()
        {
            LoginCommand = new Command(Login);
            LogoutCommand = new Command(Logout);
        }

        private void Logout(object obj)
        {
            LoggedIn = false;
        }

        private void Login(object obj)
        {
            LoggedIn = true;
        }
    }
}
