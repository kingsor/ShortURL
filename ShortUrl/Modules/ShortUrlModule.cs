﻿namespace ShortUrl.Modules
{
    using DataAccess;
    using Helpers;
    using Model;
    using MongoDB.Bson;
    using Nancy;
    using Nancy.ModelBinding;
    using Nancy.Responses.Negotiation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ShortUrlModule : NancyModule
    {
        public ShortUrlModule(UrlStore urlStore)
        {
            Get["/"] = _ => View["index.html"];

            Post["/"] = _ => ShortenAndSaveUrl(urlStore);

            Post["/new"] = _ => ShortenAndSaveUrl(urlStore);

            Post["/test"] = parameters => TestMethod(parameters);

            Get["/{shorturl}"] = parameters => GetLongUrl(parameters, urlStore);

            Get["/items"] = parameters => GetSavedItems(urlStore);

            Get["/stats"] = parameters => GetAllStats(urlStore);

            Get["/{shorturl}/stats"] = parameters => GetShortUrlStats(parameters, urlStore);

            Get["/{shorturl}/content"] = parameters => GetShortUrlContent(parameters, urlStore);

            Get["/cleanup"] = _ => CleanupCollections(urlStore);
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

        private dynamic TestMethod(dynamic parameters)
        {
            var newUrl = this.Bind<NewUrl>();

            return Negotiate.WithModel(new { Input = newUrl });
        }

        private dynamic CleanupCollections(UrlStore urlStore)
        {
            urlStore.ClearCollections();

            return View["404.html"];
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
            // if run at localhost the .First() method throws an exception
            // because locally there is no forwarding through proxy as in AppHarbor configuration
            var userAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();

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

            if(longUrl == null)
            {
                return Negotiate.WithModel(new { Message = "Url parameter is null" });
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
