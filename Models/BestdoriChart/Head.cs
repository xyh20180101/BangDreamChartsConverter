using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BangDreamChartsConverter.Models.BestdoriChart
{
    /// <summary>
    ///     bestdori谱面头部信息
    /// </summary>
    public class Head
    {
        /// <summary>
        /// </summary>
        public string cmd = "BPM";

        /// <summary>
        ///     种类
        /// </summary>
        public string type = "System";

        /// <summary>
        ///     所在节拍
        /// </summary>
        [DefaultValue(-1)]
        public double beat { get; set; }

        /// <summary>
        ///     每分钟节拍数
        /// </summary>
        public float bpm { get; set; }
    }
}
