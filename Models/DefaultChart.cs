using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangDreamChartsConverter.Models
{
    public class DefaultChart
    {
        /// <summary>
        ///     延迟时间(单位:毫秒)
        /// </summary>
        public int Delay_ms { get; set; }

        /// <summary>
        ///     每分钟节拍数
        /// </summary>
        public float Bpm { get; set; }

        /// <summary>
        ///     谱面数据
        /// </summary>
        public List<DefaultNote> Notes { get; set; }
    }
}
