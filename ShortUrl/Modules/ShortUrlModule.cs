namespace ShortUrl.Modules
{
    using DataAccess;
    using Helpers;
    using MongoDB.Bson;
    using Nancy;
    using Nancy.ModelBinding;
    using Nancy.Responses.Negotiation;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ShortUrlModule : NancyModule
    {
        public ShortUrlModule(UrlStore urlStore)
        {
            Get["/"] = _ => View["index.html"];

            Post["/"] = _ => ShortenAndSaveUrl(urlStore);

            Post["/new"] = _ => ShortenAndSaveUrl(urlStore);

            Get["/{shorturl}"] = parameters => GetLongUrl(parameters, urlStore);

            Get["/cleanup"] = _ => CleanupCollections(urlStore);
        }

        private dynamic CleanupCollections(UrlStore urlStore)
        {
            urlStore.ClearCollections();

            return View["404.html"];
        }

        private  dynamic GetLongUrl(dynamic parameters, UrlStore urlStore)
        {
            string shortUrl = parameters.shorturl;

            //*
            var bsonHeaders = new BsonDocument();

            foreach(String key in Request.Headers.Keys)
            {
                IEnumerable<string> values = Request.Headers[key];

                int count = values.Count();

                if(count == 1)
                {
                    bsonHeaders.Add(key, values.First());
                }
                else
                {
                    BsonArray bsonArray = new BsonArray(values);
                    bsonHeaders.Add(key, bsonArray);
                }
            }
            //*/

            var logRequest = new BsonDocument
            {
                {"shortUrl", shortUrl },
                {"userHostAddress", GetUserHostAddress() },
                {"headers", bsonHeaders },
                {"userAgent", Request.Headers.UserAgent},
                {"referrer", Request.Headers.Referrer },
                {"timestamp", DateTime.UtcNow },
                {"statusCode", 200 }
            };


            string longUrl = urlStore.GetUrlForNav(shortUrl.ToString(), logRequest);

            if (String.IsNullOrEmpty(longUrl))
            {
                return View["404.html"];
            }
            else
            {
                return Response.AsRedirect(longUrl);
            }
        }

        private String GetUserHostAddress()
        {
            var userAddress = Request.Headers["X-Forwarded-For"].First();

            if(String.IsNullOrEmpty(userAddress))
            {
                userAddress = Request.UserHostAddress;
            }
            else
            {
                //extract first IP
                var index = userAddress.IndexOf(',');
                if (index > 0)
                {
                    userAddress = userAddress.Substring(0, index);
                }

                //remove port
                index = userAddress.IndexOf(':');
                if (index > 0)
                {
                    userAddress = userAddress.Substring(0, index);
                }
            }

            return userAddress;
        }

        private Negotiator ShortenAndSaveUrl(UrlStore urlStore)
        {
            string longUrl = Request.Form.url;
            if(longUrl == null)
            {
                var newUrl = this.Bind<NewUrl>();
                longUrl = newUrl.Url;
            }

            var shortUrl = ShortenUrl(longUrl);

            if(urlStore.GetUrlFor(shortUrl) == null)
            {
                urlStore.SaveUrl(longUrl, shortUrl);
            }

            return Negotiate.WithModel(new { ShortUrl = ("http://" + Request.Headers.Host + "/" + shortUrl) });
        }

        private string ShortenUrl(string longUrl)
        {
            UInt32 hash = FNVHash.fnv_32a_str(longUrl);
            return Base58Converter.Encode(hash);
        }
    }
}
