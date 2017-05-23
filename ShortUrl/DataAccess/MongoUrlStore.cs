namespace ShortUrl.DataAccess
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
        private IMongoCollection<BsonDocument> webmarks;
        private IMongoCollection<BsonDocument> urlstats;

        public MongoUrlStore(string connectionString)
        {
            var dbName = MongoUrl.Create(connectionString).DatabaseName;

            if(String.IsNullOrEmpty(dbName))
            {
                dbName = "shorturl_db";
            }

            try
            {
                database = new MongoClient(connectionString).GetDatabase(dbName);
                webmarks = database.GetCollection<BsonDocument>("webmarks");
                urlstats = database.GetCollection<BsonDocument>("urlstats");
            }
            catch(Exception ex)
            {
                String message = String.Format("Error connecting to mongodb: connectionString = {0} dbName = {1}", connectionString, dbName);
                throw new Exception(message, ex);
            }
        }

        public void ClearCollections()
        {
            //database.DropCollection("webmarks");
            database.DropCollection("urlstats");
        }

        public void SaveUrl(string url, string shortenedUrl)
        {
            var newDoc = new BsonDocument
            {
                { "url", url },
                { "shortUrl", shortenedUrl },
                { "timestamp", DateTime.UtcNow }
            };

            newDoc = GetUrlDetails(newDoc);

            try
            {
                webmarks.InsertOne(newDoc);
            }
            catch (Exception ex)
            {
                String message = ex.Message;
                throw;
            }
        }

        public void SaveOrUpdateUrl(string url, string shortenedUrl)
        {
            throw new NotImplementedException();
        }

        public string GetUrlFor(string shortenedUrl)
        {
            var urlDocument =
                webmarks
                .Find(Builders<BsonDocument>.Filter.Eq("shortUrl", shortenedUrl))
                .FirstOrDefaultAsync()
                .Result;

            return
                urlDocument == null ?
                null : urlDocument["url"].AsString;
        }

        public string GetUrlForNav(string shortenedUrl, BsonDocument logRequest)
        {
            var urlDocument = GetUrlFor(shortenedUrl);
                
            if(String.IsNullOrEmpty(urlDocument))
            {
                logRequest["statusCode"] = 404;
            }

            try
            {
                urlstats.InsertOne(logRequest);
            }
            catch (Exception ex)
            {
                String message = ex.Message;
                throw;
            }

            return urlDocument;
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

                if(!String.IsNullOrEmpty(key) /* && !key.Equals("article:tag")*/)
                {

                    key = key.Replace('.', ':');

                    if (metaDoc.Contains(key))
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
