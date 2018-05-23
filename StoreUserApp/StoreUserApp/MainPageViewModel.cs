using PCLCrypto;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace StoreUserApp
{
    public class MainPageViewModel : BindableObject
    {
        private string _userName;
        private bool _loggedIn;
        private string _password;
        private string _encrypted;

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Password { get => _password; set { _password = value; OnPropertyChanged();} }
        public string Encrypted { get => _encrypted; set { _encrypted = value; OnPropertyChanged();} }

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
        public ICommand LogoutCommand { get; }

        private readonly IFileHelper _fileHelper;

        public MainPageViewModel()
        {
            LoginCommand = new Command(Login);
            LogoutCommand = new Command(Logout);
            _fileHelper = DependencyService.Get<IFileHelper>();
        }

        private async void Logout(object obj)
        {
            UserName = string.Empty;
            await Write(UserName);
            LoggedIn = false;
        }

        private async void Login(object obj)
        {
            await Write(UserName);
            AddSensitiveValue("password", Password);
            Encrypted = App.Current.Properties["password"].ToString();
            Password = string.Empty;
            LoggedIn = true;
        }

        public async void OnNavigatedTo()
        {
            var user = await Read();
            if (string.IsNullOrWhiteSpace(user)) return;

            UserName = user;
            Password = ReadSensitiveValue("password");
            Encrypted = App.Current.Properties["password"].ToString();
            LoggedIn = true;
        }

        public const string FileName = "user.dat";

        public async Task<string> Read()
        {
            using (var reader = new StreamReader(File.Open(_fileHelper
                .GetFullPath(FileName), FileMode.OpenOrCreate)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task Write(string text)
        {
            using (var writer = new StreamWriter(_fileHelper.GetFullPath(FileName), append: false))
            {
                await writer.WriteAsync(text);
            }
        }

        private ICryptographicKey GetPrivateKey(ISymmetricKeyAlgorithmProvider provider)
        {
            var pkmValues = App.Current.Properties.ContainsKey("PrivateKeyMaterial") ?
                App.Current.Properties["PrivateKeyMaterial"].ToString() : null;

            var pkm = pkmValues == null ?
                WinRTCrypto.CryptographicBuffer.GenerateRandom(32) : 
                Convert.FromBase64String(pkmValues);

            if (pkmValues == null)
            {
                var value = Convert.ToBase64String(pkm);
                App.Current.Properties["PrivateKeyMaterial"] = value;
            }

            var cryptkey = provider.CreateSymmetricKey(pkm);

            return cryptkey;
        }

        public void AddSensitiveValue(string key, string data)
        {
            var binarydata = GetBinaryData(data);

            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);

            var cryptkey = GetPrivateKey(provider);

            var cipherText = WinRTCrypto.CryptographicEngine.Encrypt(cryptkey, binarydata);

            var serializedText = Convert.ToBase64String(cipherText);

            App.Current.Properties[key] = serializedText;
        }

        public string ReadSensitiveValue(string key)
        {
            if (!App.Current.Properties.ContainsKey(key)) return null;

            var raw = App.Current.Properties[key].ToString();

            if (string.IsNullOrWhiteSpace(raw)) return null;

            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var privateKey = GetPrivateKey(provider);

            var binaryData = Convert.FromBase64String(raw);

            var decrypted = WinRTCrypto.CryptographicEngine.Decrypt(privateKey, binaryData);

            var decryptedString = GetPlainData(decrypted);

            return decryptedString;
        }

        private static byte[] GetBinaryData(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        private static string GetPlainData(byte[] binarydata)
        {
            return binarydata == null ? null :
                Encoding.UTF8.GetString(binarydata, 0, binarydata.Length);
        }
    }
}
