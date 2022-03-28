namespace SimpleChatApp.Server
{
    public interface IChatServerModel
    {
        Task ClearAllConnections();

        void Dispose();

        Task<(List<MessageData>? logs, ActionStatus status)> GetLogs(Guid sid, DateTime startTime, DateTime endTime);

        Task<AuthorizationAnswer> LogIn(string username, string passwordHash, string peerData, bool clearActiveConnection);

        Task<RegistrationStatus> RegisterNewUser(string username, string passwordHash);

        Task<ActionStatus> SendMessage(Guid sid, string text);

        Task<(BufferBlock<MessageData>? buffer, ActionStatus actionStatus)> Subscribe(Guid sid);

        Task<ActionStatus> TryUnsubscribe(Guid sid);
    }
}