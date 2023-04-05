using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Gaming.XboxLive;

namespace ZongziTEK_Blackboard_Sticker
{
    public class Settings
    {
        public Storage Storage { get; set; } = new Storage();
    }

    public class Storage
    {
        public bool isFilesSavingWithProgram { get; set; } = true;
        public string dataPath { get; set; } = "D:\\ZongziTEK_Blackboard_Sticker_Data";
    }
}
