using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangDreamChartsConverter.Models;

namespace BangDreamChartsConverter.IServices
{
    public interface IConvertService
    {
        DefaultChart GenerateDefaultChart(string inputText, ConvertTypeFrom convertTypeFrom, bool checkRepeat,
            double delay);

        string CovertDefaultChart(DefaultChart defaultChart, ConvertTypeTo convertTypeTo);
    }
}
