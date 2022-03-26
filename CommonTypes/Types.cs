namespace SimpleChatApp.CommonTypes;

public enum RegistrationStatus
{
    RegistrationSuccessfull = 0,
    LoginAlreadyExist = 1,
    BadInput = 2,
    ServerError = 3
}

public static class EnumConverterExtension
{
    public static GrpcService.RegistrationStatus Convert(this RegistrationStatus status) =>
    status switch
    {
        RegistrationStatus.RegistrationSuccessfull => GrpcService.RegistrationStatus.RegistrationSuccessfull,
        RegistrationStatus.LoginAlreadyExist => GrpcService.RegistrationStatus.LoginAlreadyExist,
        RegistrationStatus.BadInput => GrpcService.RegistrationStatus.BadInput,
        RegistrationStatus.ServerError => GrpcService.RegistrationStatus.RegistratioError,
        _ => throw new NotImplementedException(),
    };

    public static GrpcService.AuthorizationStatus Convert(this AuthorizationStatus status) =>
    status switch
    {
        AuthorizationStatus.AuthorizationSuccessfull => GrpcService.AuthorizationStatus.AuthorizationSuccessfull,
        AuthorizationStatus.WrongLoginOrPassword => GrpcService.AuthorizationStatus.WrongLoginOrPassword,
        AuthorizationStatus.AnotherConnectionActive => GrpcService.AuthorizationStatus.AnotherConnectionActive,
        AuthorizationStatus.ServerError => GrpcService.AuthorizationStatus.AuthorizationError,
        _ => throw new NotImplementedException(),
    };

    public static GrpcService.ActionStatus Convert(this ActionStatus status) =>
    status switch
    {
        ActionStatus.Allowed => GrpcService.ActionStatus.Allowed,
        ActionStatus.Forbidden => GrpcService.ActionStatus.Forbidden,
        ActionStatus.WrongSid => GrpcService.ActionStatus.WrongSid,
        ActionStatus.ServerError => GrpcService.ActionStatus.ServerError,
        _ => throw new NotImplementedException(),
    };

    public static GrpcService.MessageData Convert(this MessageData message) => new()
    {
        MessageId = new() { Guid_ = message.Id.ToString() },
        PlayerId = message.userId,
        PlayerLogin = message.UserLogin,
        Text = message.Text,
        Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(message.Time.ToUniversalTime()),
    };

    public static GrpcService.AuthorizationAnswer Convert(this AuthorizationAnswer answer) => new()
    {
        Sid = new() { Guid_ = answer.Sid.ToString() },
        Status = answer.AuthorizationStatus.Convert(),
    };
}

public enum AuthorizationStatus
{
    AuthorizationSuccessfull = 0,
    WrongLoginOrPassword = 1,
    AnotherConnectionActive = 2,
    ServerError = 3
}

public enum ActionStatus
{
    Allowed = 0,
    Forbidden = 1,
    WrongSid = 2,
    ServerError = 3
}

public readonly record struct AuthorizationAnswer(AuthorizationStatus AuthorizationStatus, System.Guid Sid);
public readonly record struct AccountData(int Id, string Username, string PasswordHash);
public readonly record struct ConnectionData(int Id, System.Guid Sid);
public readonly record struct MessageData(System.Guid Id, int userId, string UserLogin, string Text, DateTime Time);