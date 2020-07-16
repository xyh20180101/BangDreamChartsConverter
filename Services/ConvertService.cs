using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BangDreamChartsConverter.Exceptions;
using BangDreamChartsConverter.IServices;
using BangDreamChartsConverter.Models;
using BangDreamChartsConverter.Models.BestdoriChart;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BangDreamChartsConverter.Services
{
    public class ConvertService : IConvertService
    {
        public DefaultChart GenerateDefaultChart(string inputText, ConvertTypeFrom convertTypeFrom, bool checkRepeat, double delay)
        {
            var result = new DefaultChart();
            try
            {
                switch (convertTypeFrom)
                {
                    case ConvertTypeFrom.Bestdori: result = GetDefaultChartFromBestdoriScore(inputText); break;
                    case ConvertTypeFrom.BangSimulator: result = GetDefaultChartFromBangSimulatorScore(inputText); break;
                    case ConvertTypeFrom.BangBangBoom: result = GetDefaultChartFromBangbangboomScore(inputText); break;
                    case ConvertTypeFrom.Bandori: result = GetDefaultChartFromBandoriJson(inputText); break;
                }
            }
            catch (Exception e)
            {
                throw new FormatErrorException(e.Message);
            }
            return result;
        }

        public string CovertDefaultChart(DefaultChart defaultChart, ConvertTypeTo convertTypeTo)
        {
            try
            {
                switch (convertTypeTo)
                {
                    case ConvertTypeTo.Bestdori: return ToBestdoriScore(defaultChart);
                    case ConvertTypeTo.BangSimulator: return ToBangSimulatorScore(defaultChart);
                    case ConvertTypeTo.BangBangBoom: return ToBangbangboomScore(defaultChart);
                    case ConvertTypeTo.BangCraft: return ToBangCraftScore(defaultChart);
                    case ConvertTypeTo.BMS: return ToBMS(defaultChart);
                }
                throw new Exception();
            }
            catch (Exception e)
            {
                throw new OutputErrorException(e.Message);
            }
        }

        #region 读取

        /// <summary>
        ///     从simulator谱面文本构造谱面对象
        /// </summary>
        /// <param name="scoreString">谱面文本</param>
        private DefaultChart GetDefaultChartFromBangSimulatorScore(string scoreString)
        {
            var defaultScore = new DefaultChart();
            var scoreStringArray =
                scoreString.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (int.TryParse(scoreStringArray[0], out var delay_ms) && float.TryParse(scoreStringArray[1], out var bpm))
            {
                defaultScore.Delay_ms = delay_ms;
                defaultScore.Bpm = bpm;
            }
            else
            {
                throw new Exception("转换出错，请检查原谱面文本");
            }

            var notes = new List<DefaultNote>();
            for (var i = 3; i < scoreStringArray.Length; i++)
            {
                var str = scoreStringArray[i].Split('/');
                if (double.TryParse(str[0], out var time) && Enum.TryParse(str[1], out DefaultNoteType noteType) &&
                    int.TryParse(str[2], out var track))
                    notes.Add(new DefaultNote
                    {
                        Time = time,
                        NoteType = noteType,
                        Track = track
                    });
                else
                    throw new Exception("转换出错，请检查原谱面文本");
            }

            //先按时间排，然后按轨道从左到右排
            defaultScore.Notes = notes.OrderBy(p => p.Time).ThenBy(p => p.Track).ToList();
            return defaultScore;
        }

        /// <summary>
        ///     从bestdori谱面文本构造谱面对象
        /// </summary>
        /// <param name="scoreString">谱面文本</param>
        private DefaultChart GetDefaultChartFromBestdoriScore(string scoreString)
        {
            var defaultScore = new DefaultChart();
            var arrayList = JsonConvert.DeserializeObject<ArrayList>(scoreString);
            var head = new Head
            {
                bpm = ((JObject)arrayList[0])["bpm"].ToObject<float>(),
                beat = ((JObject)arrayList[0])["beat"].ToObject<double>()
            };
            defaultScore.Bpm = head.bpm;
            defaultScore.Delay_ms = (int)(head.beat / head.bpm * 60000);

            //提取note列表
            arrayList.RemoveAt(0);
            var tempJson = JsonConvert.SerializeObject(arrayList);
            var tempList = JsonConvert.DeserializeObject<List<Note>>(tempJson);

            var notes = new List<DefaultNote>();
            foreach (var note in tempList)
            {
                var tempNote = new DefaultNote { Time = note.beat, Track = note.lane };

                if (note.note == NoteType.Single &&
                    !note.skill &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.白键;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Single &&
                    note.skill &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.技能;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Single &&
                    !note.skill &&
                    note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.粉键;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.A &&
                    note.start &&
                    !note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条a_开始;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.A &&
                    !note.start &&
                    !note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条a_中间;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.A &&
                    !note.start &&
                    note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条a_结束;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.A &&
                    !note.start &&
                    note.end &&
                    note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条a_粉键结束;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.B &&
                    note.start &&
                    !note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条b_开始;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.B &&
                    !note.start &&
                    !note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条b_中间;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.B &&
                    !note.start &&
                    note.end &&
                    !note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条b_结束;
                    notes.Add(tempNote);
                    continue;
                }

                if (note.note == NoteType.Slide &&
                    note.pos == PosType.B &&
                    !note.start &&
                    note.end &&
                    note.flick)
                {
                    tempNote.NoteType = DefaultNoteType.滑条b_粉键结束;
                    notes.Add(tempNote);
                    continue;
                }

                throw new Exception("bestdori=>bangSimulator音符转换失败");
            }

            //先按时间排，然后按轨道从左到右排
            defaultScore.Notes = notes.OrderBy(p => p.Time).ThenBy(p => p.Track).ToList();
            return defaultScore;
        }

        /// <summary>
        ///     从bangbangboom谱面文本构造谱面对象
        /// </summary>
        /// <param name="scoreString">谱面文本</param>
        private DefaultChart GetDefaultChartFromBangbangboomScore(string scoreString)
        {
            var defaultScore = new DefaultChart();
            var noteArray = scoreString.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var head = noteArray[0].Split('|');
            defaultScore.Delay_ms = (int)(double.Parse(head[1]) * 1000);
            defaultScore.Bpm = float.Parse(head[2]);

            var notes = new List<DefaultNote>();
            var isA = true;
            for (var i = 1; i < noteArray.Length; i++)
            {
                var noteInfo = noteArray[i].Split('|');
                //白键
                if (noteInfo[0] == "s")
                {
                    var noteTimeAndTrack = noteInfo[1].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = DefaultNoteType.白键,
                        Time = double.Parse(noteTimeAndTrack[0]) / 24,
                        Track = int.Parse(noteTimeAndTrack[1]) + 1
                    });
                    continue;
                }

                //粉键
                if (noteInfo[0] == "f")
                {
                    var noteTimeAndTrack = noteInfo[1].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = DefaultNoteType.粉键,
                        Time = double.Parse(noteTimeAndTrack[0]) / 24,
                        Track = int.Parse(noteTimeAndTrack[1]) + 1
                    });
                    continue;
                }

                //非粉滑条
                if (noteInfo[0] == "l" && noteInfo[1] == "0")
                {
                    var startTimeAndTrack = noteInfo[2].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = isA ? DefaultNoteType.滑条a_开始 : DefaultNoteType.滑条b_开始,
                        Time = double.Parse(startTimeAndTrack[0]) / 24,
                        Track = int.Parse(startTimeAndTrack[1]) + 1
                    });
                    for (var j = 3; j < noteInfo.Length - 1; j++)
                    {
                        var noteTimeAndTrack = noteInfo[j].Split(':');
                        notes.Add(new DefaultNote
                        {
                            NoteType = isA ? DefaultNoteType.滑条a_中间 : DefaultNoteType.滑条b_中间,
                            Time = double.Parse(noteTimeAndTrack[0]) / 24,
                            Track = int.Parse(noteTimeAndTrack[1]) + 1
                        });
                    }

                    var endTimeAndTrack = noteInfo[noteInfo.Length - 1].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = isA ? DefaultNoteType.滑条a_结束 : DefaultNoteType.滑条b_结束,
                        Time = double.Parse(endTimeAndTrack[0]) / 24,
                        Track = int.Parse(endTimeAndTrack[1]) + 1
                    });
                    isA = !isA;
                    continue;
                }

                //粉滑条
                if (noteInfo[0] == "l" && noteInfo[1] == "1")
                {
                    var startTimeAndTrack = noteInfo[2].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = isA ? DefaultNoteType.滑条a_开始 : DefaultNoteType.滑条b_开始,
                        Time = double.Parse(startTimeAndTrack[0]) / 24,
                        Track = int.Parse(startTimeAndTrack[1]) + 1
                    });
                    for (var j = 3; j < noteInfo.Length - 1; j++)
                    {
                        var noteTimeAndTrack = noteInfo[j].Split(':');
                        notes.Add(new DefaultNote
                        {
                            NoteType = isA ? DefaultNoteType.滑条a_中间 : DefaultNoteType.滑条b_中间,
                            Time = double.Parse(noteTimeAndTrack[0]) / 24,
                            Track = int.Parse(noteTimeAndTrack[1]) + 1
                        });
                    }

                    var endTimeAndTrack = noteInfo[noteInfo.Length - 1].Split(':');
                    notes.Add(new DefaultNote
                    {
                        NoteType = isA ? DefaultNoteType.滑条a_粉键结束 : DefaultNoteType.滑条b_粉键结束,
                        Time = double.Parse(endTimeAndTrack[0]) / 24,
                        Track = int.Parse(endTimeAndTrack[1]) + 1
                    });
                    isA = !isA;
                }
            }

            //先按时间排，然后按轨道从左到右排
            defaultScore.Notes = notes.OrderBy(p => p.Time).ThenBy(p => p.Track).ToList();
            return defaultScore;
        }

        /// <summary>
        ///     从bandori database谱面json构造谱面对象
        /// </summary>
        /// <param name="scoreString">谱面文本</param>
        private DefaultChart GetDefaultChartFromBandoriJson(string scoreString)
        {
            var defaultScore = new DefaultChart();
            var score = JsonConvert.DeserializeObject<dynamic>(scoreString);

            defaultScore.Bpm = score.metadata.bpm;
            defaultScore.Delay_ms = 0;
            var noteList = new List<DefaultNote>();

            foreach (var note in score.objects)
            {
                if (note.type == "System")
                    continue;
                var defaultNote = new DefaultNote
                {
                    Time = note.beat,
                    Track = note.lane
                };

                if ((note.effect == "Single" || note.effect == "FeverSingle") && note.property == "Single")
                {
                    defaultNote.NoteType = DefaultNoteType.白键;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Skill" && note.property == "Single")
                {
                    defaultNote.NoteType = DefaultNoteType.技能;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Flick" && note.property == "Single")
                {
                    defaultNote.NoteType = DefaultNoteType.粉键;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "SlideStart_A" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条a_开始;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Slide_A" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条a_中间;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "SlideEnd_A" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条a_结束;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "SlideEndFlick_A" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条a_粉键结束;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "SlideStart_B" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条b_开始;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Slide_B" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条b_中间;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "SlideEnd_B" && note.property == "Slide")
                {
                    defaultNote.NoteType = DefaultNoteType.滑条b_结束;
                    noteList.Add(defaultNote);
                    continue;
                }

                if ((note.effect == "Single" || note.effect == "Skill" || note.effect == "FeverSingle") &&
                    note.property == "LongStart")
                {
                    defaultNote.NoteType = DefaultNoteType.长键_开始;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Single" && note.property == "LongEnd")
                {
                    defaultNote.NoteType = DefaultNoteType.长键_结束;
                    noteList.Add(defaultNote);
                    continue;
                }

                if (note.effect == "Flick" && note.property == "LongEnd")
                {
                    defaultNote.NoteType = DefaultNoteType.长键_粉键结束;
                    noteList.Add(defaultNote);
                }
            }

            defaultScore.Notes = noteList;

            return defaultScore;
        }

        #endregion

        #region 输出

        /// <summary>
        ///     输出为bangSimulator播放器格式
        /// </summary>
        /// <returns></returns>
        private string ToBangSimulatorScore(DefaultChart defaultChart)
        {
            var str = "";
            str += $"{defaultChart.Delay_ms}{Environment.NewLine}";
            str += $"{defaultChart.Bpm}{Environment.NewLine}";
            str += $"0/0/0{Environment.NewLine}";
            return defaultChart.Notes.Aggregate(str,
                (current, note) => current + $"{note.Time}/{(int)note.NoteType}/{note.Track}{Environment.NewLine}");
        }

        /// <summary>
        ///     输出为bestdori制谱器格式
        /// </summary>
        /// <returns></returns>
        private string ToBestdoriScore(DefaultChart defaultChart)
        {
            var score = new ArrayList();
            var head = new Head
            {
                beat = defaultChart.Delay_ms / 60000 * defaultChart.Bpm,
                bpm = defaultChart.Bpm
            };
            score.Add(head);
            var tempList = new List<Note>();
            var lastPosType = PosType.B;
            var index = 0;
            foreach (var note in defaultChart.Notes)
            {
                var tempNote = new Note();
                switch (note.NoteType)
                {
                    case DefaultNoteType.白键:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Single
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.粉键:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Single,
                            flick = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.技能:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Single,
                            skill = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条a_开始:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.A,
                            start = true
                        };
                        lastPosType = tempNote.pos;
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条a_中间:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.A
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条a_结束:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.A,
                            end = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条a_粉键结束:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.A,
                            end = true,
                            flick = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条b_开始:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.B,
                            start = true
                        };
                        lastPosType = tempNote.pos;
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条b_中间:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.B
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条b_结束:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.B,
                            end = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.滑条b_粉键结束:
                        tempNote = new Note
                        {
                            lane = note.Track,
                            beat = note.Time,
                            note = NoteType.Slide,
                            pos = PosType.B,
                            end = true,
                            flick = true
                        };
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.长键_开始:
                        {
                            tempNote = new Note
                            {
                                lane = note.Track,
                                beat = note.Time,
                                note = NoteType.Slide,
                                pos = lastPosType == PosType.A ? PosType.B : PosType.A,
                                start = true
                            };
                            lastPosType = tempNote.pos;

                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.长键_结束 && defaultChart.Notes[i].Time != note.Time &&
                                    defaultChart.Notes[i].Track == note.Track)
                                {
                                    var endNote = new Note
                                    {
                                        lane = defaultChart.Notes[i].Track,
                                        beat = defaultChart.Notes[i].Time,
                                        note = NoteType.Slide,
                                        pos = tempNote.pos,
                                        end = true
                                    };
                                    tempList.Add(endNote);
                                    break;
                                }

                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.长键_粉键结束 && defaultChart.Notes[i].Time != note.Time &&
                                    defaultChart.Notes[i].Track == note.Track)

                                {
                                    var endNote = new Note
                                    {
                                        lane = defaultChart.Notes[i].Track,
                                        beat = defaultChart.Notes[i].Time,
                                        note = NoteType.Slide,
                                        pos = tempNote.pos,
                                        end = true,
                                        flick = true
                                    };
                                    tempList.Add(endNote);
                                    break;
                                }
                            }
                        }
                        tempList.Add(tempNote);
                        break;

                    case DefaultNoteType.改变bpm:
                        break;
                }

                index++;
            }

            //先按时间排，然后让滑条结束音符优先在前，其余按轨道从左到右排
            score.AddRange(tempList.OrderBy(p => p.beat).ThenByDescending(p => p.end).ThenBy(p => p.lane).ToList());

            return JsonConvert.SerializeObject(score, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
        }

        /// <summary>
        ///     输出为bangbangboom制谱器格式
        /// </summary>
        /// <returns></returns>
        private string ToBangbangboomScore(DefaultChart defaultChart)
        {
            var str = "";
            str += "\n\n";
            str += $"+|{defaultChart.Delay_ms / 1000}|{defaultChart.Bpm}|4";
            str += "\n\n";
            var index = 0;
            foreach (var note in defaultChart.Notes)
            {
                switch (note.NoteType)
                {
                    case DefaultNoteType.技能:
                    case DefaultNoteType.白键:
                        str += $"s|{(int)(note.Time * 24)}:{note.Track - 1}\n";
                        break;

                    case DefaultNoteType.粉键:
                        str += $"f|{(int)(note.Time * 24)}:{note.Track - 1}\n";
                        break;

                    case DefaultNoteType.滑条a_开始:
                        {
                            var isFlick = 0;
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_结束 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    isFlick = 0;
                                    break;
                                }

                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_粉键结束 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    isFlick = 1;
                                    break;
                                }
                            }

                            str += $"l|{isFlick}|{(int)(note.Time * 24)}:{note.Track - 1}";
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_中间 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    str += $"|{(int)(defaultChart.Notes[i].Time * 24)}:{defaultChart.Notes[i].Track - 1}";
                                    continue;
                                }

                                if ((defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_结束 ||
                                     defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_粉键结束) && defaultChart.Notes[i].Time != note.Time)
                                {
                                    str += $"|{(int)(defaultChart.Notes[i].Time * 24)}:{defaultChart.Notes[i].Track - 1}\n";
                                    break;
                                }
                            }
                        }
                        break;

                    case DefaultNoteType.滑条b_开始:
                        {
                            var isFlick = 0;
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_结束 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    isFlick = 0;
                                    break;
                                }

                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_粉键结束 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    isFlick = 1;
                                    break;
                                }
                            }

                            str += $"l|{isFlick}|{(int)(note.Time * 24)}:{note.Track - 1}";
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_中间 && defaultChart.Notes[i].Time != note.Time)
                                {
                                    str += $"|{(int)(defaultChart.Notes[i].Time * 24)}:{defaultChart.Notes[i].Track - 1}";
                                    continue;
                                }

                                if ((defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_结束 ||
                                     defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_粉键结束) && defaultChart.Notes[i].Time != note.Time)
                                {
                                    str += $"|{(int)(defaultChart.Notes[i].Time * 24)}:{defaultChart.Notes[i].Track - 1}\n";
                                    break;
                                }
                            }
                        }
                        break;

                    case DefaultNoteType.长键_开始:
                        {
                            var isFlick = 0;
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                            {
                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.长键_结束 && defaultChart.Notes[i].Time != note.Time &&
                                    defaultChart.Notes[i].Track == note.Track)
                                {
                                    isFlick = 0;
                                    break;
                                }

                                if (defaultChart.Notes[i].NoteType == DefaultNoteType.长键_粉键结束 && defaultChart.Notes[i].Time != note.Time &&
                                    defaultChart.Notes[i].Track == note.Track)
                                {
                                    isFlick = 1;
                                    break;
                                }
                            }

                            str += $"l|{isFlick}|{(int)(note.Time * 24)}:{note.Track - 1}";
                            for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                                if ((defaultChart.Notes[i].NoteType == DefaultNoteType.长键_结束 ||
                                     defaultChart.Notes[i].NoteType == DefaultNoteType.长键_粉键结束) && defaultChart.Notes[i].Time != note.Time &&
                                    defaultChart.Notes[i].Track == note.Track)
                                {
                                    str += $"|{(int)(defaultChart.Notes[i].Time * 24)}:{defaultChart.Notes[i].Track - 1}\n";
                                    break;
                                }
                        }
                        break;

                    case DefaultNoteType.改变bpm:
                        break;
                }

                index++;
            }

            return str;
        }

        /// <summary>
        ///     输出为BangCraft谱面工程格式
        /// </summary>
        /// <returns></returns>
        private string ToBangCraftScore(DefaultChart defaultChart)
        {
            var xml = new XmlDocument();
            var Save = xml.CreateElement("Save");
            Save.SetAttribute("name", "BGCdate");
            xml.AppendChild(Save);

            var info = xml.CreateElement("info");
            Save.AppendChild(info);

            var music = xml.CreateElement("music");
            music.InnerText = "test";
            info.AppendChild(music);

            var bpm = xml.CreateElement("bpm");
            bpm.InnerText = defaultChart.Bpm.ToString();
            info.AppendChild(bpm);

            var delay = xml.CreateElement("delay");
            delay.InnerText = defaultChart.Delay_ms.ToString();
            info.AppendChild(delay);

            var bpmP1 = xml.CreateElement("bpmP1");
            bpmP1.InnerText = "";
            info.AppendChild(bpmP1);

            var bpmD1 = xml.CreateElement("bpmD1");
            bpmD1.InnerText = "0";
            info.AppendChild(bpmD1);

            var bpmP2 = xml.CreateElement("bpmP2");
            bpmP2.InnerText = "";
            info.AppendChild(bpmP2);

            var bpmD2 = xml.CreateElement("bpmD2");
            bpmD2.InnerText = "0";
            info.AppendChild(bpmD2);

            var bpmP3 = xml.CreateElement("bpmP3");
            bpmP3.InnerText = "";
            info.AppendChild(bpmP3);

            var bpmD3 = xml.CreateElement("bpmD3");
            bpmD3.InnerText = "0";
            info.AppendChild(bpmD3);

            foreach (var note in defaultChart.Notes)
                //N
                if (note.NoteType == DefaultNoteType.白键 || note.NoteType == DefaultNoteType.技能)
                {
                    var noteN = xml.CreateElement("noteN");
                    Save.AppendChild(noteN);

                    var lineN = xml.CreateElement("lineN");
                    lineN.InnerText = (note.Track - 4).ToString();
                    noteN.AppendChild(lineN);

                    var posN = xml.CreateElement("posN");
                    posN.InnerText = (note.Time * 2).ToString();
                    noteN.AppendChild(posN);

                    var typeN = xml.CreateElement("typeN");
                    typeN.InnerText = "N";
                    noteN.AppendChild(typeN);
                }

            foreach (var note in defaultChart.Notes)
                //F
                if (note.NoteType == DefaultNoteType.粉键)
                {
                    var noteF = xml.CreateElement("noteF");
                    Save.AppendChild(noteF);

                    var lineF = xml.CreateElement("lineF");
                    lineF.InnerText = (note.Track - 4).ToString();
                    noteF.AppendChild(lineF);

                    var posF = xml.CreateElement("posF");
                    posF.InnerText = (note.Time * 2).ToString();
                    noteF.AppendChild(posF);

                    var typeF = xml.CreateElement("typeF");
                    typeF.InnerText = "F";
                    noteF.AppendChild(typeF);
                }

            var index = 0;
            foreach (var note in defaultChart.Notes)
            {
                //LS
                if (note.NoteType == DefaultNoteType.滑条a_开始)
                {
                    var noteL = xml.CreateElement("noteL");
                    Save.AppendChild(noteL);

                    var lineL = xml.CreateElement("lineL");
                    lineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(lineL);

                    var posL = xml.CreateElement("posL");
                    posL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(posL);

                    var typeL = xml.CreateElement("typeL");
                    typeL.InnerText = "LS";
                    noteL.AppendChild(typeL);

                    var startlineL = xml.CreateElement("startlineL");
                    startlineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(startlineL);

                    var startposL = xml.CreateElement("startposL");
                    startposL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(startposL);

                    for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                    {
                        if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_中间 && defaultChart.Notes[i].Time != note.Time)
                        {
                            var noteM = xml.CreateElement("noteL");
                            Save.AppendChild(noteM);

                            var lineM = xml.CreateElement("lineL");
                            lineM.InnerText = (defaultChart.Notes[i].Track - 4).ToString();
                            noteM.AppendChild(lineM);

                            var posM = xml.CreateElement("posL");
                            posM.InnerText = (defaultChart.Notes[i].Time * 2).ToString();
                            noteM.AppendChild(posM);

                            var typeM = xml.CreateElement("typeL");
                            typeM.InnerText = "LM";
                            noteM.AppendChild(typeM);

                            noteM.AppendChild(startlineL.Clone());

                            noteM.AppendChild(startposL.Clone());
                            continue;
                        }

                        if ((defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_结束 ||
                             defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_粉键结束) && defaultChart.Notes[i].Time != note.Time)
                        {
                            var noteE = xml.CreateElement("noteL");
                            Save.AppendChild(noteE);

                            var lineE = xml.CreateElement("lineL");
                            lineE.InnerText = (defaultChart.Notes[i].Track - 4).ToString();
                            noteE.AppendChild(lineE);

                            var posE = xml.CreateElement("posL");
                            posE.InnerText = (defaultChart.Notes[i].Time * 2).ToString();
                            noteE.AppendChild(posE);

                            var typeE = xml.CreateElement("typeL");
                            typeE.InnerText = defaultChart.Notes[i].NoteType == DefaultNoteType.滑条a_结束 ? "LE" : "LF";
                            noteE.AppendChild(typeE);

                            noteE.AppendChild(startlineL.Clone());

                            noteE.AppendChild(startposL.Clone());
                            break;
                        }
                    }
                }

                if (note.NoteType == DefaultNoteType.滑条b_开始)
                {
                    var noteL = xml.CreateElement("noteL");
                    Save.AppendChild(noteL);

                    var lineL = xml.CreateElement("lineL");
                    lineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(lineL);

                    var posL = xml.CreateElement("posL");
                    posL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(posL);

                    var typeL = xml.CreateElement("typeL");
                    typeL.InnerText = "LS";
                    noteL.AppendChild(typeL);

                    var startlineL = xml.CreateElement("startlineL");
                    startlineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(startlineL);

                    var startposL = xml.CreateElement("startposL");
                    startposL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(startposL);

                    for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                    {
                        if (defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_中间 && defaultChart.Notes[i].Time != note.Time)
                        {
                            var noteM = xml.CreateElement("noteL");
                            Save.AppendChild(noteM);

                            var lineM = xml.CreateElement("lineL");
                            lineM.InnerText = (defaultChart.Notes[i].Track - 4).ToString();
                            noteM.AppendChild(lineM);

                            var posM = xml.CreateElement("posL");
                            posM.InnerText = (defaultChart.Notes[i].Time * 2).ToString();
                            noteM.AppendChild(posM);

                            var typeM = xml.CreateElement("typeL");
                            typeM.InnerText = "LM";
                            noteM.AppendChild(typeM);

                            noteM.AppendChild(startlineL.Clone());

                            noteM.AppendChild(startposL.Clone());
                            continue;
                        }

                        if ((defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_结束 ||
                             defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_粉键结束) && defaultChart.Notes[i].Time != note.Time)
                        {
                            var noteE = xml.CreateElement("noteL");
                            Save.AppendChild(noteE);

                            var lineE = xml.CreateElement("lineL");
                            lineE.InnerText = (defaultChart.Notes[i].Track - 4).ToString();
                            noteE.AppendChild(lineE);

                            var posE = xml.CreateElement("posL");
                            posE.InnerText = (defaultChart.Notes[i].Time * 2).ToString();
                            noteE.AppendChild(posE);

                            var typeE = xml.CreateElement("typeL");
                            typeE.InnerText = defaultChart.Notes[i].NoteType == DefaultNoteType.滑条b_结束 ? "LE" : "LF";
                            noteE.AppendChild(typeE);

                            noteE.AppendChild(startlineL.Clone());

                            noteE.AppendChild(startposL.Clone());
                            break;
                        }
                    }
                }

                if (note.NoteType == DefaultNoteType.长键_开始)
                {
                    var noteL = xml.CreateElement("noteL");
                    Save.AppendChild(noteL);

                    var lineL = xml.CreateElement("lineL");
                    lineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(lineL);

                    var posL = xml.CreateElement("posL");
                    posL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(posL);

                    var typeL = xml.CreateElement("typeL");
                    typeL.InnerText = "LS";
                    noteL.AppendChild(typeL);

                    var startlineL = xml.CreateElement("startlineL");
                    startlineL.InnerText = (note.Track - 4).ToString();
                    noteL.AppendChild(startlineL);

                    var startposL = xml.CreateElement("startposL");
                    startposL.InnerText = (note.Time * 2).ToString();
                    noteL.AppendChild(startposL);

                    for (var i = index + 1; i < defaultChart.Notes.Count; i++)
                        if ((defaultChart.Notes[i].NoteType == DefaultNoteType.长键_结束 ||
                             defaultChart.Notes[i].NoteType == DefaultNoteType.长键_粉键结束) && defaultChart.Notes[i].Time != note.Time &&
                            defaultChart.Notes[i].Track == note.Track)
                        {
                            var noteE = xml.CreateElement("noteL");
                            Save.AppendChild(noteE);

                            var lineE = xml.CreateElement("lineL");
                            lineE.InnerText = (defaultChart.Notes[i].Track - 4).ToString();
                            noteE.AppendChild(lineE);

                            var posE = xml.CreateElement("posL");
                            posE.InnerText = (defaultChart.Notes[i].Time * 2).ToString();
                            noteE.AppendChild(posE);

                            var typeE = xml.CreateElement("typeL");
                            typeE.InnerText = defaultChart.Notes[i].NoteType == DefaultNoteType.长键_结束 ? "LE" : "LF";
                            noteE.AppendChild(typeE);

                            noteE.AppendChild(startlineL.Clone());

                            noteE.AppendChild(startposL.Clone());
                            break;
                        }
                }

                index++;
            }

            var result = Helper.ConvertXmlDocumentTostring(xml);

            return result;
        }

        /// <summary>
        ///     输出为BMS格式
        /// </summary>
        /// <returns></returns>
        private string ToBMS(DefaultChart defaultChart)
        {
            /*轨道1-7:6123458*/

            /*1通道    白键03 粉键04 技能05 绿条a开始/中间06 绿条a结束07 绿条a粉键结束08 绿条b开始/中间09 绿条b结束0A 绿条b粉键结束0B*/
            /*5通道    长键开始/结束03 长键粉键结束04*/

            var result = "#00001:01" + "\r\n";

            result = result.Insert(0, @"
*---------------------- HEADER FIELD

#PLAYER 1
#GENRE
#TITLE
#ARTIST
" + $"#BPM {defaultChart.Bpm}" + @"
#PLAYLEVEL
#RANK 3

#STAGEFILE

#WAV01 bgm.wav
#WAV03 bd.wav
#WAV04 flick.wav
#WAV05 skill.wav
#WAV06 slide_a.wav
#WAV07 slide_end_a.wav
#WAV08 slide_end_flick_a.wav
#WAV09 slide_b.wav
#WAV0A slide_end_b.wav
#WAV0B slide_end_flick_b.wav
#WAV0C cmd_fever_end.wav
#WAV0D cmd_fever_ready.wav
#WAV0E cmd_fever_start.wav
#WAV0F fever_note.wav
#WAV0G fever_note_flick.wav

#BGM bgm001

*---------------------- MAIN DATA FIELD

");

            var beatCount = 0;

            while (beatCount * 4 < defaultChart.Notes.Last().Time)
            {
                var strList = new List<string>();

                //长键以外
                for (var i = 1; i <= 7; i++)
                {
                    var a = beatCount.ToString().PadLeft(3, '0');
                    var b = "1";
                    var c = "6";
                    switch (i)
                    {
                        case 1:
                            c = "6";
                            break;

                        case 2:
                            c = "1";
                            break;

                        case 3:
                            c = "2";
                            break;

                        case 4:
                            c = "3";
                            break;

                        case 5:
                            c = "4";
                            break;

                        case 6:
                            c = "5";
                            break;

                        case 7:
                            c = "8";
                            break;
                    }

                    var trackNormalNotes = defaultChart.Notes.Where(p =>
                            p.Time >= beatCount * 4 && p.Time < (beatCount + 1) * 4 && p.Track == i &&
                            p.NoteType != DefaultNoteType.长键_开始 && p.NoteType != DefaultNoteType.长键_结束 &&
                            p.NoteType != DefaultNoteType.长键_粉键结束)
                        .OrderBy(p => p.Time)
                        .ToList();
                    if (!trackNormalNotes.Any())
                        continue;
                    var fractions = trackNormalNotes.Select(p => p.Time % 4 / 4).ToList().ConvertToFraction();
                    var d = new string('0', fractions[0].Item2 * 2);
                    for (var j = 0; j < trackNormalNotes.Count(); j++)
                    {
                        d = d.Remove(fractions[j].Item1 * 2, 2);
                        var typeText = "03";
                        switch (trackNormalNotes[j].NoteType)
                        {
                            case DefaultNoteType.白键:
                                typeText = "03";
                                break;

                            case DefaultNoteType.粉键:
                                typeText = "04";
                                break;

                            case DefaultNoteType.技能:
                                typeText = "05";
                                break;

                            case DefaultNoteType.滑条a_开始:
                            case DefaultNoteType.滑条a_中间:
                                typeText = "06";
                                break;

                            case DefaultNoteType.滑条a_结束:
                                typeText = "07";
                                break;

                            case DefaultNoteType.滑条a_粉键结束:
                                typeText = "08";
                                break;

                            case DefaultNoteType.滑条b_开始:
                            case DefaultNoteType.滑条b_中间:
                                typeText = "09";
                                break;

                            case DefaultNoteType.滑条b_结束:
                                typeText = "0A";
                                break;

                            case DefaultNoteType.滑条b_粉键结束:
                                typeText = "0B";
                                break;

                            case DefaultNoteType.长键_开始:
                            case DefaultNoteType.长键_结束:
                                typeText = "03";
                                break;

                            case DefaultNoteType.长键_粉键结束:
                                typeText = "04";
                                break;
                        }

                        d = d.Insert(fractions[j].Item1 * 2, typeText);
                    }

                    var line = $"#{a}{b}{c}:{d}";

                    strList.Add(line);
                }

                foreach (var str in strList.OrderBy(p => p)) result += str + "\r\n";

                strList = new List<string>();

                //长键
                for (var i = 1; i <= 7; i++)
                {
                    var a = beatCount.ToString().PadLeft(3, '0');
                    var b = "5";
                    var c = "6";
                    switch (i)
                    {
                        case 1:
                            c = "6";
                            break;

                        case 2:
                            c = "1";
                            break;

                        case 3:
                            c = "2";
                            break;

                        case 4:
                            c = "3";
                            break;

                        case 5:
                            c = "4";
                            break;

                        case 6:
                            c = "5";
                            break;

                        case 7:
                            c = "8";
                            break;
                    }

                    var trackNormalNotes = defaultChart.Notes.Where(p =>
                            p.Time >= beatCount * 4 && p.Time < (beatCount + 1) * 4 && p.Track == i && (
                                p.NoteType == DefaultNoteType.长键_开始 || p.NoteType == DefaultNoteType.长键_结束 ||
                                p.NoteType == DefaultNoteType.长键_粉键结束))
                        .OrderBy(p => p.Time)
                        .ToList();
                    if (!trackNormalNotes.Any())
                        continue;

                    var fractions = trackNormalNotes.Select(p => p.Time % 4 / 4).ToList().ConvertToFraction();
                    var d = new string('0', fractions[0].Item2 * 2);
                    for (var j = 0; j < trackNormalNotes.Count(); j++)
                    {
                        d = d.Remove(fractions[j].Item1 * 2, 2);
                        var typeText = "03";
                        switch (trackNormalNotes[j].NoteType)
                        {
                            case DefaultNoteType.长键_开始:
                            case DefaultNoteType.长键_结束:
                                typeText = "03";
                                break;

                            case DefaultNoteType.长键_粉键结束:
                                typeText = "04";
                                break;
                        }

                        d = d.Insert(fractions[j].Item1 * 2, typeText);
                    }

                    var line = $"#{a}{b}{c}:{d}";

                    strList.Add(line);
                }

                foreach (var str in strList.OrderBy(p => p)) result += str + "\r\n";

                result += "\r\n";
                beatCount++;
            }

            return result;
        }


        #endregion

        #region 内部方法

        /// <summary>
        ///     检查重复note
        /// </summary>
        /// <param name="defaultScore"></param>
        private void CheckRepeat(DefaultChart defaultScore)
        {
            var repeatList = defaultScore.Notes.GroupBy(p => new { p.Time, p.Track }).Where(p => p.Count() > 1).ToList();

            var str = "位于\r\n";
            foreach (var i in repeatList) str += $"Time:{i.Key.Time} Track:{i.Key.Track} NoteCount:{i.Count()}\r\n";

            str += "转换过程不作处理,请在原谱面文件上自行修改";

            if (repeatList.Count != 0);
        }

        /// <summary>
        ///     直ab绿条转长键
        /// </summary>
        /// <param name="defaultScore"></param>
        private void GenLongNote(DefaultChart defaultScore)
        {
            var notes = defaultScore.Notes;
            for (var i = 0; i < notes.Count; i++)
            {
                if (notes[i].NoteType == DefaultNoteType.滑条a_开始)
                {
                    var isLong = false;
                    for (var j = i + 1; j < notes.Count; j++)
                    {
                        if (notes[j].NoteType == DefaultNoteType.滑条a_中间)
                        {
                            break;
                        }

                        if (notes[j].NoteType == DefaultNoteType.滑条a_结束)
                        {
                            if (notes[j].Track == notes[i].Track)
                            {
                                isLong = true;
                                notes[j].NoteType = DefaultNoteType.长键_结束;
                            }
                            break;
                        }

                        if (notes[j].NoteType == DefaultNoteType.滑条a_粉键结束)
                        {
                            if (notes[j].Track == notes[i].Track)
                            {
                                isLong = true;
                                notes[j].NoteType = DefaultNoteType.长键_粉键结束;
                            }
                            break;
                        }
                    }

                    if (isLong)
                        notes[i].NoteType = DefaultNoteType.长键_开始;
                }

                if (notes[i].NoteType == DefaultNoteType.滑条b_开始)
                {
                    var isLong = false;
                    for (var j = i + 1; j < notes.Count; j++)
                    {
                        if (notes[j].NoteType == DefaultNoteType.滑条b_中间)
                        {
                            break;
                        }

                        if (notes[j].NoteType == DefaultNoteType.滑条b_结束)
                        {
                            if (notes[j].Track == notes[i].Track)
                            {
                                isLong = true;
                                notes[j].NoteType = DefaultNoteType.长键_结束;
                            }
                            break;
                        }

                        if (notes[j].NoteType == DefaultNoteType.滑条b_粉键结束)
                        {
                            if (notes[j].Track == notes[i].Track)
                            {
                                isLong = true;
                                notes[j].NoteType = DefaultNoteType.长键_粉键结束;
                            }
                            break;
                        }
                    }

                    if (isLong)
                        notes[i].NoteType = DefaultNoteType.长键_开始;
                }
            }
        }

        /// <summary>
        ///		修复首尾连接同类型滑条
        /// </summary>
        /// <param name="defaultScore"></param>
        private void FixSamePosSlide(DefaultChart defaultScore)
        {
            var notes = defaultScore.Notes;
            for (var i = 0; i < notes.Count; i++)
            {
                if (notes[i].NoteType == DefaultNoteType.滑条a_结束)
                {
                    var sameTimeNotes = notes.Where(p => p.Time == notes[i].Time && p.Track != notes[i].Track).ToList();
                    if (sameTimeNotes.Count == 1 && sameTimeNotes[0].NoteType == DefaultNoteType.滑条a_开始)
                    {
                        sameTimeNotes[0].NoteType = DefaultNoteType.滑条b_开始;
                        for (int j = i + 1; j < notes.Count; j++)
                        {
                            if (notes[j].NoteType == DefaultNoteType.滑条a_中间)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条b_中间;
                                continue;
                            }

                            if (notes[j].NoteType == DefaultNoteType.滑条a_结束)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条b_结束;
                                break;
                            }

                            if (notes[j].NoteType == DefaultNoteType.滑条a_粉键结束)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条b_粉键结束;
                                break;
                            }
                        }
                    }
                }

                if (notes[i].NoteType == DefaultNoteType.滑条b_结束)
                {
                    var sameTimeNotes = notes.Where(p => p.Time == notes[i].Time && p.Track != notes[i].Track).ToList();
                    if (sameTimeNotes.Count == 1 && sameTimeNotes[0].NoteType == DefaultNoteType.滑条b_开始)
                    {
                        sameTimeNotes[0].NoteType = DefaultNoteType.滑条a_开始;
                        for (int j = i + 1; j < notes.Count; j++)
                        {
                            if (notes[j].NoteType == DefaultNoteType.滑条b_中间)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条a_中间;
                                continue;
                            }

                            if (notes[j].NoteType == DefaultNoteType.滑条b_结束)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条a_结束;
                                break;
                            }

                            if (notes[j].NoteType == DefaultNoteType.滑条b_粉键结束)
                            {
                                notes[j].NoteType = DefaultNoteType.滑条a_粉键结束;
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
