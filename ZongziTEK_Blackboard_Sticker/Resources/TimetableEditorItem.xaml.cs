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

namespace ZongziTEK_Blackboard_Sticker.Resources
{
    /// <summary>
    /// TimetableEditorItem.xaml 的交互逻辑
    /// </summary>
    public partial class TimetableEditorItem : UserControl
    {
        public TimetableEditorItem()
        {
            InitializeComponent();

            if (TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockHintSubject.Visibility = Visibility.Hidden;
            }
        }

        public event EventHandler LessonInfoChanged;

        public static readonly DependencyProperty SubjectProperty =
        DependencyProperty.Register("Subject", typeof(string), typeof(TimetableEditorItem));

        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(string), typeof(TimetableEditorItem));

        public string StartTime
        {
            get { return (string)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(string), typeof(TimetableEditorItem));

        public string EndTime
        {
            get { return (string)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        private void TextBoxSubject_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlockHintSubject.Visibility = Visibility.Hidden;
        }

        private void TextBoxSubject_LostFocus(object sender, RoutedEventArgs e)
        {
            if(TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
        }

        private void OnLessonInfoChanged()
        {
            EventHandler handler = LessonInfoChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void TextBoxSubject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSubject.Text == "")
            {
                TextBlockHintSubject.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockHintSubject.Visibility = Visibility.Hidden;
            }
            Subject = TextBoxSubject.Text;
            OnLessonInfoChanged();
        }

        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartTime = StartTimeTextBox.Text;
            OnLessonInfoChanged();
        }

        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EndTime = EndTimeTextBox.Text;
            OnLessonInfoChanged();
        }
    }
}
