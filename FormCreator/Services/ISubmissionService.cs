using FormCreator.Models;

namespace FormCreator.Services
{
    public interface ISubmissionService
    {
        Submission GetSubmission(Guid id);
        List<Submission> GetSubmissionsByForm(Guid formId);
        List<Submission> GetSubmissionsByUser(Guid userId);
        Submission? Submit(Submission submission);
        bool RemoveSubmission(Guid id);
    }
}
