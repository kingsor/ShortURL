namespace ShortUrl
{
    using DataAccess;
    using Nancy;
    using Nancy.Extensions;
    using Nancy.ModelBinding;
    using Nancy.Responses.Negotiation;
    using System;

    public class ShortUrlModule : NancyModule
    {
        public ShortUrlModule(UrlStore urlStore)
        {
            Get["/"] = _ => View["index.html"];
            Post["/"] = _ => ShortenAndSaveUrl(urlStore);
            Post["/new"] = _ => ShortenAndSaveUrl(urlStore);
            Get["/{shorturl}"] = param =>
            {
                string shortUrl = param.shorturl;
                return Response.AsRedirect(urlStore.GetUrlFor(shortUrl.ToString()));
            };
        }

        private Negotiator ShortenAndSaveUrl(UrlStore urlStore)
        {
            string longUrl = Request.Form.url;
            if(longUrl == null)
            {
                var newUrl = this.Bind<NewUrl>();
                //longUrl = Request.Body.AsString();
                longUrl = newUrl.Url;
            }
            var shortUrl = ShortenUrl(longUrl);

            if(urlStore.GetUrlFor(shortUrl) == null)
            {
                urlStore.SaveUrl(longUrl, shortUrl);
            }

            //return View["shortened_url", new { Request.Headers.Host, ShortUrl = shortUrl }];
            return Negotiate.WithModel(new { ShortUrl = ("http://" + Request.Headers.Host + "/" + shortUrl) });
        }

        private string ShortenUrl(string longUrl)
        {
            UInt32 hash = FNVHash.fnv_32a_str(longUrl);
            //String base58Encoded = Base58Converter.Encode(hash);
            return Base58Converter.Encode(hash);
        }
    }
}
