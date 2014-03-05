using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public class HttpHeaderCollection : IDictionary<string, string>
    {
        IDictionary<string, string> data;

        public ICollection<string> Keys
        {
            get { return this.data.Keys; }
        }
        
        public ICollection<string> Values
        {
            get { return this.data.Values; }
        }

        public int Count
        {
            get { return this.data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public string this[string key]
        {
            get { return this.data[key]; }
            set { this.data[key] = value; }
        }

        public string this[HttpHeader key]
        {
            get { return this.data[GetHttpHeaderName(key)]; }
            set { this.data[GetHttpHeaderName(key)] = value; }
        }

        public HttpHeaderCollection()
        {
            this.data = new Dictionary<string, string>();
        }

        public static string GetHttpHeaderName(HttpHeader header)
        {
            switch (header)
            {
                case HttpHeader.ContentEncoding: return "Content-Encoding";
                case HttpHeader.ContentLength: return "Content-Length";
                case HttpHeader.ContentType: return "Content-Type";
            }

            throw new ArgumentOutOfRangeException("Unknown header.");
        }

        public void Add(string key, string value)
        {
            this.data.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.data.ContainsKey(key);
        }

        public bool ContainsKey(HttpHeader header)
        {
            return this.data.ContainsKey(GetHttpHeaderName(header));
        }
                
        public bool Remove(string key)
        {
            return this.data.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return this.data.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            this.data.Add(item);
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return this.data.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            this.data.CopyTo(array, arrayIndex);
        }
               
        public bool Remove(KeyValuePair<string, string> item)
        {
            return this.data.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
}
