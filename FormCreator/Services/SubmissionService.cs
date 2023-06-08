using FormCreator.Models;
using MongoDB.Driver;

namespace FormCreator.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IMongoCollection<Submission> _submissions;
        public SubmissionService(IDbConfig config, IMongoClient client)
        {
            var db = client.GetDatabase(config.DatabaseName);
            _submissions = db.GetCollection<Submission>(config.SubmissionsCN);
        }
        public Submission GetSubmission(Guid id)
        {
            return _submissions.Find(x => x.Id == id).FirstOrDefault();
        }
        public List<Submission> GetSubmissionsByForm(Guid formId)
        {
            return _submissions.Find(x => x.FormId == formId).ToList();
        }
        public List<Submission> GetSubmissionsByUser(Guid userId)
        {
            return _submissions.Find(x => x.UserId == userId).ToList();
        }
        public Submission? Submit(Submission submission)
        {
            if (submission.Submissions.Count == 0)
            {
                return null;
            }
            submission.Id = Guid.NewGuid();
            submission.SubmissionDate = DateTime.UtcNow;
            _submissions.InsertOne(submission);
            return submission;
        }
        public bool RemoveSubmission(Guid id)
        {
            return _submissions.DeleteOne(x =>  x.Id == id).DeletedCount > 0;
        }
    }
}
