namespace ShortUrl
{
    using System.Reflection;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.ViewEngines;
    using DataAccess;
    using Nancy.TinyIoc;
    using Nancy.Conventions;
    using System;
    using System.Configuration;
    using Modules;
    using System.IO;
    using Nancy.Responses;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var connString = Environment.GetEnvironmentVariable("MONGOLAB_URI");
            if(String.IsNullOrEmpty(connString))
            {
                connString = ConfigurationManager.AppSettings["MONGOLAB_URI"];
            }

            var mongoUrlStore = new MongoUrlStore(connString);
            container.Register<UrlStore>(mongoUrlStore);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            // this line works for console app but not for asp.net app
            //nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/public", @"Content/public"));

            // this solution was found here: [https://groups.google.com/d/msg/nancy-web-framework/N3neO1FJ3Qc/NzooDTVSUFIJ]
            // and it works for asp.net app and for console app
            nancyConventions.StaticContentsConventions.Add((ctx, rootPath) =>
            {
                var path = ctx.Request.Url.Path;

                const string resourcePath = "/public";

                if (!path.StartsWith(resourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return new EmbeddedFileResponse(
                    this.GetType().Assembly,
                    "ShortUrl.Content.public",
                    Path.GetFileName(ctx.Request.Url.Path));
            });

            base.ConfigureConventions(nancyConventions);
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                ResourceViewLocationProvider.RootNamespaces[Assembly.GetAssembly(this.GetType())] = "ShortUrl";

                return NancyInternalConfiguration
                  .WithOverrides(x => x.ViewLocationProvider = typeof(ResourceViewLocationProvider));
            }
        }
    }
}
