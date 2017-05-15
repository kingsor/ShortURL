namespace ShortUrl.Modules
{
    using DataAccess;
    using Helpers;
    using Nancy;
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
            Get["/{shorturl}"] = parameters => GetLongUrl(parameters, urlStore);

            //Get["/{shorturl}"] = param =>
            //{
            //    string shortUrl = param.shorturl;
            //    string longUrl = urlStore.GetUrlFor(shortUrl.ToString());

            //    if (String.IsNullOrEmpty(longUrl))
            //    {
            //        return View["404.html"];
            //    }
            //    else
            //    {
            //        return Response.AsRedirect(longUrl);
            //    }
            //};
        }

        private  dynamic GetLongUrl(dynamic parameters, UrlStore urlStore)
        {
            string shortUrl = parameters.shorturl;
            string longUrl = urlStore.GetUrlFor(shortUrl.ToString());

            if (String.IsNullOrEmpty(longUrl))
            {
                return View["404.html"];
            }
            else
            {
                return Response.AsRedirect(longUrl);
            }
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
