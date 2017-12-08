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
    public static class CatByName
    {
        [FunctionName("CatByName")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "catname")] HttpRequestMessage req,
            TraceWriter log)
        {

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            if (string.IsNullOrWhiteSpace(name))
                return req.CreateResponse(HttpStatusCode.OK);

            RootObject dogByName;
            using (var client = new WebClient())
            {
                client.BaseAddress = "https://www.battersea.org.uk";
                var json = client.DownloadString($"/api/animals/cats");
                dogByName = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var results = dogByName.Animals.Values.ToList()
                .Where(d => d.title.ToLowerInvariant() == name.ToLowerInvariant()).Select(d => new ApiAnimalDetails()
                {
                    Name = d.title,
                    Age = d.field_animal_age,
                    Gender = d.field_animal_sex,
                    Size = d.field_animal_size,
                    Breed = d.field_animal_breed,
                    ImageUrl = "https:" + d.field_animal_thumbnail,
                    Location = d.field_animal_centre,
                    IsChildFriendly = d.field_animal_child_suitability,
                    IsCatFriendly = d.field_animal_cat_suitability,
                    IsDogFriendly = d.field_animal_dog_suitability,
                    Path = "https://www.battersea.org.uk" + d.path,
                    Rehomed = d.field_animal_rehomed,
                    Reserved = d.field_animal_reserved
                });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(results), Encoding.UTF8, "application/json")
            };
        }
    }
}
