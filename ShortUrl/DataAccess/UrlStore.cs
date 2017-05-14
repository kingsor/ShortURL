namespace ShortUrl.DataAccess
{
	public interface UrlStore
	{
		void SaveUrl(string url, string shortenedUrl);
		string GetUrlFor(string shortenedUrl);
        void SaveOrUpdateUrl(string url, string shortenedUrl);
	}
}