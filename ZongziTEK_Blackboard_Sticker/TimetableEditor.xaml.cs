using Microsoft.Win32;
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
using System.Windows.Media.Animation;
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

            AutoSelectTimetableToEdit();
        }

        #region Window & Controls
        public static event Action EditorButtonUseCurriculum_Click;

        private bool isCloseWithoutWarning = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isCloseWithoutWarning)
            {
                e.Cancel = true;
                if (!isEdited || MessageBox.Show("确定要直接关闭课程表编辑器吗\n这将丢失未保存的课程", "关闭而不保存", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
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
            if (isEdited || MessageBox.Show("是否保存当前含时间信息的课程表内容", "ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                ButtonSave_Click(null, null);
            }
            MessageBox.Show("若后续需要使用含时间信息的课程表，请在设置中启用", "ZongziTEK 黑板贴");
            EditorButtonUseCurriculum_Click?.Invoke();
            isCloseWithoutWarning = true;
            Close();
        }

        private async void ComboBoxDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTimetable();
            if (ComboBoxDay.SelectedIndex == 7)
            {
                InfoBarTempTimetable.IsOpen = true;

                DoubleAnimation heightAnimation = new()
                {
                    From = 0,
                    To = 50,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };
                InfoBarTempTimetable.BeginAnimation(HeightProperty, heightAnimation);
            }
            else if (InfoBarTempTimetable.IsOpen)
            {
                DoubleAnimation heightAnimation = new()
                {
                    From = 50,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
                };
                InfoBarTempTimetable.BeginAnimation(HeightProperty, heightAnimation);

                await Task.Delay(500);
                InfoBarTempTimetable.IsOpen = false;
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
                    Lesson lesson = new Lesson(changedItem.Subject, TimeSpan.Parse(changedItem.StartTime), TimeSpan.Parse(changedItem.EndTime), changedItem.IsSplitBelow);
                    GetSelectedDay()[index] = lesson;
                    if (lesson.IsSplitBelow)
                    {
                        changedItem.Margin = new Thickness(0, 0, 0, 8);
                    }
                    else
                    {
                        changedItem.Margin = new Thickness(0);
                    }
                }
                catch { }
            }

            isEdited = true;
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

            isEdited = true;

            SetButtonCopyIsEnabled();
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();
        }

        private void ButtonInsertLesson_Click(object sender, RoutedEventArgs e)
        {
            TimetableEditorItem item = new TimetableEditorItem();
            item.LessonInfoChanged += Item_LessonInfoChanged;
            item.LessonDeleting += Item_LessonDeleting;
            GetSelectedDay().Add(new Lesson("", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), false));
            ListStackPanel.Children.Add(item);

            isEdited = true;

            SetButtonCopyIsEnabled();
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();
        }

        private void ScrollViewerTimetable_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollViewerTimetable.LineUp();
            else ScrollViewerTimetable.LineDown();
        }
        #endregion

        #region Load & Save
        private Timetable Timetable = MainWindow.Timetable;
        private bool isEdited = false;

        private async void LoadTimetable()
        {
            TextBlockLessonCount.Text = GetSelectedDay().Count.ToString();

            ListStackPanel.Children.Clear();

            ScrollViewerTimetable.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            ScrollViewerTimetable.ScrollToTop();

            foreach (Lesson lesson in GetSelectedDay())
            {
                TimetableEditorItem item = new TimetableEditorItem()
                {
                    Subject = lesson.Subject,
                    StartTime = lesson.StartTime.ToString(@"hh\:mm"),
                    EndTime = lesson.EndTime.ToString(@"hh\:mm"),
                    IsSplitBelow = lesson.IsSplitBelow
                };
                if (lesson.IsSplitBelow)
                {
                    item.Margin = new Thickness(0, 0, 0, 8);
                }
                item.LessonInfoChanged += Item_LessonInfoChanged;
                item.LessonDeleting += Item_LessonDeleting;
                ListStackPanel.Children.Add(item);

                if (MainWindow.Settings.Look.IsAnimationEnhanced)
                {
                    DoubleAnimation opacityAnimation = new()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                    };
                    item.BeginAnimation(OpacityProperty, opacityAnimation);

                    ThicknessAnimation marginAnimation = new()
                    {
                        From = new Thickness(0, 24, 0, item.Margin.Bottom),
                        To = new Thickness(0, 0, 0, item.Margin.Bottom),
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                    };
                    item.BeginAnimation(MarginProperty, marginAnimation);

                    await Task.Delay(25);
                }
            }

            ScrollViewerTimetable.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            SetButtonCopyIsEnabled();
        }
        #endregion

        #region ClipBoard
        private List<Lesson> ClipBoard = new List<Lesson>();

        private async void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipBoard = GetSelectedDay().ToList();

            if (ButtonCopy.ActualWidth > 32)
            {
                DoubleAnimation buttonCopyWidthAnimation = new()
                {
                    From = ButtonCopy.ActualWidth,
                    To = 32,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonCopy.BeginAnimation(WidthProperty, buttonCopyWidthAnimation);

                ThicknessAnimation buttonCopyPaddingAnimation = new()
                {
                    From = new Thickness(10, 0, 10, 0),
                    To = new Thickness(7, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonCopy.BeginAnimation(PaddingProperty, buttonCopyPaddingAnimation);

                DoubleAnimation buttonPasteWidthAnimation = new()
                {
                    From = 0,
                    To = 32,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonPaste.BeginAnimation(WidthProperty, buttonPasteWidthAnimation);
                ButtonPaste.Visibility = Visibility.Visible;

                DoubleAnimation buttonPasteOpacityAnimation = new()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
                };
                ButtonPaste.BeginAnimation(OpacityProperty, buttonPasteOpacityAnimation);

                await Task.Delay(500);
                LabelCopy.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedDay().Count > 0)
            {
                if (MessageBox.Show("这里原有的课程将被覆盖", "确定要粘贴吗？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            GetSelectedDay().Clear();
            foreach (Lesson lesson in ClipBoard)
            {
                GetSelectedDay().Add(lesson);
            }
            LoadTimetable();
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

        private void AutoSelectTimetableToEdit()
        {

            switch (MainWindow.timetableToShow_index)
            {
                case 1: // 周一
                    ComboBoxDay.SelectedIndex = 0;
                    break;
                case 2: // 周二
                    ComboBoxDay.SelectedIndex = 1;
                    break;
                case 3: // 周三
                    ComboBoxDay.SelectedIndex = 2;
                    break;
                case 4: // 周四
                    ComboBoxDay.SelectedIndex = 3;
                    break;
                case 5: // 周五
                    ComboBoxDay.SelectedIndex = 4;
                    break;
                case 6: // 周六
                    ComboBoxDay.SelectedIndex = 5;
                    break;
                case 0: // 周日
                    ComboBoxDay.SelectedIndex = 6;
                    break;
                case 7: // 临时
                    ComboBoxDay.SelectedIndex = 7;
                    break;
            }
        }

        private void SetButtonCopyIsEnabled()
        {
            if (GetSelectedDay().Count == 0)
            {
                ButtonCopy.IsEnabled = false;
            }
            else
            {
                ButtonCopy.IsEnabled = true;
            }
        }
        #endregion

        #region Import & Export
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "导出课程表",
                FileName = "Timetable.json",
                Filter = "JSON 课程表文件 (*.json)|*.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string text = JsonConvert.SerializeObject(Timetable, Formatting.Indented);
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, text);
                }
                catch { }
            }
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "导入课程表",
                FileName = "Timetable.json",
                Filter = "JSON 课程表文件 (*.json)|*.json|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        if (MessageBox.Show("这里原有的课程将被覆盖", "确定要导入吗？", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            string text = File.ReadAllText(openFileDialog.FileName);
                            Timetable = JsonConvert.DeserializeObject<Timetable>(text);
                            LoadTimetable();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        #endregion
    }
}
