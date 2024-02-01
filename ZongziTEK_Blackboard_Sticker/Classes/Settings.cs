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
    }

    public class Storage
    {
        public bool IsFilesSavingWithProgram { get; set; } = true;
        public string DataPath { get; set; } = "D:\\ZongziTEK_Blackboard_Sticker_Data";
    }

    public class Look
    {
        public bool IsLightTheme { get; set; } = false;
        public bool IsSwitchThemeAuto { get; set; } = true;
        public bool UseLiteMode { get; set; } = false;
    }

    public class TimetableSettings
    {
        public bool IsTimetableEnabled { get; set; } = true;
        public bool IsTimetableNotificationEnabled { get; set; } = true;
        public bool UseDefaultBNSPath = true;
        public string BNSPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackboard Notification Service";
    }

    public class Blackboard
    {
        public bool IsLocked { get; set; } = false;
    }
}
