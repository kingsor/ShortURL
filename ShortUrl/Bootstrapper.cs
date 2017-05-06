﻿namespace ShortUrl
{
    using System.Reflection;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.ViewEngines;
    using DataAccess;
    using Nancy.TinyIoc;
    using Nancy.Conventions;

    public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override void ConfigureApplicationContainer(TinyIoCContainer container)
    {
      base.ConfigureApplicationContainer(container);

      var mongoUrlStore = new MongoUrlStore("mongodb://localhost:27017/");
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
