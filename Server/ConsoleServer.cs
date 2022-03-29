using Grpc.Core;

using System.Globalization;

namespace SimpleChatApp.Server;

public static class ConsoleServer
{
    private const int _ticksToKill = 200_000_000;

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
        Grpc.Core.Server server = new()
        {
            Services = { GrpcService.ChatService.BindService(new GrpcChatService(serverModel)) },
            Ports = { new ServerPort(ip, Convert.ToInt32(port), ServerCredentials.Insecure) }
        };
        try
        {
            server.Start();
            Console.WriteLine("Server listening on port " + port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
        }
        finally
        {
            await serverModel.ClearAllConnections();
            try
            {
                await server.ShutdownAsync().WaitAsync(new TimeSpan(_ticksToKill));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            serverModel.Dispose();
            Console.WriteLine("Server closed");
        }
    }
}