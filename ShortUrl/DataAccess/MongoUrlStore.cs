namespace ShortUrl.DataAccess
{
    using System.Threading;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NSoup;
    using NSoup.Nodes;
    using NSoup.Select;
    using System;
    using ReadSharp;
    using System.Threading.Tasks;
    using System.Collections.Generic;

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

            newDoc = GetUrlContent(newDoc);

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

        public List<BsonDocument> GetStatsFor(string shortenedUrl)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("shortUrl", shortenedUrl);
            var result = urlstats.Find(filter).ToListAsync().Result;

            return result;
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

        private BsonDocument GetUrlContent(BsonDocument newDoc)
        {
            String url = newDoc["url"].AsString;
            String rawContent = String.Empty;

            Reader reader = new Reader();
            Article article;

            try
            {
                article = Task.Run(() => reader.Read(new Uri(url))).GetAwaiter().GetResult();

                rawContent = article.Raw;

                newDoc.Add("rawContent", rawContent);

                BsonDocument docArticle = article.ToBsonDocument();

                newDoc.Add("content", docArticle);
            }
            //catch (ReadException ex)
            //{
            //    newDoc.Add("readError", ex.Message);
            //}
            catch(Exception ex)
            {
                newDoc.Add("readError", ex.Message);
            }

            return newDoc;
        }

        private BsonDocument GetUrlDetails(BsonDocument newDoc)
        {
            Document doc = null;

            if (newDoc.Contains("readError"))
            {
                String url = newDoc["url"].AsString;
                IConnection conn = NSoupClient.Connect(url);
                conn.UserAgent("Mozilla");
                doc = conn.Get();
            }
            else
            {
                String rawContent = newDoc["rawContent"].AsString;
                doc = NSoupClient.Parse(rawContent);
            }

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

        private async Task<BsonDocument> GetUrlContentAsync(BsonDocument newDoc)
        {
            String url = newDoc["url"].AsString;
            String rawContent = String.Empty;

            Reader reader = new Reader();
            Article article;

            try
            {
                // here there was a deadlock that I solved thanks to this link
                // https://stackoverflow.com/questions/8438786/calling-an-async-method-from-a-non-async-method

                //Task<Article> readArticle = reader.Read(new Uri(url));
                //article = await readArticle;
                article = await reader.Read(new Uri(url));

                rawContent = article.Raw;

                newDoc.Add("rawContent", rawContent);

                BsonDocument docArticle = article.ToBsonDocument();

                newDoc.Add("content", docArticle);
            }
            catch (ReadException ex)
            {
                newDoc.Add("readError", ex.Message);
            }

            return newDoc;
        }

        public void SaveUrlAsync(string url, string shortenedUrl)
        {
            var newDoc = new BsonDocument
            {
                { "url", url },
                { "shortUrl", shortenedUrl },
                { "timestamp", DateTime.UtcNow }
            };

            newDoc = GetUrlContentAsync(newDoc).Result;

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

    }
}
