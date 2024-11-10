using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ZongziTEK_Blackboard_Sticker
{
    public class Settings
    {
        public Storage Storage { get; set; } = new Storage();
        public Look Look { get; set; } = new Look();
        public TimetableSettings TimetableSettings { get; set; } = new TimetableSettings();
        public Blackboard Blackboard { get; set; } = new Blackboard();
        public InfoBoard InfoBoard { get; set; } = new InfoBoard();
        public Automation Automation { get; set; } = new Automation();
        public Update Update { get; set; } = new Update();
    }

    public class Storage
    {
        public bool IsFilesSavingWithProgram { get; set; } = true;
        public string DataPath { get; set; } = "D:\\ZongziTEK_Blackboard_Sticker_Data";
    }

    public class Look
    {
        public double WindowScaleMultiplier { get; set; } = 1;
        public int Theme { get; set; } = 0; // 0 代表自动切换，1 代表浅色，2 代表深色
        public bool IsAnimationEnhanced { get; set; } = true;
        //public bool UseLiteMode { get; set; } = false;
        //public bool IsLiteModeWithInfoBoard { get; set; } = false;
        public int LookMode { get; set; } = 0; // 0 代表默认模式，1 代表顶部为时钟的简约模式，2 代表顶部为信息看板的简约模式
    }

    public class TimetableSettings
    {
        public bool IsTimetableEnabled { get; set; } = true;
        public bool IsTimetableNotificationEnabled { get; set; } = true;
        public double FontSize { get; set; } = 24;
        public double BeginNotificationTime { get; set; } = 60;
        public bool IsBeginSpeechEnabled { get; set; } = false;
        public double OverNotificationTime { get; set; } = 10;
        public bool IsOverSpeechEnabled { get; set; } = false;
        public int Voice { get; set; } = 55;
        public double TimeOffset { get; set; } = 0; // 秒
    }

    public class Blackboard
    {
        public bool IsLocked { get; set; } = false;
    }

    public class InfoBoard
    {
        public bool isCountdownPageEnabled { get; set; } = true;
        public bool isDatePageEnabled { get; set; } = true;
        public bool isWeatherPageEnabled { get; set; } = true;
        public bool isWeatherForecastPageEnabled { get; set; } = true;
        public string CountdownName { get; set; } = "高考";
        public DateTime CountdownDate { get; set; } = DateTime.Parse("2025/6/7");
        public int CountdownWarnDays { get; set; } = 30;
        public string WeatherCity { get; set; } = "北京";
    }

    public class Automation
    {
        public bool IsAutoHideHugoAssistantEnabled { get; set; } = false;
        public bool IsBottomMost { get; set; } = true;
    }

    public class Update
    {
        public bool IsUpdateAutomatic { get; set; } = true;
        public int UpdateChannel { get; set; } = 0; // 0 代表 Release 频道，1 代表 Preview 频道
    }
}
