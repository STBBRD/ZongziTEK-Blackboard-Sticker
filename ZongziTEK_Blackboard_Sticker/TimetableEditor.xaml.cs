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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

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

            ComboBoxDay.SelectedIndex = 7;
            ChangeComboBoxDaySelectedIndexToday();
        }

        #region Window & Controls
        public static event Action EditorButtonUseCurriculum_Click;

        private bool isCloseWithoutWarning = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isCloseWithoutWarning)
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
            string text = JsonConvert.SerializeObject(Timetable, Formatting.Indented);
            try
            {
                File.WriteAllText(MainWindow.GetDataPath() + MainWindow.timetableFileName, text);
            }
            catch { }

            isCloseWithoutWarning = true;
            Close();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonUseCurriculum_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否保存当前含时间信息的课程表内容", "ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                ButtonSave_Click(null, null);
            }
            MessageBox.Show("若后续需要使用含时间信息的课程表，请在设置中启用", "ZongziTEK 黑板贴");
            EditorButtonUseCurriculum_Click?.Invoke();
            isCloseWithoutWarning = true;
            Close();
        }

        private void ComboBoxDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTimetable();
            if (ComboBoxDay.SelectedIndex == 7)
            {
                TextBlockHintTempTimetable.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockHintTempTimetable.Visibility = Visibility.Collapsed;
            }
        }

        private void Item_LessonInfoChanged(object sender, EventArgs e)
        {
            if (sender is TimetableEditorItem)
            {
                TimetableEditorItem changedItem = sender as TimetableEditorItem;
                int index = ListStackPanel.Children.IndexOf(changedItem);

                try
                {
                    Lesson lesson = new Lesson(changedItem.Subject, TimeSpan.Parse(changedItem.StartTime), TimeSpan.Parse(changedItem.EndTime));
                    GetSelectedDay()[index] = lesson;
                }
                catch { }
            }
        }

        private void Item_LessonDeleting(object sender, EventArgs e)
        {
            if (sender is TimetableEditorItem)
            {
                TimetableEditorItem itemToDelete = sender as TimetableEditorItem;
                int index = ListStackPanel.Children.IndexOf(itemToDelete);

                ListStackPanel.Children.RemoveAt(index);
                GetSelectedDay().RemoveAt(index);
            }
        }

        private void ButtonInsertLesson_Click(object sender, RoutedEventArgs e)
        {
            TimetableEditorItem item = new TimetableEditorItem();
            item.LessonInfoChanged += Item_LessonInfoChanged;
            item.LessonDeleting += Item_LessonDeleting;
            GetSelectedDay().Add(new Lesson("", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0)));
            ListStackPanel.Children.Add(item);
        }
        #endregion

        #region Load & Save
        Timetable Timetable = MainWindow.Timetable;

        private void LoadTimetable()
        {
            ListStackPanel.Children.Clear();
            foreach (Lesson lesson in GetSelectedDay())
            {
                TimetableEditorItem item = new TimetableEditorItem()
                {
                    Subject = lesson.Subject,
                    StartTime = lesson.StartTime.ToString(@"hh\:mm"),
                    EndTime = lesson.EndTime.ToString(@"hh\:mm")
                };
                item.LessonInfoChanged += Item_LessonInfoChanged;
                item.LessonDeleting += Item_LessonDeleting;
                ListStackPanel.Children.Add(item);
            }
        }
        #endregion

        #region Other Functions
        private List<Lesson> GetSelectedDay()
        {
            switch (ComboBoxDay.SelectedIndex)
            {
                case 0:
                    return Timetable.Monday;
                case 1:
                    return Timetable.Tuesday;
                case 2:
                    return Timetable.Wednesday;
                case 3:
                    return Timetable.Thursday;
                case 4:
                    return Timetable.Friday;
                case 5:
                    return Timetable.Saturday;
                case 6:
                    return Timetable.Sunday;
                case 7:
                    return Timetable.Temp;
            }
            return Timetable.Monday;
        }

        private void ChangeComboBoxDaySelectedIndexToday()
        {

            string day = DateTime.Today.DayOfWeek.ToString();
            switch (day)
            {
                case "Monday":
                    ComboBoxDay.SelectedIndex = 0;
                    break;
                case "Tuesday":
                    ComboBoxDay.SelectedIndex = 1;
                    break;
                case "Wednesday":
                    ComboBoxDay.SelectedIndex = 2;
                    break;
                case "Thursday":
                    ComboBoxDay.SelectedIndex = 3;
                    break;
                case "Friday":
                    ComboBoxDay.SelectedIndex = 4;
                    break;
                case "Saturday":
                    ComboBoxDay.SelectedIndex = 5;
                    break;
                case "Sunday":
                    ComboBoxDay.SelectedIndex = 6;
                    break;
            }
        }
        #endregion
    }
}
