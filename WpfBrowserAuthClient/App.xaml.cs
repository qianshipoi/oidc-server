using IdentityModel.Client;

using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;

namespace WpfBrowserAuthClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? mutex = null;
        private const string appName = "WpfBrowserAuthClient";
        private readonly static string applicationPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WpfBrowserAuthClient.exe");

        public const string Api = "https://demo.duendesoftware.com/api/dpop/test";
        public const string CustomUriScheme = "openiddict-wpf-browser-auth-client"; 
        public const string SigninCallback = $"{CustomUriScheme}://signin";
        public const string SignoutCallback = $"{CustomUriScheme}://signout";

        protected override async void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, appName, out var createdNew);
            if (!createdNew)
            {
                var message = "[\"active\"]";
                if(e.Args.Length > 0)
                {
                    message = JsonSerializer.Serialize(e.Args);
                    await SendCallbackToMainProcess(e.Args[0]);
                }
                Shutdown();
                return;
            }

            base.OnStartup(e);

            if (!ProtocolManager.IsProtocolRegistered(CustomUriScheme))
            {
                // private protocol not registered, register it

                // check if running as administrator
                CheckAdmin();
                ProtocolManager.RegisterProtocol(CustomUriScheme, applicationPath);
            }
            else
            {
                // private protocol already registered, check if up to date
                if (!ProtocolManager.IsProtocolUpToDate(CustomUriScheme, applicationPath))
                {
                    // private protocol not up to date, update it

                    // check if running as administrator
                    CheckAdmin();
                    ProtocolManager.RegisterProtocol(CustomUriScheme, applicationPath);
                }
            }

            if (e.Args.Length>0)
                await SendCallbackToMainProcess(e.Args[0]);
        }

        private async Task SendCallbackToMainProcess(string callbackUrl)
        {
            if (callbackUrl.StartsWith(SigninCallback))
            {
                var response = new AuthorizeResponse(callbackUrl);
                if (!String.IsNullOrWhiteSpace(response.State))
                {
                    var callbackManager = new CallbackManager(response.State);
                    await callbackManager.RunClient(callbackUrl);
                }
            }
            else if (callbackUrl.StartsWith(SignoutCallback))
            {
                var loginResult = (this.MainWindow as MainWindow)?.LoginResult;
                if (loginResult != null)
                {
                    var sub = loginResult.User.FindFirst("sub")!.Value;
                    var callbackManager = new CallbackManager(sub);
                    await callbackManager.RunClient(callbackUrl);
                }
            }
        }


        private static void CheckAdmin()
        {
            if (!IsRunningAsAdministrator())
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = applicationPath,
                    Verb = "runas" 
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch
                {
                    MessageBox.Show("启动管理员权限失败。");
                    return;
                }

                // 关闭当前进程
                Application.Current.Shutdown();
                return;
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
