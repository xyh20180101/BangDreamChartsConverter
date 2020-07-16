using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BangDreamChartsConverter.Models.BestdoriChart
{
    /// <summary>
    ///     bestdori谱面音符信息
    /// </summary>
    public class Note
    {
        /// <summary>
        ///     种类
        /// </summary>
        public string type = "Note";

        /// <summary>
        ///     轨道
        /// </summary>
        public int lane { get; set; }

        /// <summary>
        ///     所在节拍
        /// </summary>
        public double beat { get; set; }

        /// <summary>
        ///     音符类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public NoteType note { get; set; }

        /// <summary>
        ///     是否技能键
        /// </summary>
        public bool skill { get; set; }

        /// <summary>
        ///     是否滑键
        /// </summary>
        public bool flick { get; set; }

        /// <summary>
        ///     滑条类别
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PosType pos { get; set; }

        /// <summary>
        ///     是否滑条开始
        /// </summary>
        public bool start { get; set; }

        /// <summary>
        ///     是否滑条结束
        /// </summary>
        public bool end { get; set; }
    }
}
