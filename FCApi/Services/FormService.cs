using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using ClassLibraryModel;

namespace FCApi.Services
{
    public class FormService : IFormService
    {
        private readonly IMongoCollection<FormModelV2> _forms;
        public FormService(IDbConfig config, IMongoClient client)
        {
            var db = client.GetDatabase(config.DatabaseName);
            _forms = db.GetCollection<FormModelV2>(config.FormsCN);
        }
        public FormModelV2 GetForm(Guid id)
        {
            return _forms.Find(x => x.Id == id).FirstOrDefault();
        }
        public List<FormModelV2> GetFormsByUser(Guid uid)
        {
            return _forms.Find(x => x.OwnerId == uid).ToList();
        }
        public FormModelV2? CreateForm(FormModelV2 model)
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

        public List<FormModelV2> SearchForm(string name)
        {
            var fIndex = Builders<FormModelV2>.Filter.Where(
                x => x.Name.ToLower().Contains(name.ToLower())
                && x.CanBeSearched 
            );

            return _forms.Find(fIndex).ToList();
        }
        public bool EditForm(FormModelV2 model)
        {
            return _forms.ReplaceOne(x => x.Id == model.Id, model).ModifiedCount > 0;
        }
    }
}
