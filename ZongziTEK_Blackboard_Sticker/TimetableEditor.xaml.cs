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
        
        public static event Action EditorButtonUseCurriculum_Click;

        private bool isCloseWithButtonUseCurriculum = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isCloseWithButtonUseCurriculum)
            {
                e.Cancel = true;
                if (MessageBox.Show("确定直接关闭课程表编辑器吗\n这将丢失未保存的课程", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    e.Cancel = false;
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonUseCurriculum_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("是否保存当前含时间信息的课程表内容","ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                ButtonSave_Click(null, null);
            }
            MessageBox.Show("若后续需要使用含时间信息的课程表，请在设置中启用","ZongziTEK 黑板贴");
            EditorButtonUseCurriculum_Click?.Invoke();
            isCloseWithButtonUseCurriculum = true;
            Close();
        }
    }
}
