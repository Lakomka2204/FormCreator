using FormCreator.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using static FormCreator.Services.IUserService;

namespace FormCreator.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<UserModel> _users;
        private readonly IPasswordService passwordService;
        private readonly IEmailService emailService;

        public UserService(IDbConfig config, IMongoClient client, IPasswordService passwordService, IEmailService emailService)
        {
            var db = client.GetDatabase(config.DatabaseName);
            _users = db.GetCollection<UserModel>(config.UsersCN);
            this.passwordService = passwordService;
            this.emailService = emailService;
        }
        public VerifyStatus VerifyEmailCode(Guid userId, string code)
        {
            var user = GetUser(userId);
            if (user.EmailVerified)
                return VerifyStatus.AlreadyVerified;
            bool match = emailService.CheckCode(userId, code);
            user.EmailVerified = match;
            if (match)
                UpdateUser(userId, user);
            return match ? VerifyStatus.Verified : VerifyStatus.WrongCode;
        }
        public UserModel? GetUserByLoginAndPassword(string login, string password, out LoginStatus loginStatus)
        {
            try
            {
                loginStatus = LoginStatus.NoStatus;
                var filter = Builders<UserModel>.Filter.Or(
                    Builders<UserModel>.Filter.Eq(u => u.Email, login),
                    Builders<UserModel>.Filter.Eq(u => u.Username, login)
                );
                var user = _users.Find(filter).FirstOrDefault();
                if (user == null)
                {
                    loginStatus = LoginStatus.NoUser;
                    return null;
                }
                if (!passwordService.VerifyPassword(user, user.Password, password))
                {
                    loginStatus = LoginStatus.WrongPassword;
                    return null;
                }
                if (user.AccountState == AccountState.Deleted)
                {
                    loginStatus = LoginStatus.NoUser;
                    return null;
                }
                loginStatus = user.AccountState == AccountState.PendingDeletion ? LoginStatus.AccountDeleted : LoginStatus.Success;
                return user;
            }
            catch
            {
                loginStatus = LoginStatus.NoStatus;
                return null;
            }
        }
        public UserModel? RegisterUser(UserModel user)
        {
            if (GetUsers().Any(x =>
            {
                var emailsplit = x.Email.Split('@');
                return emailsplit[0].Replace(".", "") + "@" + emailsplit[1] == user.Email;
            }))
                return null;
            user.Password = passwordService.HashPassword(user, user.Password);
            if (emailService.CanSendEmail(user.Id))
                emailService.SendVerificationCodeEmail(user.Id, user.Email, "Registration verification");
            _users.InsertOne(user);
            return user;
        }

        public List<UserModel> GetUsers()
        {
            return _users.Find(_ => true).ToList();
        }

        public UserModel GetUser(Guid id)
        {
            var u = _users.Find(x => x.Id == id).FirstOrDefault();
            if (u == null) return null;
            if (DateTime.Now >= u.DeletionDate && u.AccountState == AccountState.PendingDeletion)
            {
                u.AccountState = AccountState.Deleted;
                UpdateUser(id, u);
                return u;
            }
            return u;
        }
        public bool SetDeletionDate(Guid id, out DateTime deletionTime)
        {
            deletionTime = DateTime.MinValue;
            var u = GetUser(id);
            if (u.AccountState == AccountState.Deleted) return false;
            if (u.AccountState == AccountState.PendingDeletion) return false;
            u.AccountState = AccountState.PendingDeletion;
            u.DeletionDate = DateTime.UtcNow.AddMonths(3);
            deletionTime = u.DeletionDate;
            return UpdateUser(id, u);
        }
        public bool RemoveUser(Guid id)
        {
            return _users.DeleteOne(x => x.Id == id).DeletedCount > 0;
        }

        public bool UpdateUser(Guid id, UserModel user)
        {
            return _users.ReplaceOne(x => x.Id == id, user).ModifiedCount > 0;
        }
    }
}
