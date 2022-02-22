using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace CustomForm.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomFormController : ControllerBase
    {
        private readonly ILogger<CustomFormController> _logger;
        private readonly MongoClient _mongoClient;
        public IConfiguration _configuration { get; }
        
        public CustomFormController(IConfiguration configuration, ILogger<CustomFormController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var appSettings = _configuration.GetSection("AppSettings");
            _mongoClient = new MongoClient(new MongoUrl(appSettings["MongoUrl"]));
        }

        [HttpGet]
        public async Task<IEnumerable<ExpandoObject>> GetAsync(String formDomainName, String formName)
        {
            var customDatabase = _mongoClient.GetDatabase("CustomDatabase_"+ formDomainName);
                var collection = customDatabase.GetCollection<ExpandoObject>("CustomCollection"+ formName);
               var customRecords = await collection.Find(Builders<ExpandoObject>.Filter.Empty).ToListAsync();
            return customRecords;
        }


        [HttpPost]
        public ActionResult Post(ExpandoObject formData)
        {
            var formDataFormated = new  ExpandoObject();
            IDictionary<string, object> forDataToDictionary =  (IDictionary<string, object>)formData;
            foreach (var item in forDataToDictionary)
            {
                try
                {
                    formDataFormated.TryAdd(item.Key, item.Value.ToString());
                }catch (Exception ex)
                {
                    return BadRequest(new
                        {
                            success = false,
                            message = "Failed to parse submitted form"
                        }
                    );
                }

            }
            var formDomainName = "";
            var formName = "";
            if (forDataToDictionary.ContainsKey("formDomainName"))
                formDomainName = forDataToDictionary["formDomainName"].ToString();
            else
                return BadRequest(new
                {
                    success = false,
                    message = "Please pass Form Domain Name as formDomainName"
                });

            if (forDataToDictionary.ContainsKey("formName"))
                formName = forDataToDictionary["formName"].ToString();
            else
                return BadRequest(new
                {
                    success = false,
                    message = "Please pass Form Name as formName"
                });

            try
            {
                var customDatabase = _mongoClient.GetDatabase("CustomDatabase_" + formDomainName);
                if(customDatabase.GetCollection<ExpandoObject>("CustomCollection" + formName) == null )
                    customDatabase.CreateCollection("CustomCollection" + formName);
                var collection = customDatabase.GetCollection<ExpandoObject>("CustomCollection" + formName);
                collection.InsertOne(formDataFormated);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message ="Internal server error"
                }
                );  
            }
            return Ok(new
                        {
                            success = true,
                            message = "Form submitted successfully"
                        }
                    );
        }
    }
}
