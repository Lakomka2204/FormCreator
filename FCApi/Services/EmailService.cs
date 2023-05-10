using ClassLibraryModel;
using FCApi.Models;
using MongoDB.Driver;
using System.Net.Mail;
using System.Text;

namespace FCApi.Services
{
    public class EmailService : IEmailService
    {
        private IMongoCollection<EmailVerificationModel> _verificationCodes;

        public EmailService(IDbConfig config, IMongoClient client)
        {
            var db = client.GetDatabase(config.DatabaseName);
            _verificationCodes = db.GetCollection<EmailVerificationModel>(config.VerificationCodesCN);
        }
        public Guid SendVerificationCodeEmail(Guid associatedUid, string email, string reason)
        {
            var emailCode = new EmailVerificationModel()
            {
                Code = Random.Shared.Next(100000, 999999).ToString().PadLeft(6, '0'),
                Id = Guid.NewGuid(),
                Email = email,
                UserId = associatedUid,
                SendTime = DateTime.Now,
                Reason = reason,
            };
            // code for sending emails
            SendEmail(email, reason, $"Your verification code is {emailCode.Code}. Have a good day.", out _);
            _verificationCodes.InsertOne(emailCode);
            return emailCode.Id;
        }
        public bool CheckCode(Guid userId, string code)
        {
            var codeObj = GetCodeByUID(userId);
            return codeObj != null && codeObj.Code == code;
        }
        public bool CheckCodeById(Guid emailId,string code)
        {
            var codeObj = GetCodeByID(emailId);
            return codeObj != null && codeObj.Code == code;
        }
        bool SendEmail(string to, string subj, string content, out Exception? ex)
        {
            ex = null;
            try
            {
                MailMessage mm = new MailMessage(new MailAddress("admin@a.com", "FormCreator"),
                    new MailAddress(to))
                {
                    Subject = subj,
                    Body = content,
                    IsBodyHtml = false,
                    BodyEncoding = Encoding.UTF8,
                };

                using (var client = new SmtpClient("localhost", 25))
                {
                    client.Send(mm);
                    return true;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        public EmailVerificationModel? GetCodeByID(Guid emailId)
        {
            return _verificationCodes.Find(x => x.Id == emailId)?.ToList().LastOrDefault();
        }
        public EmailVerificationModel? GetCodeByUID(Guid userId)
        {
            return _verificationCodes.Find(x => x.UserId == userId)?.ToList()?.LastOrDefault();
        }
        public bool CanSendEmail(Guid userId)
        {
            var code = _verificationCodes.Find(x => x.UserId == userId)?.ToList()?.LastOrDefault();
            if (code == null) return true;
            var ts = DateTime.UtcNow - code.SendTime;
            return ts.TotalMinutes > 5;
        }
    }
}
