using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangDreamChartsConverter.Exceptions
{
    public class OutputErrorException:Exception
    {
        public OutputErrorException(string message) : base(message)
        {
        }
    }
}
