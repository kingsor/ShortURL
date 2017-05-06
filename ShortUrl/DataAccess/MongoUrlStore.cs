﻿namespace ShortUrl.DataAccess
{
    using System.Threading;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NSoup;
    using NSoup.Nodes;
    using NSoup.Select;
    using System;

    public class MongoUrlStore : UrlStore
    {
        private IMongoDatabase database;
        private IMongoCollection<BsonDocument> urls;

        public MongoUrlStore(string connectionString)
        {
            database = new MongoClient(connectionString).GetDatabase("webmarksdb");
            urls = database.GetCollection<BsonDocument>("webmarks");
        }

        public void SaveUrl(string url, string shortenedUrl)
        {
            var newDoc = new BsonDocument
            {
                { "url", url },
                { "short_url", shortenedUrl }
            };

            newDoc = GetUrlDetails(newDoc);

            urls.InsertOneAsync(newDoc, CancellationToken.None);
        }

        public void SaveOrUpdateUrl(string url, string shortenedUrl)
        {
            throw new NotImplementedException();
        }

        public string GetUrlFor(string shortenedUrl)
        {
            var urlDocument =
                urls
                .Find(Builders<BsonDocument>.Filter.Eq("short_url", shortenedUrl))
                .FirstOrDefaultAsync()
                .Result;

            return
                urlDocument == null ?
                null : urlDocument["url"].AsString;
        }

        private BsonDocument GetUrlDetails(BsonDocument newDoc)
        {
            string url = newDoc["url"].AsString;

            IConnection conn = NSoupClient.Connect(url);
            conn.UserAgent("Mozilla");

            Document doc = conn.Get();

            newDoc.Add("title", doc.Title);

            Elements metaElements = doc.Select("meta");

            BsonDocument metaDoc = new BsonDocument();

            foreach (Element item in metaElements)
            {
                String key = String.Empty;
                String value = String.Empty;

                foreach (NSoup.Nodes.Attribute attrib in item.Attributes)
                {
                    if (attrib.Key.Equals("name") || attrib.Key.Equals("property") || attrib.Key.Equals("http-equiv"))
                    {
                        key = attrib.Value;
                    }

                    if (attrib.Key.Equals("content"))
                    {
                        value = attrib.Value;
                    }

                    if (attrib.Key.Equals("charset"))
                    {
                        key = attrib.Key;
                        value = attrib.Value;
                    }
                }

                if(!String.IsNullOrEmpty(key) && !key.Equals("article:tag"))
                {
                    if(metaDoc.Contains(key))
                    {
                        metaDoc[key] = value;
                    }
                    else
                    {
                        metaDoc.Add(key, value);
                    }
                }
            }

            newDoc.Add("meta", metaDoc);

            return newDoc;
        }
        
    }
}
