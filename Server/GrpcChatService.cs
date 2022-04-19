using Grpc.Core;

using SimpleChatApp.GrpcService;

namespace SimpleChatApp.Server;

public class GrpcChatService : ChatService.ChatServiceBase
{
    private readonly IChatServerModel _chatServerModel;

    public GrpcChatService(IChatServerModel chatServerModel) => _chatServerModel = chatServerModel;

    public override async Task<Messages> GetLogs(TimeIntervalRequest request, ServerCallContext context)
    {
        try
        {
            (var logs, var status) = await _chatServerModel.GetLogs(new(request.Sid.Guid_.ToString()),
                                                                    request.StartTime.ToDateTime(),
                                                                    request.EndTime.ToDateTime());
            var messages = new Messages() { ActionStatus = status.Convert() };
            if (logs != null)
            {
                foreach (var message in logs)
                {
                    messages.Logs.Add(message.Convert());
                }
            }
            return messages;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new Messages() { ActionStatus = GrpcService.ActionStatus.ServerError };
        }
    }

    public override async Task<GrpcService.AuthorizationAnswer> LogIn(AuthorizationData request, ServerCallContext context) => 
        (await _chatServerModel.LogIn(request.UserData.Login, request.UserData.PasswordHash, context.Peer, request.ClearActiveConnection))
        .Convert();

    public override async Task<RegistrationAnswer> RegisterNewUser(UserData request, ServerCallContext context) => new() 
    { 
        Status = (await _chatServerModel.RegisterNewUser(request.Login, request.PasswordHash)).Convert() 
    };

    public override async Task Subscribe(GrpcService.Guid request, IServerStreamWriter<Messages> responseStream, ServerCallContext context)
    {
        try
        {
            (var buffer, var status) = await _chatServerModel.Subscribe(new(request.Guid_));
            if (status != CommonTypes.ActionStatus.Allowed)
            {
                await responseStream.WriteAsync(new() { ActionStatus = status.Convert() });
                return;
            }

            await foreach (var message in buffer!.ReceiveAllAsync(context.CancellationToken))
            {
                Messages messages = new() { ActionStatus = GrpcService.ActionStatus.Allowed };
                messages.Logs.Add(message.Convert());
                await responseStream.WriteAsync(messages);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine(ex.ToString());
            await responseStream.WriteAsync(new() { ActionStatus = GrpcService.ActionStatus.ServerError });
        }
        finally
        {
            await _chatServerModel.CloseUserConnection(new System.Guid(request.Guid_));
        }
    }

    public override async Task<ActionStatusMessage> Unsubscribe(GrpcService.Guid request, ServerCallContext context)
    {
        try
        {
            return new()
            {
                ActionStatus = (await _chatServerModel.CloseUserConnection(new System.Guid(request.Guid_))).Convert()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return new() { ActionStatus = GrpcService.ActionStatus.ServerError };
        }
    }

    public override async Task<ActionStatusMessage> Write(OutgoingMessage request, ServerCallContext context) => new()
    {
        ActionStatus = (await _chatServerModel.SendMessage(new(request.Sid.Guid_), request.Text)).Convert()
    };

    public async void CloseAllConnections() => await _chatServerModel.ClearAllConnections();
}