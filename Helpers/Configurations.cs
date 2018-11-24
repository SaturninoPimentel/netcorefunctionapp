namespace FunctionCore.Helpers
{
    public class Configurations
    {
        public static MongoDBConnectionInfo GetMongoDbConnectionInfo()
        {
            MongoDBConnectionInfo mongoDbConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = Settings.COSMOSDB_CONNECTIONSTRING,
                DatabaseId = Settings.COSMOSDB_DATABASEID,
                ContactAccountCollection = Settings.COSMOSDB_CONTACTINFOACCOUNTCOLLECTION
            };
            return mongoDbConnectionInfo;
        }
    }
}