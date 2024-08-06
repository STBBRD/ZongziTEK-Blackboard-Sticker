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
using System.Windows.Shapes;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string title, string question, string defaultAnswer = "")
        {
            InitializeComponent();
            this.Title = title;
            lblQuestion.Content = question;
            txtAnswer.Text = defaultAnswer;

            // 处理DPI变化
            this.Loaded += InputDialog_Loaded;
        }

        private void InputDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取当前DPI
            var dpi = VisualTreeHelper.GetDpi(this);
            // 调整窗口大小和组件位置
            AdjustForDpi(dpi.PixelsPerDip);
        }

        private void AdjustForDpi(double dpiScale)
        {
            // 调整窗口大小
            this.Width = 300 * dpiScale;
            this.Height = 150 * dpiScale;

            // 调整组件位置和大小
            lblQuestion.Margin = new Thickness(10 * dpiScale);
            txtAnswer.Margin = new Thickness(10 * dpiScale);
            txtAnswer.Width = 260 * dpiScale;
            btnDialogOk.Margin = new Thickness(10 * dpiScale);
            btnDialogOk.Width = 75 * dpiScale;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }
    }



}
