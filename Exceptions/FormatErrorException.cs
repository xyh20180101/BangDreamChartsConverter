using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangDreamChartsConverter.Exceptions
{
    public class FormatErrorException:Exception
    {
        public FormatErrorException(string message) : base(message)
        {
        }
    }
}
