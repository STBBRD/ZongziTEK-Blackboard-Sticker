using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.UI.Popups;
using ZongziTEK_Blackboard_Sticker.Properties;
using File = System.IO.File;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DrawingAttributes drawingAttributes;
        public MainWindow()
        {
            InitializeComponent();
            drawingAttributes = new DrawingAttributes();
            inkCanvas.DefaultDrawingAttributes = drawingAttributes;
            drawingAttributes.Color = Colors.White;
            drawingAttributes.Width = 2;
            drawingAttributes.Height = 1.5;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.FitToCurve = true;
            drawingAttributes.IgnorePressure = true;
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString(("HH:mm:ss"));
            LoadCurriculum();
            LoadStrokes();
            Height = System.Windows.SystemParameters.WorkArea.Height;
            Width = System.Windows.SystemParameters.WorkArea.Width / 2;
            Top = 0;
            Left = SystemParameters.WorkArea.Width - Width;
            ColumnLauncher.Width = new GridLength(Width * 0.3);

            rowZero.MaxHeight = Height - 114;

            clockTimer = new DispatcherTimer();
            clockTimer.Tick += new EventHandler(Clock);
            clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            clockTimer.Start();
        }
        private void window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveCurriculum();
        }
        #region Ink Canvas
        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            borderClearConfirm.Visibility = Visibility.Visible;
            touchGrid.Visibility = Visibility.Collapsed;
        }
        private void btnClearCancel_Click(object sender, RoutedEventArgs e)
        {
            borderClearConfirm.Visibility = Visibility.Collapsed;
            touchGrid.Visibility = Visibility.Visible;
        }

        private void btnClearOK_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Strokes.Clear();

            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "sticker.icstk"))
            {
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "sticker.icstk");
            }
            borderClearConfirm.Visibility = Visibility.Collapsed;
            touchGrid.Visibility = Visibility.Visible;
        }
        private void SaveStrokes()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            FileStream fileStream = new FileStream(path + "sticker.icstk", FileMode.Create);
            inkCanvas.Strokes.Save(fileStream);
            fileStream.Close();
        }
        private void LoadStrokes()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(path + "sticker.icstk"))
            {
                FileStream fileStream = new FileStream(path + "sticker.icstk", FileMode.Open);
                inkCanvas.Strokes = new StrokeCollection(fileStream);
                fileStream.Close();
            }
        }
        private void inkCanvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(0.8, 0.8, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(1.2, 0);
                    stroke.Transform(matrix, false);
                }
            }
            else
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(1.25, 1.25, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(0, 1.2);
                    stroke.Transform(matrix, false);
                }
            }
            SaveStrokes();
        }

        private List<int> dec = new List<int>(); //记录触摸设备ID
        Point centerPoint; //中心点

        private void touchGrid_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = inkCanvas;
            e.Mode = ManipulationModes.All;
        }

        private void touchGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (dec.Count >= 2)
            {
                ManipulationDelta md = e.DeltaManipulation;
                Vector trans = md.Translation;  // 获得位移矢量
                double rotate = md.Rotation;  // 获得旋转角度
                Vector scale = md.Scale;  // 获得缩放倍数

                Matrix m = new Matrix();

                // Find center of element and then transform to get current location of center
                FrameworkElement fe = e.Source as FrameworkElement;
                Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
                center = m.Transform(center);  // 转换为矩阵缩放和旋转的中心点

                // Update matrix to reflect translation/rotation
                m.Translate(trans.X, trans.Y);  // 移动
                m.ScaleAt(scale.X, scale.Y, center.X, center.Y);  // 缩放

                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    stroke.Transform(m, false);

                    try
                    {
                        stroke.DrawingAttributes.Width *= md.Scale.X;
                        stroke.DrawingAttributes.Height *= md.Scale.Y;
                    }
                    catch { }
                }
            }
        }

        InkCanvasEditingMode lastEditingMode = new InkCanvasEditingMode();
        private void inkCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            dec.Add(e.TouchDevice.Id);
            //设备1个的时候，记录中心点
            if (dec.Count == 1)
            {
                TouchPoint touchPoint = e.GetTouchPoint(inkCanvas);
                centerPoint = touchPoint.Position;
            }
            //设备两个及两个以上，将画笔功能关闭
            if (dec.Count > 1)
            {
                if (inkCanvas.EditingMode != InkCanvasEditingMode.None)
                {
                    lastEditingMode = inkCanvas.EditingMode;
                }
                if (inkCanvas.EditingMode != InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                }
            }
        }

        private void inkCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            //手势完成后切回之前的状态
            if (dec.Count > 1)
            {
                if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = lastEditingMode;
                    SaveStrokes();
                }
            }
            dec.Remove(e.TouchDevice.Id);
        }
        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            SaveStrokes();
        }

        private void inkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            SaveStrokes();
        }
        #endregion

        #region Curriculum
        public static Curriculums Curriculums = new Curriculums();
        public static string curriculumsFileName = "Curriculums.json";

        private void SaveCurriculum()
        {
            Curriculums.Monday.Curriculums = textBoxMonday.Text;
            Curriculums.Tuesday.Curriculums = textBoxTuesday.Text;
            Curriculums.Wednesday.Curriculums = textBoxWednesday.Text;
            Curriculums.Thursday.Curriculums = textBoxThursday.Text;
            Curriculums.Friday.Curriculums = textBoxFriday.Text;
            Curriculums.Saturday.Curriculums = textBoxSaturday.Text;
            Curriculums.Sunday.Curriculums = textBoxSunday.Text;

            string text = JsonConvert.SerializeObject(Curriculums, Formatting.Indented);
            try
            {
                File.WriteAllText(curriculumsFileName, text);
            }
            catch { }
        }

        private void LoadCurriculum()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + curriculumsFileName))
            {
                try
                {
                    string text = File.ReadAllText(curriculumsFileName);
                    Curriculums = JsonConvert.DeserializeObject<Curriculums>(text);
                }
                catch { }
            }

            textBoxMonday.Text = Curriculums.Monday.Curriculums;
            textBoxTuesday.Text = Curriculums.Tuesday.Curriculums;
            textBoxWednesday.Text = Curriculums.Wednesday.Curriculums;
            textBoxThursday.Text = Curriculums.Thursday.Curriculums;
            textBoxFriday.Text = Curriculums.Friday.Curriculums;
            textBoxSaturday.Text = Curriculums.Saturday.Curriculums;
            textBoxSunday.Text = Curriculums.Sunday.Curriculums;

            string day = DateTime.Today.DayOfWeek.ToString();

            switch (day)
            {
                case "Monday":
                    textBlockCurriculum.Text = Curriculums.Monday.Curriculums;
                    break;
                case "Tuesday":
                    textBlockCurriculum.Text = Curriculums.Tuesday.Curriculums;
                    break;
                case "Wednesday":
                    textBlockCurriculum.Text = Curriculums.Wednesday.Curriculums;
                    break;
                case "Thursday":
                    textBlockCurriculum.Text = Curriculums.Thursday.Curriculums;
                    break;
                case "Friday":
                    textBlockCurriculum.Text = Curriculums.Friday.Curriculums;
                    break;
                case "Saturday":
                    textBlockCurriculum.Text = Curriculums.Saturday.Curriculums;
                    break;
                case "Sunday":
                    textBlockCurriculum.Text = Curriculums.Sunday.Curriculums;
                    break;
            }
        }

        private void editCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            stackPanelCurriculum.Visibility = Visibility.Collapsed;
            editCurriculumButton.Visibility = Visibility.Collapsed;
            scrollViewerLauncher.Visibility = Visibility.Collapsed;

            saveCurriculumButton.Visibility = Visibility.Visible;
            scrollViewerCurriculum.Visibility = Visibility.Visible;
        }

        private void saveCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurriculum();

            stackPanelCurriculum.Visibility = Visibility.Visible;
            editCurriculumButton.Visibility = Visibility.Visible;
            scrollViewerLauncher.Visibility = Visibility.Visible;

            saveCurriculumButton.Visibility = Visibility.Collapsed;
            scrollViewerCurriculum.Visibility = Visibility.Collapsed;

            LoadCurriculum();
        }
        #endregion        

        #region Clock

        private void Clock(object sender, EventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString(("HH:mm:ss"));
        }

        private DispatcherTimer clockTimer;


        #endregion

        #region Launcher
        private void buttonEasiNote5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Seewo\EasiNote5\swenlauncher\swenlauncher.exe");
            }
            catch (Exception)
            { }
        }
        private void buttonEasiCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Seewo\EasiCamera\sweclauncher\sweclauncher.exe");
            }
            catch (Exception)
            { }
        }
        private void buttonExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"C:\Windows\System32\explorer.exe");
            }
            catch (Exception)
            { }
        }


        #endregion
    }
}
