using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZongziTEK_Blackboard_Sticker
{
    public class Timetable
    {
        public List<Lesson> Monday { get; set; } = new List<Lesson>();
        public List<Lesson> Tuesday { get; set; } = new List<Lesson>();
        public List<Lesson> Wednesday { get; set; } = new List<Lesson>();
        public List<Lesson> Thursday { get; set; } = new List<Lesson>();
        public List<Lesson> Friday { get; set; } = new List<Lesson>();
        public List<Lesson> Saturday { get; set; } = new List<Lesson>();
        public List<Lesson> Sunday { get; set; } = new List<Lesson>();
        public List<Lesson> Temp { get; set; } = new List<Lesson>();

        public string ToCurriculums(List<Lesson> list)
        {
            string curriculums = "";
            if (list.Count > 0)
            {
                foreach (Lesson lesson in list)
                {
                    curriculums += lesson.Subject + "\n";
                }
                if (curriculums.Length > 0) curriculums = curriculums.Remove(curriculums.Length - 1);
            }
            else
            {
                curriculums = "无课程";
            }
            return curriculums;
        }

        public void Sort(Timetable timetable)
        {
            foreach (List<Lesson> day in new[] { timetable.Monday, timetable.Tuesday, timetable.Wednesday, timetable.Thursday, timetable.Friday, timetable.Saturday, timetable.Sunday, timetable.Temp })
            {
                day.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            }
        }
    }

    public class Lesson
    {
        public string Subject { get; set; } = "";
        public TimeSpan StartTime { get; set; } = new TimeSpan();
        public TimeSpan EndTime { get; set; } = new TimeSpan();
        public bool IsSplitBelow { get; set; } = false;
        public bool IsStrongClassOverNotificationEnabled { get; set; } = false;

        public Lesson(string subject, TimeSpan startTime, TimeSpan endTime, bool isSplitBelow, bool isStrongClassOverNotificationEnabled)
        {
            Subject = subject;
            StartTime = startTime;
            EndTime = endTime;
            IsSplitBelow = isSplitBelow;
            IsStrongClassOverNotificationEnabled = isStrongClassOverNotificationEnabled;
        }
    }
}
