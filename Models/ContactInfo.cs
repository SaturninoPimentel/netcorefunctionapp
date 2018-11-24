using System;
using FunctionCore.Helpers;
using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionCore
{
    public class ContactInfo : TableEntity
    {
        public ContactInfo()
        {
            Id = Guid.NewGuid().ToString("n");
            CreatedTime = DateTime.UtcNow;
            PartitionKey = Settings.PARTITIONKEY;
            RowKey = Settings.ROWKEY;
        }
        public string Id {get;set;}
        public DateTime CreatedTime{get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public string Number { get; set; }
        
    } 

    public class ContactInfoUpdateDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public string Number { get; set; }
    }

    public class ContactInfoDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }
    }

}