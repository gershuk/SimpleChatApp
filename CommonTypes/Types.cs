using System;

namespace SimpleChatApp.CommonTypes
{
    public enum RegistrationStatus
    {
        RegistrationSuccessfull = 0,
        LoginAlreadyExist = 1,
        BadInput = 2,
        ServerError = 3
    }

    public static class EnumConverterExtension
    {
        public static GrpcService.RegistrationStatus Convert(this RegistrationStatus status)
        {
            switch (status)
            {
                case RegistrationStatus.RegistrationSuccessfull:
                    return GrpcService.RegistrationStatus.RegistrationSuccessfull;

                case RegistrationStatus.LoginAlreadyExist:
                    return GrpcService.RegistrationStatus.LoginAlreadyExist;

                case RegistrationStatus.BadInput:
                    return GrpcService.RegistrationStatus.BadInput;

                case RegistrationStatus.ServerError:
                    return GrpcService.RegistrationStatus.RegistratioError;

                default:
                    throw new NotImplementedException();
            }
        }

        public static GrpcService.AuthorizationStatus Convert(this AuthorizationStatus status)
        {
            switch (status)
            {
                case AuthorizationStatus.AuthorizationSuccessfull:
                    return GrpcService.AuthorizationStatus.AuthorizationSuccessfull;

                case AuthorizationStatus.WrongLoginOrPassword:
                    return GrpcService.AuthorizationStatus.WrongLoginOrPassword;

                case AuthorizationStatus.AnotherConnectionActive:
                    return GrpcService.AuthorizationStatus.AnotherConnectionActive;

                case AuthorizationStatus.ServerError:
                    return GrpcService.AuthorizationStatus.AuthorizationError;

                default:
                    throw new NotImplementedException();
            }
        }

        public static GrpcService.ActionStatus Convert(this ActionStatus status)
        {
            switch (status)
            {
                case ActionStatus.Allowed:
                    return GrpcService.ActionStatus.Allowed;

                case ActionStatus.Forbidden:
                    return GrpcService.ActionStatus.Forbidden;

                case ActionStatus.WrongSid:
                    return GrpcService.ActionStatus.WrongSid;

                case ActionStatus.ServerError:
                    return GrpcService.ActionStatus.ServerError;

                default:
                    throw new NotImplementedException();
            }
        }

        public static GrpcService.MessageData Convert(this MessageData message)
        {
            return new GrpcService.MessageData()
            {
                MessageId = new GrpcService.Guid() { Guid_ = message.Id.ToString() },
                PlayerId = message.UserId,
                PlayerLogin = message.UserLogin,
                Text = message.Text,
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(message.Time.ToUniversalTime()),
            };
        }

        public static MessageData Convert(this GrpcService.MessageData message)
        {
            return new MessageData()
            {
                Id = new Guid(message.MessageId.Guid_),
                Text = message.Text,
                Time = message.Timestamp.ToDateTime(),
                UserId = (int)message.PlayerId,
                UserLogin = message.PlayerLogin,
            };
        }

        public static GrpcService.AuthorizationAnswer Convert(this AuthorizationAnswer answer)
        {
            return new GrpcService.AuthorizationAnswer()
            {
                Sid = new GrpcService.Guid() { Guid_ = answer.Sid.ToString() },
                Status = answer.AuthorizationStatus.Convert(),
            };
        }
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

    public readonly struct AuthorizationAnswer
    {
        public readonly AuthorizationStatus AuthorizationStatus;
        public readonly Guid Sid;

        public AuthorizationAnswer(AuthorizationStatus authorizationStatus, Guid sid)
        {
            AuthorizationStatus = authorizationStatus;
            Sid = sid;
        }
    }

    public readonly struct AccountData
    {
        public readonly int Id;
        public readonly string Username;
        public readonly string PasswordHash;

        public AccountData(int id, string username, string passwordHash)
        {
            Id = id;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }
    }

    public readonly struct ConnectionData
    {
        public readonly int Id;
        public readonly Guid Sid;

        public ConnectionData(int id, Guid sid)
        {
            Id = id;
            Sid = sid;
        }
    }

    public struct MessageData
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public MessageData(Guid id, int userId, string userLogin, string text, DateTime time)
        {
            Id = id;
            UserId = userId;
            UserLogin = userLogin ?? throw new ArgumentNullException(nameof(userLogin));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Time = time;
        }
    }
}