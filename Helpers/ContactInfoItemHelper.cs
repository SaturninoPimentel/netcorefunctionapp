using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace FunctionCore.Helpers
{
   public class ContactInfoItemHelper : DBHelper
    {
        private IMongoCollection<ContactInfo> collection = null;

        public ContactInfoItemHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
            collection = GetMongoDatabase().GetCollection<ContactInfo>(GetMongoDBConnectionInfo().ContactAccountCollection);
        }

        public async Task CreateContactInfoItemAsync(ContactInfo contactInfo)
        {
            await collection.InsertOneAsync(contactInfo);
        }

        public ContactInfo GetGetContactInfo(string id)
        {
            ContactInfo result = collection.AsQueryable<ContactInfo>().Where<ContactInfo>(sb => sb.Id == id).SingleOrDefault();
            return result;
        }

        public List<ContactInfo> GetContactInfoList(string firstName,
         string lastName)
        {

            return collection.AsQueryable<ContactInfo>().Where<ContactInfo>(pre => pre.FirstName.Contains(firstName)
            ||pre.LastName.Contains(lastName))
            .ToList();

        }

        public async Task<bool> UpdateContactInfoAsync(ContactInfoUpdateDTO contactInfoUpdateDTO,string id)
        {
            FilterDefinition<ContactInfo> filterDefinition=Builders<ContactInfo>.Filter.Eq(pre=>pre.Id,id);
            UpdateDefinition<ContactInfo> updateDefinition=Builders<ContactInfo>.Update
            .Set(pre=>pre.FirstName,contactInfoUpdateDTO.FirstName)
            .Set(pre=>pre.LastName,contactInfoUpdateDTO.LastName)
            .Set(pre=>pre.Number,contactInfoUpdateDTO.Number);
            var updateResponse= await collection.UpdateOneAsync(filterDefinition,updateDefinition);
            return updateResponse.ModifiedCount!=0;
        }
    }
}