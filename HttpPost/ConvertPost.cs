using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangDreamChartsConverter.Models;

namespace BangDreamChartsConverter.HttpPost
{
    public class ConvertPost
    {
        public string InputText { get; set; }

        public ConvertTypeFrom From { get; set; }

        public ConvertTypeTo To { get; set; }

        public bool CheckRepeat { get; set; }

        public int Delay { get; set; }
    }
}
