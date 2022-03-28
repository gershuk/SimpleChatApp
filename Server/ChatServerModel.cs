namespace SimpleChatApp.Server;

public sealed class ChatServerModel : IDisposable, IChatServerModel
{
    private bool _disposedValue;

    private const string _serverDataBaseConnectionString = "Data Source=server.db";
    private const string _playerConnectionsDataBaseConnectionString = "Data Source=:memory:";

    private const string _accountsTableCreateText = @"CREATE TABLE IF NOT EXISTS ""Accounts"" (""Id"" INTEGER NOT NULL UNIQUE, ""Username""  TEXT NOT NULL UNIQUE,
""PasswordHash"" TEXT NOT NULL, PRIMARY KEY(""Id""));";

    private const string _messagesTableCreateText = @"CREATE TABLE IF NOT EXISTS ""Messages"" (""Id"" TEXT NOT NULL UNIQUE, ""UserId"" INTEGER NOT NULL,
""Text"" TEXT NOT NULL, ""Timestamp"" DATETIME NOT NULL, PRIMARY KEY(""Id""), FOREIGN KEY(""UserId"") REFERENCES Accounts(""Id""));";

    private const string _connectionsTableCreateText = @"CREATE TABLE IF NOT EXISTS ""Connections"" (""UserId"" INTEGER NOT NULL UNIQUE, ""Sid""  TEXT NOT NULL UNIQUE,
