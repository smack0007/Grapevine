using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public class HttpResponse
    {
        StringBuilder body;

        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        public HttpHeaderCollection Headers
        {
            get;
            private set;
        }

        public HttpResponse()
        {
            this.body = new StringBuilder();
            this.StatusCode = HttpStatusCode.Ok;
            this.Headers = new HttpHeaderCollection();
        }

        public void WriteLine()
        {
            this.body.AppendLine();
        }

        public void WriteLine(string value)
        {
            this.body.AppendLine(value);
        }

        public void WriteLine(string format, params object[] args)
        {
            this.body.AppendFormat(format, args);
            this.body.AppendLine();
        }

        public string GetBody()
        {
            return this.body.ToString();
        }
    }
}
