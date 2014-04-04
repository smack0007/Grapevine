using System;

namespace Grapevine.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer http = new HttpServer();

			http.ProcessRequest = (request, response) =>
			{
				Console.WriteLine(request.Url);

				if (request.Url != "/foo")
				{
					response.WriteLine("Hello from {0}.", request.Url);
				}
				else
				{
					response.StatusCode = HttpStatusCode.NotFound;
				}
			};
			
			http.Start();
									
			Console.WriteLine("Press enter to stop HTTP server.");
			Console.ReadLine();

			http.Stop();

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
        }
    }
}
