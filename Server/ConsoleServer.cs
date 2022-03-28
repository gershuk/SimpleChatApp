using Grpc.Core;

using System.Globalization;

namespace SimpleChatApp.Server;

public static class ConsoleServer
{
    public static async Task Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("Enter ip");
        string? ip = Console.ReadLine();
        if (ip == null || ip == string.Empty)
        {
            ip = "localhost";
            Console.WriteLine(ip);
        }
        Console.WriteLine("Enter port");
        string? port = Console.ReadLine();
        if (port == null || port == string.Empty)
        {
            port = "30051";
            Console.WriteLine(port);
        }
        var serverModel = new ChatServerModel();
        try
        {
            Grpc.Core.Server server = new()
            {
                Services = { GrpcService.ChatService.BindService(new GrpcChatService(serverModel)) },
                Ports = { new ServerPort(ip, Convert.ToInt32(port), ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Server listening on port " + port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
            //await server.KillAsync();
            try
            {
                await server.ShutdownAsync().WaitAsync(new TimeSpan(1000_000)).ContinueWith(async task => { await server.KillAsync(); Console.WriteLine("Kill"); });
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task canceled - server was killed");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("Time out -server was killed");
            }
        }
        finally
        {
            serverModel.Dispose();
        }
    }
}