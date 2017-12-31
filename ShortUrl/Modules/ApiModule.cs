using MongoDB.Bson;
using Nancy;
using ShortUrl.DataAccess;
using System.Text;

namespace ShortUrl.Modules
{
    public class ApiModule : NancyModule
    {
        public ApiModule(UrlStore urlStore) : base("/api")
        {
            Get["/items"] = parameters => GetSavedItems(urlStore);

            Get["/stats"] = parameters => GetAllStats(urlStore);

            Get["/{shorturl}/stats"] = parameters => GetShortUrlStats(parameters, urlStore);

            Get["/{shorturl}/content"] = parameters => GetShortUrlContent(parameters, urlStore);
        }

        private dynamic GetSavedItems(UrlStore urlStore)
        {
            var items = urlStore.GetSavedItems();

            var json = items.ToJson();

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return new Response
            {
                ContentType = "application/json",
                Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
            };
        }

        private dynamic GetAllStats(UrlStore urlStore)
        {
            var stats = urlStore.GetAllStats();

            var jsonBytes = Encoding.UTF8.GetBytes(stats.ToJson());

            return new Response
            {
                ContentType = "application/json",
                Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
            };
        }

        private dynamic GetShortUrlStats(dynamic parameters, UrlStore urlStore)
        {
            string shortUrl = parameters.shorturl;

            var stats = urlStore.GetStatsFor(shortUrl);

            var json = stats.ToJson();

            //return Negotiate.WithModel(result);

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return new Response
            {
                ContentType = "application/json",
                Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
            };
        }

        private dynamic GetShortUrlContent(dynamic parameters, UrlStore urlStore)
        {
            return Negotiate.WithModel(new { Message = "Calling method GetShortUrlContent" });
        }
    }
}
