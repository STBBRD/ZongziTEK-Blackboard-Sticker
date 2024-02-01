using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ZongziTEK_Blackboard_Sticker.Pages
{
    /// <summary>
    /// CountdownPage.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownPage : Page
    {
        public CountdownPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private DispatcherTimer timer;

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = MainWindow.Settings.InfoBoard.CountdownDate - DateTime.Now;
            if (timeSpan.Days < 0)
            {
                LabelDays.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 204, 0));
                LabelName.Content = MainWindow.Settings.InfoBoard.CountdownName + "已开始";
                timeSpan = -timeSpan;
            }
            else
            {
                if(timeSpan.Days<MainWindow.Settings.InfoBoard.CountdownWarnDays) LabelDays.Foreground = Brushes.Red;
                else LabelDays.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 204, 0));
                LabelName.Content = "距离" + MainWindow.Settings.InfoBoard.CountdownName + "还有";
            }
            LabelDays.Content = timeSpan.Days;
            LabelDaysDetail.Content = (timeSpan.TotalDays - timeSpan.Days).ToString(".000");
        }
    }
}
