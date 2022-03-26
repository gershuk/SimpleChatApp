using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using System.Globalization;

using static SimpleChatApp.GrpcService.ChatService;

namespace SimpleChatApp.Client;

public static class ConsoleClient
{
    private static ChatServiceClient _chatServiceClient;

    public static async Task Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("Enter ip");
        var ip = Console.ReadLine();
        if (ip == null || ip == string.Empty)
        {
            ip = "127.0.0.1";
            Console.WriteLine(ip);
        }
        Console.WriteLine("Enter port");
        var port = Console.ReadLine();
        if (port == null || port == string.Empty)
        {
            port = "30051";
            Console.WriteLine(port);
        }

        _chatServiceClient = new(new Channel($"{ip}:{port}", ChannelCredentials.Insecure));
        Console.WriteLine("Client created");
        Console.WriteLine("Enter login");
        var login = Console.ReadLine();
        Console.WriteLine("Enter password");
        var password = await SHA256.GetStringHash(Console.ReadLine());
        var ans1 = await _chatServiceClient.RegisterNewUserAsync(new()
        {
            Login = login,
            PasswordHash = password
        });
        Console.WriteLine(ans1);
        var ans2 = await _chatServiceClient.LogInAsync(new()
        {
            ClearActiveConnection = true,
            UserData = new() { Login = login, PasswordHash = password }
        });
        Console.WriteLine(ans2);
        var logs = await _chatServiceClient.GetLogsAsync(new()
        {
            Sid = ans2.Sid,
            StartTime = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime()),
            EndTime = Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime())
        });

        foreach (var log in logs.Logs)
        {
            Console.WriteLine(log);
        }

        var data = _chatServiceClient.Subscribe(ans2.Sid).ResponseStream;
        var ans3 = await _chatServiceClient.WriteAsync(new() { Sid = ans2.Sid, Text = "OOOOOOOOOOOOOOOOOO" });
        while (await data.MoveNext())
        {
            Console.WriteLine(data.Current);
            var ans4 = await _chatServiceClient.UnsubscribeAsync(ans2.Sid);
            Console.WriteLine(ans4);
        }
    }
}