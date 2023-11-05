using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Windows.Gaming.XboxLive;

namespace ZongziTEK_Blackboard_Sticker
{
    public class Settings
    {
        public Storage Storage { get; set; } = new Storage();
        public Look Look { get; set; } = new Look();
        public TimetableSettings TimetableSettings { get; set; } = new TimetableSettings();
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
        public bool useTimetable { get; set; } = true;
    }
}
