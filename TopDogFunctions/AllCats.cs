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
    public static class AllCats
    {
        [FunctionName("AllCats")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "cats")] HttpRequestMessage req,
            TraceWriter log)
        {

            RootObject cats;
            using (var client = new WebClient())
            {
                client.BaseAddress = "https://www.battersea.org.uk";
                var json = client.DownloadString($"/api/animals/cats");
                cats = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var results = cats.Animals.Values.ToList().Select(d => new ApiAnimalDetails
            {
                Name = d.title,
                Gender= d.field_animal_sex,
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
