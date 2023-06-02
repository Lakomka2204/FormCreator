using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using ClassLibraryModel;

namespace FCApi.Services
{
    public class FormService : IFormService
    {
        private readonly IMongoCollection<FormModel> _forms;
        public FormService(IDbConfig config, IMongoClient client)
        {
            var db = client.GetDatabase(config.DatabaseName);
            _forms = db.GetCollection<FormModel>(config.FormsCN);
        }
        public FormModel GetForm(Guid id)
        {
            return _forms.Find(x => x.Id == id).FirstOrDefault();
        }
        public List<FormModel> GetFormsByUser(Guid uid)
        {
            return _forms.Find(x => x.OwnerId == uid).ToList();
        }
        public FormModel? CreateForm(FormModel model)
        {
            if (model.FormElements.Any(x => x == null)) return null;
            model.Id = Guid.NewGuid();
            _forms.InsertOne(model);

            return model;
        }
        public bool DeleteForm(Guid id)
        {
            return _forms.DeleteOne(x => x.Id == id).DeletedCount > 0;
        }

        public List<FormModel> SearchForm(string name)
        {
            var fIndex = Builders<FormModel>.Filter.Where(
                x => x.Name.ToLower().Contains(name.ToLower())
                && x.CanBeSearched 
            );

            return _forms.Find(fIndex).ToList();
        }
        public bool EditForm(FormModel model)
        {
            return _forms.ReplaceOne(x => x.Id == model.Id, model).ModifiedCount > 0;
        }
    }
}