""PeerData"" TEXT NOT NULL, PRIMARY KEY(""UserId""));";

    private readonly SqliteConnection _serverDataBaseConnection;
    private readonly SqliteConnection _playerConnectionsDataBaseConnection;

    private readonly Regex _badInputCheckRegex = new(@"[^\w1-9]+", RegexOptions.Compiled);

    private readonly ConcurrentDictionary<Guid, BufferBlock<MessageData>> _subscribers;

    private async Task<AuthorizationAnswer> AuthorizeUser(int id, string peerData)
    {
        try
        {
            Guid guid = Guid.NewGuid();
            using SqliteCommand addConnection = new($@"INSERT INTO Connections VALUES(""{id}"", ""{guid}"", ""{peerData}"")",
                                                             _playerConnectionsDataBaseConnection);
            await addConnection.ExecuteNonQueryAsync();
            return new(AuthorizationStatus.AuthorizationSuccessfull, guid);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new(AuthorizationStatus.ServerError, Guid.Empty);
        }
    }

    private async Task<RegistrationStatus> CreateAccount(string username, string passwordHash)
    {
        try
        {
            using SqliteCommand addAccount = new($@"INSERT INTO Accounts (Username, PasswordHash) VALUES(""{username}"",""{passwordHash}"")",
                                                 _serverDataBaseConnection);
            await addAccount.ExecuteNonQueryAsync();
            return RegistrationStatus.RegistrationSuccessfull;
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return RegistrationStatus.ServerError;
        }
    }

    private async Task<AuthorizationStatus> ClearUserConnection(int id)
    {
        try
        {
            using SqliteCommand deleteConnections = new($@"DELETE FROM Connections WHERE UserId = ""{id}""",
                                                             _playerConnectionsDataBaseConnection);
            await deleteConnections.ExecuteNonQueryAsync();

            return AuthorizationStatus.AuthorizationSuccessfull;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return AuthorizationStatus.ServerError;
        }
    }

    private async Task<AuthorizationStatus> ClearUserConnection(Guid sid)
    {
        try
        {
            using SqliteCommand deleteConnections = new($@"DELETE FROM Connections WHERE Sid = ""{sid}""",
                                                             _playerConnectionsDataBaseConnection);
            await deleteConnections.ExecuteNonQueryAsync();

            return AuthorizationStatus.AuthorizationSuccessfull;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return AuthorizationStatus.ServerError;
        }
    }

    private async Task<bool> IsUserConnectionExist(int id)
    {
        using SqliteCommand getConnections = new($@"SELECT COUNT(UserId) FROM Connections WHERE UserId = ""{id}""",
                                                       _playerConnectionsDataBaseConnection);
        return (long?)await getConnections.ExecuteScalarAsync() > 0;
    }

    private async Task<int?> GetIdForSid(Guid sid)
    {
        using SqliteCommand getConnections = new($@"SELECT UserId FROM Connections WHERE Sid = ""{sid}""",
                                                       _playerConnectionsDataBaseConnection);
        using SqliteDataReader? sqlReader = await getConnections.ExecuteReaderAsync();
        int? id = null;
        if (sqlReader.HasRows)
        {
            Task<bool>? rows = sqlReader.ReadAsync();
            id = sqlReader.GetInt32(0);
        }
        return id;
    }

    private async Task<bool> IsUserConnectionExist(Guid sid)
    {
        using SqliteCommand getConnections = new($@"SELECT COUNT(UserId) FROM Connections WHERE Sid = ""{sid}""",
                                                       _playerConnectionsDataBaseConnection);
        return (long?)await getConnections.ExecuteScalarAsync() > 0;
    }

    private async Task<AuthorizationStatus> CanUserAuthorize(int id)
    {
        try
        {
            return await IsUserConnectionExist(id)
                   ? AuthorizationStatus.AnotherConnectionActive
                   : AuthorizationStatus.AuthorizationSuccessfull;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return AuthorizationStatus.ServerError;
        }
    }

    public ChatServerModel()
    {
        _serverDataBaseConnection = new(_serverDataBaseConnectionString);
        _serverDataBaseConnection.Open();
        using SqliteCommand createAccountsTableCommand = new(_accountsTableCreateText, _serverDataBaseConnection);
        createAccountsTableCommand.ExecuteNonQuery();

        using SqliteCommand createMessagesTableCommand = new(_messagesTableCreateText, _serverDataBaseConnection);
        createMessagesTableCommand.ExecuteNonQuery();

        _playerConnectionsDataBaseConnection = new(_playerConnectionsDataBaseConnectionString);
        _playerConnectionsDataBaseConnection.Open();
        using SqliteCommand createConnectionsTableCommand = new(_connectionsTableCreateText, _playerConnectionsDataBaseConnection);
        createConnectionsTableCommand.ExecuteNonQuery();

        _subscribers = new();
    }

    private async Task<AccountData?> GetAccountData(string username, string passwordHash)
    {
        username = username.ToUpper();
        using SqliteCommand getLoginsCommand = new($@"SELECT Id,Username,PasswordHash FROM Accounts WHERE Username = ""{username}"" AND PasswordHash = ""{passwordHash}""",
                                                   _serverDataBaseConnection);

        AccountData? accountData = null;

        using SqliteDataReader? sqlReader = await getLoginsCommand.ExecuteReaderAsync();
        if (sqlReader.HasRows)
        {
            Task<bool>? rows = sqlReader.ReadAsync();
            accountData = new(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
        }

        return accountData;
    }

    private async Task<AccountData?> GetAccountData(string username)
    {
        username = username.ToUpper();
        using SqliteCommand getLoginsCommand = new($@"SELECT Id,Username,PasswordHash FROM Accounts WHERE Username = ""{username}""",
                                                   _serverDataBaseConnection);

        AccountData? accountData = null;

        using SqliteDataReader? sqlReader = await getLoginsCommand.ExecuteReaderAsync();
        if (sqlReader.HasRows)
        {
            Task<bool>? rows = sqlReader.ReadAsync();
            accountData = new(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
        }

        return accountData;
    }

    private async Task<AccountData?> GetAccountData(int id)
    {
        using SqliteCommand getLoginsCommand = new($@"SELECT Id,Username,PasswordHash FROM Accounts WHERE Id = ""{id}""",
                                                   _serverDataBaseConnection);
        AccountData? accountData = null;
        using SqliteDataReader? sqlReader = await getLoginsCommand.ExecuteReaderAsync();
        if (sqlReader.HasRows)
        {
            Task<bool>? rows = sqlReader.ReadAsync();
            accountData = new(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
        }
        return accountData;
    }

    public async Task<AuthorizationAnswer> LogIn(string username, string passwordHash, string peerData, bool clearActiveConnection)
    {
        AccountData? accountData = await GetAccountData(username, passwordHash);
        AuthorizationStatus status = (accountData.HasValue, clearActiveConnection) switch
        {
            (false, _) => AuthorizationStatus.WrongLoginOrPassword,
            (true, false) => await CanUserAuthorize(accountData!.Value.Id),
            (true, true) => await ClearUserConnection(accountData!.Value.Id),
        };

        return (status is AuthorizationStatus.AuthorizationSuccessfull)
               ? await AuthorizeUser(accountData!.Value.Id, peerData)
               : new(status, Guid.Empty);
    }

    public async Task<RegistrationStatus> RegisterNewUser(string username, string passwordHash)
    {
        username = username.ToUpper();
        using SqliteCommand getLoginsCountCommand = new($@"SELECT COUNT(Username) FROM Accounts WHERE Username = ""{username}""",
                                                   _serverDataBaseConnection);
        long? loginsCount = (long?)await getLoginsCountCommand.ExecuteScalarAsync();
        bool isLoginCorrect = _badInputCheckRegex.Matches(username).Count > 0;
        RegistrationStatus status = (loginsCount, isLoginCorrect) switch
        {
            ( > 0, _) => RegistrationStatus.LoginAlreadyExist,
            (0, false) => RegistrationStatus.RegistrationSuccessfull,
            (0, true) => RegistrationStatus.BadInput,
            _ => throw new NotImplementedException(),
        };

        return status is RegistrationStatus.RegistrationSuccessfull
               ? await CreateAccount(username, passwordHash)
               : status;
    }

    public async Task<(BufferBlock<MessageData>? buffer, ActionStatus actionStatus)> Subscribe(Guid sid)
    {
        return await IsUserConnectionExist(sid)
? (_subscribers.GetOrAdd(sid, new BufferBlock<MessageData>()), ActionStatus.Allowed)
: (null, ActionStatus.WrongSid);
    }

    private void BroadCastMessage(MessageData messageData)
    {
        foreach (BufferBlock<MessageData>? subscriber in _subscribers.Values)
        {
            subscriber.SendAsync(messageData);
        }
    }

    private async Task<ActionStatus> AddMessageToBase(MessageData messageData)
    {
        try
        {
            using SqliteCommand addAccount = new($@"INSERT INTO Messages VALUES(""{messageData.Id}"",""{messageData.UserId}"",
""{messageData.Text}"", ""{messageData.Time}"")", _serverDataBaseConnection);
            await addAccount.ExecuteNonQueryAsync();
            return ActionStatus.Allowed;
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return ActionStatus.ServerError;
        }
    }

    public async Task<ActionStatus> SendMessage(Guid sid, string text)
    {
        try
        {
            bool isConnectionExist = await IsUserConnectionExist(sid);
            if (!isConnectionExist)
            {
                return ActionStatus.WrongSid;
            }

            int? id = await GetIdForSid(sid);
            if (!id.HasValue)
            {
                return ActionStatus.WrongSid;
            }

            AccountData? accauntData = await GetAccountData(id.Value);
            if (!accauntData.HasValue)
            {
                return ActionStatus.ServerError;
            }

            MessageData messageData = new(Guid.NewGuid(), id.Value, accauntData.Value.Username, text, DateTime.Now);
            ActionStatus status = await AddMessageToBase(messageData);
            if (status != ActionStatus.Allowed)
            {
                return ActionStatus.ServerError;
            }

            BroadCastMessage(messageData);

            return ActionStatus.Allowed;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ActionStatus.ServerError;
        }
    }

    public async Task<ActionStatus> TryUnsubscribe(Guid sid)
    {
        bool founded = _subscribers.Remove(sid, out BufferBlock<MessageData>? bufferBlock);
        bufferBlock?.Complete();
        await ClearUserConnection(sid);
        return founded ? ActionStatus.Allowed : ActionStatus.WrongSid;
    }

    public async Task<(List<MessageData>? logs, ActionStatus status)> GetLogs(Guid sid, DateTime startTime, DateTime endTime)
    {
        if (!await IsUserConnectionExist(sid))
        {
            return (null, ActionStatus.WrongSid);
        }

        List<MessageData> logs = new();
        using SqliteCommand getLoginsCommand = new($@"SELECT Messages.Id,UserId,Username,""Text"",Timestamp
FROM Messages Inner JOIN Accounts on Messages.UserId = Accounts.Id
WHERE Timestamp BETWEEN ""{startTime}"" AND ""{endTime}""
ORDER BY Timestamp",
_serverDataBaseConnection);

        using SqliteDataReader? sqlReader = await getLoginsCommand.ExecuteReaderAsync();
        if (sqlReader.HasRows)
        {
            while (await sqlReader.ReadAsync())
            {
                logs.Add(new(sqlReader.GetGuid(0),
                              sqlReader.GetInt32(1),
                              sqlReader.GetString(2),
                              sqlReader.GetString(3),
                              sqlReader.GetDateTime(4)));
            }
        }
        return (logs, ActionStatus.Allowed);
    }

    public async Task ClearAllConnections()
    {
        using SqliteCommand clearConnections = new($@"DELETE FROM Connections", _playerConnectionsDataBaseConnection);
        await clearConnections.ExecuteNonQueryAsync();

        foreach (var subscriber in _subscribers.Values)
            subscriber.Complete();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                ClearAllConnections().Wait();
                using SqliteCommand clearConnections = new($@"DROP TABLE Connections", _playerConnectionsDataBaseConnection);
                clearConnections.ExecuteNonQuery();
                _playerConnectionsDataBaseConnection.Dispose();
                _serverDataBaseConnection.Dispose();
            }
            _disposedValue = true;
        }
    }

    ~ChatServerModel() => Dispose(false);
}