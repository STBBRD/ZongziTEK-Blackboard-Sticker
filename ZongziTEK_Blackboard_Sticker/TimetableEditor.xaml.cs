using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// TimetableEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableEditor : Window
    {
        public TimetableEditor()
        {
            InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //Timetable timetable = new Timetable();
            //timetable.Tuesday = new List<Lesson>();
            //timetable.Tuesday.Add(new Lesson("数学", new TimeSpan(14, 1, 0), new TimeSpan(14, 2, 0)));

            //string text = JsonConvert.SerializeObject(timetable, Formatting.Indented);
            //try
            //{
            //    File.WriteAllText("D:\\Repos\\ZongziTEK_Blackboard_Sticker\\ZongziTEK_Blackboard_Sticker\\bin\\Release\\Timetable.json", text);
            //}
            //catch { }
        }
    }
}
