using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models.aExceptionsLoger
{
    public class CustomException : ApplicationException
    {
        public CustomException(string message) : base(message) { }
    }
}
