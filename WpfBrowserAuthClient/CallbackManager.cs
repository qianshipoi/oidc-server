using System.IO.Pipes;
using System.IO;

namespace WpfBrowserAuthClient
{
    internal class CallbackManager
    {
        private readonly string _name;

        public CallbackManager(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public int ClientConnectTimeoutSeconds { get; set; } = 1;

        // This method is invoked when the callback opens the application. It sends
        // the callback url back to the main instance of the application.
        public async Task RunClient(string args)
        {
            using (var client = new NamedPipeClientStream(".", _name, PipeDirection.Out))
            {
                await client.ConnectAsync(ClientConnectTimeoutSeconds * 1000);

                using (var sw = new StreamWriter(client) { AutoFlush = true })
                {
                    await sw.WriteAsync(args);
                }
            }
        }

        // This method is invoked from the main instance of the application. It
        // receives the callback url from the secondary instance of the application
        // started by the callback redirect.
        public async Task<string> RunServer()
        {
            using (var server = new NamedPipeServerStream(_name, PipeDirection.In))
            {
                await server.WaitForConnectionAsync();

                using (var sr = new StreamReader(server))
                {
                    var msg = await sr.ReadToEndAsync();
                    return msg;
                }
            }
        }
    }
}
