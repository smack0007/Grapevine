using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grapevine
{
    public class HttpServer
    {
		TcpListener tcpListener;
		ManualResetEvent connectionReceived = new ManualResetEvent(false);

		IPAddress address;
		int port;
		Action<HttpRequest, HttpResponse> processRequest;

		public bool IsRunning
		{
			get;
			private set;
		}

		public IPAddress Address
		{
			get { return this.address; }
			
			set
			{
				if (this.IsRunning)
					ThrowPropertyCannotBeChangedWhenRunningException("Address");

				this.address = value;
			}
		}

		public int Port
		{
			get { return this.port; }

			set
			{
				if (this.IsRunning)
					ThrowPropertyCannotBeChangedWhenRunningException("Port");

				this.port = value;
			}
		}

		public Action<HttpRequest, HttpResponse> ProcessRequest
		{
			get { return this.processRequest; }
			
			set
			{
				if (this.IsRunning)
					ThrowPropertyCannotBeChangedWhenRunningException("ProcessRequest");

				this.processRequest = value;
			}
		}

        public HttpServer()
        {
			this.address = IPAddress.Any;
			this.port = 8080;
        }

		private static void ThrowPropertyCannotBeChangedWhenRunningException(string property)
		{
			throw new GrapevineException(string.Format("{0} property may not be changed once the server is running.", property));
		}

        public void Start()
        {
			if (this.processRequest == null)
				throw new GrapevineException("ProcessRequest must be set before starting the server.");

            this.tcpListener = new TcpListener(this.address, this.port);
            this.tcpListener.Start();
			this.IsRunning = true;

			Task.Factory.StartNew((state) =>
			{
				while (this.IsRunning)
				{
					this.connectionReceived.Reset();
									
					this.tcpListener.BeginAcceptTcpClient(
						this.Connect,
						null);

					this.connectionReceived.WaitOne();
				}
			}, TaskCreationOptions.LongRunning);
        }

		public void Stop()
		{
			this.tcpListener.Stop();
			this.IsRunning = false;
		}
				
        private void Connect(IAsyncResult asyncResult)
        {
			TcpClient client = null;

			try
			{
				if (this.IsRunning)
				{
					client = this.tcpListener.EndAcceptTcpClient(asyncResult);
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				this.connectionReceived.Set();
			}
						
			if (client != null && client.Connected)
			{
				using (NetworkStream stream = client.GetStream())
				{
					StreamReader sr = new StreamReader(stream);
					string requestLine = sr.ReadLine();

					if (requestLine == null)
						return;

					HttpRequest request = new HttpRequest();

					string[] requestLineParts = requestLine.Split(' ');
					request.Method = ParseHttpMethod(requestLineParts[0]);
					request.Url = requestLineParts[1];
					request.HttpVersion = requestLineParts[2].Substring(5); // Substring(5) is for HTTP/

					this.ParseRequestHeaders(request, sr);

					if (stream.DataAvailable)
						this.ParseRequestBody(request, sr);

					HttpResponse response = new HttpResponse();

					this.processRequest(request, response);

					StreamWriter sw = new StreamWriter(stream);
					sw.WriteLine("HTTP/{0} {1}", request.HttpVersion, FormatStatusCode(response.StatusCode));

					foreach (var header in response.Headers)
						sw.WriteLine("{0}: {1}", header.Key, header.Value);

					string body = response.GetBody();

					if (!response.Headers.ContainsKey("Content-Type"))
						sw.WriteLine("Content-Type: text/plain");

					if (!response.Headers.ContainsKey("Content-Length"))
						sw.WriteLine("Content-Length: {0}", body.Length);

					sw.WriteLine();
					sw.Write(body);
					sw.WriteLine();
					sw.Flush();
				}
			}
        }

        private static HttpMethod ParseHttpMethod(string method)
        {
            switch (method.ToUpper())
            {
                case "OPTIONS": return HttpMethod.Options;
                case "GET": return HttpMethod.Get;
                case "HEAD": return HttpMethod.Head;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "TRACE": return HttpMethod.Trace;
                case "CONNECT": return HttpMethod.Connect;
            }

            throw new GrapevineException("Bad HTTP Method.");
        }

        private static readonly char[] HeaderSeparator = new char[] { ':' };

        private void ParseRequestHeaders(HttpRequest request, StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != null && line.Length > 0)
            {
                string[] headerParts = line.Split(HeaderSeparator, 2);
                request.Headers.Add(headerParts[0], headerParts[1]);
                line = sr.ReadLine();
            }
        }

        private void ParseRequestBody(HttpRequest request, StreamReader sr)
        {
            StringBuilder sb = new StringBuilder();

            string line = sr.ReadLine();
            while (line != null && line.Length > 0)
            {
                sb.AppendLine(line);
                line = sr.ReadLine();
            }

            request.Body = sb.ToString();
        }

        private static string FormatStatusCode(HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.Ok: return "200 OK";
                case HttpStatusCode.NotFound: return "404 Not Found";
            }

            throw new GrapevineException("Bad HTTP Status Code.");
        }
    }
}
