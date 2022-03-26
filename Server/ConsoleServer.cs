using Grpc.Core;

using System.Globalization;

namespace SimpleChatApp.Server;

public static class ConsoleServer
{
    public static async Task Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("Enter ip");
        var ip = Console.ReadLine();
        if (ip == null || ip == string.Empty)
        {
            ip = "localhost";
            Console.WriteLine(ip);
        }
        Console.WriteLine("Enter port");
        var port = Console.ReadLine();
        if (port == null || port == string.Empty)
        {
            port = "30051";
            Console.WriteLine(port);
        }

        Grpc.Core.Server server = new()
        {
            Services = { GrpcService.ChatService.BindService(new GrpcChatService(new ChatServerModel())) },
            Ports = { new ServerPort("localhost", Convert.ToInt32(port), ServerCredentials.Insecure) }
        };

        server.Start();

        Console.WriteLine("Server listening on port " + port);
        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        await server.ShutdownAsync();
    }
}