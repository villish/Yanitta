using System;

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
