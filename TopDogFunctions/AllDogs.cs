using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace TopDogFunctions
{
    public static class AllDogs
    {
        [FunctionName("AllDogs")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dogs")]HttpRequestMessage req, TraceWriter log)
        {

            RootObject dogs;
            using (var client = new WebClient())
            {
                client.BaseAddress = "https://www.battersea.org.uk";
                var json = client.DownloadString($"/api/animals/dogs");
                dogs = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var results = dogs.Animals.Values.ToList().Select(d => new ApiFormattedAnimals
            {
                Name = d.title,
                Breed = d.field_animal_breed,
                IsChildFriendly = d.field_animal_child_suitability,
                ImageUrl = d.field_animal_thumbnail
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(results), Encoding.UTF8, "application/json")
            };

            //    log.Info("C# HTTP trigger function processed a request.");

            //    // parse query parameter
            //    string name = req.GetQueryNameValuePairs()
            //        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //        .Value;

            //    // Get request body
            //    dynamic data = await req.Content.ReadAsAsync<object>();

            //    // Set name to query string or body data
            //    name = name ?? data?.name;

            //    return name == null
            //        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            //        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }


    }
}
