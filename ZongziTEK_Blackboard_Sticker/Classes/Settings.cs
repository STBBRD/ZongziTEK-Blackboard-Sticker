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
        public bool isFilesSavingWithProgram { get; set; } = true;
        public string dataPath { get; set; } = "D:\\ZongziTEK_Blackboard_Sticker_Data";
    }

    public class Look
    {
        public bool IsLightTheme { get; set; } = false;
        public bool IsSwitchThemeAuto { get; set; } = true;
    }

    public class TimetableSettings
    {
        public bool isTimetableEnabled { get; set; } = true;
        public bool isTimetableNotificationEnabled { get; set; } = true;
        public bool useDefaultBNSPath = true;
        public string BNSPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackboard Notification Service";
    }

    public class Blackboard
    {
        public bool isLocked { get; set; } = false;
    }
}
