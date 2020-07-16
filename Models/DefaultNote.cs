﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangDreamChartsConverter.Models
{
    public class DefaultNote
    {
        /// <summary>
        ///     时间
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        ///     音符类型
        /// </summary>
        public DefaultNoteType NoteType { get; set; }

        /// <summary>
        ///     所在轨道
        /// </summary>
        public int Track { get; set; }
    }

    public enum DefaultNoteType
    {
        白键 = 1,
        粉键 = 2,
        滑条a_开始 = 3,
        滑条a_中间 = 4,
        滑条a_结束 = 5,
        滑条b_开始 = 6,
        滑条b_中间 = 7,
        滑条b_结束 = 8,
        弃用_长键结束 = 9,
        fever = 10,
        技能 = 11,
        滑条a_粉键结束 = 12,
        滑条b_粉键结束 = 13,
        长键_粉键 = 14,
        改变bpm = 20,
        长键_开始 = 21,
        长键_结束 = 25,
        长键_粉键结束 = 26
    }
}
