using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using FunctionCore.Helpers;

namespace FunctionCore
{
    public static class ContactInfoApi
    {
        static List<ContactInfo> contactInfos=new List<ContactInfo>();
        [FunctionName("CreateContactInfo")]
        public static async Task<IActionResult> CreateContactInfo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ContactInfo")] HttpRequest req,
            ILogger log)
        {
           log.LogTrace("Creating a new contact info item");
           string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
           ContactInfo contactInfo = JsonConvert.DeserializeObject<ContactInfo>(requestBody);
           
           ContactInfoDTO contactInfoDTO=new ContactInfoDTO()
           {
               FirstName = contactInfo.FirstName,
               LastName = contactInfo.LastName,
               Number = contactInfo.Number                 
           };
          
          ContactInfoItemHelper contactInfoHelper=new ContactInfoItemHelper(Configurations.GetMongoDbConnectionInfo());
          await contactInfoHelper.CreateContactInfoItemAsync(contactInfo);
           return new OkObjectResult(contactInfoDTO);
        }

        [FunctionName("GetContactInfos")]
        public static IActionResult GetContactInfos(
            [HttpTrigger(AuthorizationLevel.Function,"get",Route="ContactInfo")] HttpRequest request,
            ILogger log)
        {
            log.LogInformation("Getting contact info list items");
            string firstName=request.Query["firstName"];
            string lastName=request.Query["lastName"];
            ContactInfoItemHelper contactInfoHelper=new ContactInfoItemHelper(Configurations.GetMongoDbConnectionInfo());
            var contactInfoResult = contactInfoHelper.GetContactInfoList(firstName,lastName);
            List<ContactInfoDTO> result=null;
            if(contactInfoResult!=null)
            {
                result=new List<ContactInfoDTO>();
                foreach (var item in contactInfoResult)
                {
                    ContactInfoDTO contactInfoDTO=new ContactInfoDTO();
                    contactInfoDTO.FirstName=item.FirstName;
                    contactInfoDTO.LastName=item.LastName;
                    contactInfoDTO.Number=item.Number;
                    contactInfoDTO.Id=item.Id;
                    result.Add(contactInfoDTO);
                }
            }
            return new OkObjectResult(result);
        }

        [FunctionName("GetContactInfoById")]
        public static IActionResult GetContactInfoById(
            [HttpTrigger(AuthorizationLevel.Function,"get",Route="ContactInfo/{id}")]HttpRequest request,
            ILogger log,
            string id)
        {
            ContactInfoItemHelper contactInfoHelper=new ContactInfoItemHelper(Configurations.GetMongoDbConnectionInfo());
            var contactInfoResult = contactInfoHelper.GetGetContactInfo(id);
            ContactInfoDTO contactInfoDTO=null;
            if(contactInfoResult==null)
            {
                return new NotFoundResult();
            }
            else
            {
                contactInfoDTO = new ContactInfoDTO();
                contactInfoDTO.FirstName=contactInfoResult.FirstName;
                contactInfoDTO.LastName=contactInfoResult.LastName;
                contactInfoDTO.Number=contactInfoResult.Number;
                contactInfoDTO.Id=contactInfoResult.Id;
            }
            return new OkObjectResult(contactInfoDTO);
        }

         [FunctionName("UpdateContactInfo")]
        public static async Task<IActionResult> UpdateContactInfo(
            [HttpTrigger(AuthorizationLevel.Function,"put",Route="ContactInfo/{id}")]HttpRequest request,           
            ILogger log,
            string id)
        {
            ContactInfoItemHelper contactInfoHelper=new ContactInfoItemHelper(Configurations.GetMongoDbConnectionInfo());
            var contactInfo=contactInfoHelper.GetGetContactInfo(id);
            if(contactInfo==null)
            {
                return new NotFoundResult();
            }
            string requestBody=await new StreamReader(request.Body).ReadToEndAsync();
            ContactInfoUpdateDTO updateContactInfoData = JsonConvert.DeserializeObject<ContactInfoUpdateDTO>(requestBody);     
            var successful= await contactInfoHelper.UpdateContactInfoAsync(updateContactInfoData,id);
            if(!successful)
            {
                return new StatusCodeResult(304);
            }
                        
            return new OkObjectResult(updateContactInfoData);
        }        
    }
}
