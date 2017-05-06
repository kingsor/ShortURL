namespace ShortUrlDesktopApp
{
    using System;
    using Nancy.Hosting.Self;

    class Program
    {
        private string url = "http://localhost";
        private int port = 8089;
        private NancyHost nancy;

        public Program()
        {
            var uri = new Uri($"{url}:{port}/");
            nancy = new NancyHost(uri);
        }

        private void Start()
        {
            nancy.Start();
            Console.WriteLine($"Started listennig port {port}");
            Console.ReadKey();
            nancy.Stop();
        }

        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Start();
        }
    }
}
