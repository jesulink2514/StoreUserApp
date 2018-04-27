using System.IO;
using System.Threading.Tasks;
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

        private readonly IFileHelper _fileHelper;

        public MainPageViewModel()
        {
            LoginCommand = new Command(Login);
            LogoutCommand = new Command(Logout);
            _fileHelper = DependencyService.Get<IFileHelper>();
        }

        private async void Logout(object obj)
        {
            LoggedIn = false;
            await Write(string.Empty);
            UserName = string.Empty;
        }

        private async void Login(object obj)
        {
            LoggedIn = true;
            await Write(UserName);
        }

        public async void OnNavigatedTo()
        {
            var user = await Read();
            if (string.IsNullOrWhiteSpace(user)) return;
            
            UserName = user;
            LoggedIn = true;
        }

        private const string FileName = "user.dat";
        private async Task<string> Read()
        {
            using (var reader = new StreamReader(File.Open(_fileHelper.GetFullPath(FileName),FileMode.OpenOrCreate)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task Write(string fileContent)
        {
            using (var writer = new StreamWriter(_fileHelper.GetFullPath(FileName),append:false))
            {
                await writer.WriteAsync(fileContent);
            }
        }
    }
}
