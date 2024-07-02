using IdentityModel.OidcClient;

using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace WpfBrowserAuthClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ApiBaseAddress = "https://localhost:44390/api/";

        private readonly OidcClient _oidcClient;
        private AuthorizeState? _state;
        private HttpClient? _apiClient;
        private bool _isLoggedIn;
        private LoginResult? _loginResult;

        public LoginResult? LoginResult => _loginResult;

        public MainWindow()
        {
            InitializeComponent();
            var options = new OidcClientOptions()
            {
                Authority = "https://localhost:44395/",
                ClientId = "wpfbrowserclient",
                Scope = "openid profile email offline_access dataEventRecords",
                RedirectUri = "https://localhost:44395/wpf-browser-auth-client-callback",
                RefreshTokenInnerHttpHandler = new SocketsHttpHandler(),
                PostLogoutRedirectUri = "https://localhost:44395/wpf-browser-auth-client-signout-callback",
                Browser = new WpfEmbeddedBrowser(),
                Policy = new Policy
                {
                    RequireIdentityTokenSignature = false
                }
            };
            _oidcClient = new OidcClient(options);
            ChangeBtnState(_isLoggedIn);
        }

        private async void LoginClick(object sender, RoutedEventArgs e)
        {
            var loginResult = await _oidcClient.LoginAsync();

            //_state = await _oidcClient.PrepareLoginAsync();
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = _state.StartUrl,
            //    UseShellExecute = true
            //});

            //var loginResult = await ReceiveSignInCallback();

            if (!loginResult.IsError)
            {
                InitializeApiClient(loginResult);
                _isLoggedIn = true;
                _loginResult = loginResult;
                MessageBox.Show($"login user: {loginResult.User.Identity!.Name}");
            }
            else
            {
                _isLoggedIn = false;
                MessageBox.Show(loginResult.Error);
            }
            ChangeBtnState(_isLoggedIn);
        }

        private void InitializeApiClient(LoginResult loginResult)
        {
            _apiClient = new HttpClient(loginResult.RefreshTokenHandler)
            {
                BaseAddress = new Uri(ApiBaseAddress)
            };
        }

        private void ChangeBtnState(bool isLoggedIn)
        {
            btnLoadData.IsEnabled = isLoggedIn;
            btnLogin.IsEnabled = !isLoggedIn;
            btnLogout.IsEnabled = isLoggedIn;
        }

        private async Task<LoginResult> ReceiveSignInCallback()
        {
            var callbackManager = new CallbackManager(_state!.State);
            var response = await callbackManager.RunServer();
            return await _oidcClient.ProcessResponseAsync(response, _state);
        }

        private async void LoadDataClick(object sender, RoutedEventArgs e)
        {
            var data = await _apiClient!.GetFromJsonAsync<List<DataEventRecord>>("DataEventRecords");
            if (data != null)
            {
                DgData.ItemsSource = data;
            }
        }

        private async void LogoutClick(object sender, RoutedEventArgs e)
        {
            var url = await _oidcClient.PrepareLogoutAsync(new LogoutRequest
            {
                IdTokenHint = _loginResult!.IdentityToken,
            });

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

            await ReceivePostLogoutCallback();
        }

        private async Task ReceivePostLogoutCallback()
        {
            var name = _loginResult!.User.FindFirst("sub")!.Value;
            var callbackManager = new CallbackManager(name);
            var response = await callbackManager.RunServer();

            if (response != null)
            {
                _isLoggedIn = false;
                ChangeBtnState(_isLoggedIn);
            }
        }
    }

    public class DataEventRecord
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}