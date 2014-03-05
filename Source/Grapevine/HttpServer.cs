using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public class HttpServer
    {
        TcpListener tcpListener;

        public HttpServer()
        {

        }

        public void Start(Action<HttpRequest, HttpResponse> processRequest)
        {
            if (processRequest == null)
                throw new ArgumentNullException("processRequest");

            this.tcpListener = new TcpListener(IPAddress.Any, 8080);
            this.tcpListener.Start();

            while (true)
            {
                try
                {
                    this.WaitForClient(processRequest);
                }
                catch (Exception)
                {
                }
            }
        }

        private void WaitForClient(Action<HttpRequest, HttpResponse> processRequest)
        {
            var client = this.tcpListener.AcceptTcpClient();

            Task.Factory.StartNew((state) =>
            {                
                using (NetworkStream stream = ((TcpClient)state).GetStream())
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

                    processRequest(request, response);

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
            }, client);
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
