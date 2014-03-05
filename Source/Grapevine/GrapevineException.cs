using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapevine
{
    public class GrapevineException : Exception
    {
        public GrapevineException(string message)
            : base(message)
        {
        }
    }
}
