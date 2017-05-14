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
            base.ConfigureConventions(nancyConventions);

            Conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", @"Content"));
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
