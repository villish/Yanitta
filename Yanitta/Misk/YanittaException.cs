using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yanitta
{
    [Serializable]
    public class YanittaException : Exception
    {
        public YanittaException(string message)
            : base(message)
        {
        }
    }
}
