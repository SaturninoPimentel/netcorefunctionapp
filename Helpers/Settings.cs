using System;

namespace FunctionCore.Helpers
{
    public class Settings
    {
        public static string COSMOSDB_CONNECTIONSTRING =>Environment.GetEnvironmentVariable("COSMOSDB_CONNECTIONSTRING");
        public static string COSMOSDB_DATABASEID => Environment.GetEnvironmentVariable("COSMOSDB_DATABASEID");
        public static string COSMOSDB_CONTACTINFOACCOUNTCOLLECTION => Environment.GetEnvironmentVariable("COSMOSDB_CONTACTINFOCOLLECTION");
        public static string PARTITIONKEY => Environment.GetEnvironmentVariable("PARTITIONKEY");
        public static string ROWKEY=>Environment.GetEnvironmentVariable("ROWKEY");
    }
}