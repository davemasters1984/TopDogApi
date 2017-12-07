using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;

namespace TopDogFunctions
{
    public static class DogsRehoming
    {
        [FunctionName("DogsRehoming")]
        public static async Task<HttpResponseMessage> Run(
                [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "dogsrehoming")] HttpRequestMessage req,
                TraceWriter log)
        {

            RootObject dogByName;
            using (var client = new WebClient())
            {
                client.BaseAddress = "https://www.battersea.org.uk";
                var json = client.DownloadString($"/api/animals/dogs");
                dogByName = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var results = dogByName.Animals.Values.ToList().Where(d => d.field_animal_rehomed.ToLowerInvariant() == "").Select(d => new ApiAnimalsFormatted()
            {
                Name = d.title,
                Breed = d.field_animal_breed,
                IsChildFriendly = d.field_animal_child_suitability,
                ImageUrl = "https:" + d.field_animal_thumbnail
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(results), Encoding.UTF8, "application/json")
            };
        }
    }
}
