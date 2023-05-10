using FCApi.Models;

namespace FCApi.Services
{
    public interface IEmailService
    {
        Guid SendVerificationCodeEmail(Guid associatedUid, string email, string reason);
        EmailVerificationModel? GetCodeByUID(Guid userId);
        EmailVerificationModel? GetCodeByID(Guid emailId);
        bool CanSendEmail(Guid userId);
        bool CheckCode(Guid userId, string code);
        bool CheckCodeById(Guid emailId, string code);
    }
}
