using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using SimpleChatApp.CommonTypes;

using System.Globalization;

using static SimpleChatApp.GrpcService.ChatService;

namespace SimpleChatApp.Client;

public static class ConsoleClient
{
    public static async Task Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        var chatServiceClient = InitClient()!;
        Console.WriteLine("Client created");

        Console.WriteLine("Enter login");
        var login = Console.ReadLine();
        Console.WriteLine("Enter password");
        var password = SHA256.GetStringHash(Console.ReadLine());

        Console.WriteLine("Register account? (Y/N)");
        var isRegister = Console.ReadLine();
        if (isRegister?.ToUpper() is "Y")
        {
            GrpcService.RegistrationAnswer? regAns = await chatServiceClient.RegisterNewUserAsync(new()
            {
                Login = login,
                PasswordHash = password
            });
            Console.WriteLine($"Registration status : {regAns.Status}");
        }

        var loginAns = await chatServiceClient.LogInAsync(new()
        {
            ClearActiveConnection = true,
            UserData = new() { Login = login, PasswordHash = password }
        });
        Console.WriteLine($"Authorization status : {loginAns.Status}{Environment.NewLine}Sid : {loginAns.Sid}");

        if (loginAns.Status is GrpcService.AuthorizationStatus.AuthorizationSuccessfull)
        {
            var logs = await chatServiceClient.GetLogsAsync(new()
            {
                Sid = loginAns.Sid,
                StartTime = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime()),
                EndTime = Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime())
            });
            Console.WriteLine($"Getting logs status : {logs.ActionStatus}");
            Console.WriteLine();

            if (logs.ActionStatus is GrpcService.ActionStatus.Allowed)
            {
                foreach (GrpcService.MessageData message in logs.Logs)
                {
                    WriteMessage(message);
                }
            }

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var subTask = SubscribeServer(loginAns.Sid, chatServiceClient, cancellationToken).ConfigureAwait(false);

            var isWorking = true;
            while (isWorking)
            {
                var text = Console.ReadLine();
                if (text is "Stop")
                {
                    isWorking = true;
                }
                else
                {
                    GrpcService.ActionStatusMessage? wrAns = await chatServiceClient.WriteAsync(new() { Sid = loginAns.Sid, Text = text });
                }
            }

            cancellationTokenSource.Cancel();
            await subTask;
        }
    }

    private static void WriteMessage(GrpcService.MessageData messageData) =>
        Console.WriteLine($"From: {messageData.PlayerLogin}{Environment.NewLine}" +
                          $"Text: {messageData.Text}{Environment.NewLine}" +
                          $"Date: {messageData.Timestamp}{Environment.NewLine}");

    private static ChatServiceClient InitClient()
    {
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

        return new ChatServiceClient(new Channel($"{ip}",
                                                 Convert.ToInt32(port),
                                                 ChannelCredentials.Insecure));
    }

    private static async Task SubscribeServer(GrpcService.Guid sid, ChatServiceClient chatServiceClient, CancellationToken cancellationToken)
    {
        var subAns = chatServiceClient.Subscribe(sid).ResponseStream;
        while (await subAns.MoveNext(cancellationToken))
        {
            if (subAns.Current.ActionStatus is GrpcService.ActionStatus.Allowed || !cancellationToken.IsCancellationRequested)
            {
                foreach (var message in subAns.Current.Logs)
                {
                    WriteMessage(message);
                }
                await Task.Yield();
            }
            else
            {
                Console.WriteLine($"Get message status : {subAns.Current.ActionStatus}");
            }
        }
    }
}