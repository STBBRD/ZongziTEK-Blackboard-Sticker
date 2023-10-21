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
            foreach (Lesson lesson in list)
            {
                curriculums += lesson.Subject + "\n";
            }
            if (curriculums.Length > 0) curriculums = curriculums.Remove(curriculums.Length - 1);
            return curriculums;
        }
    }

    public class Lesson
    {
        public string Subject { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public Lesson(string subject, TimeSpan startTime, TimeSpan endTime)
        {
            Subject = subject;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
