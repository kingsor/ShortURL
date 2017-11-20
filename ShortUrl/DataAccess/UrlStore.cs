using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.DataAccess
{
	public interface UrlStore
	{
		void SaveUrl(string url, string shortenedUrl);
        void SaveUrlAsync(string url, string shortenedUrl);
        string GetUrlForNav(string shortenedUrl, BsonDocument logRequest);
        string GetUrlFor(string shortenedUrl);
        List<BsonDocument> GetStatsFor(string shortenedUrl);
        List<BsonDocument> GetSavedItems();
        void SaveOrUpdateUrl(string url, string shortenedUrl);
        void ClearCollections();
	}
}