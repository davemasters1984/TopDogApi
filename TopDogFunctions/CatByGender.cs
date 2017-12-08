using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace TopDogFunctions
{
    public static class CatByGender
    {
        [FunctionName("CatByGender")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "catgender")] HttpRequestMessage req,
            TraceWriter log)
        {

            // parse query parameter
            string gender = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "gender", true) == 0)
                .Value;

            if (string.IsNullOrWhiteSpace(gender))
                return req.CreateResponse(HttpStatusCode.OK);

            RootObject dogByName;
            using (var client = new WebClient())
            {
                client.BaseAddress = "https://www.battersea.org.uk";
                var json = client.DownloadString($"/api/animals/cats");
                dogByName = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var results = dogByName.Animals.Values.ToList()
                .Where(d => d.field_animal_sex.ToLowerInvariant() == gender.ToLowerInvariant()).Select(d =>
                    new ApiAnimalDetails()
                    {
                        Name = d.title,
                        Gender = d.field_animal_sex,
                        Breed = d.field_animal_breed,
                        ImageUrl = "https:" + d.field_animal_thumbnail,
                        Location = d.field_animal_centre,
                    });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(results), System.Text.Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
