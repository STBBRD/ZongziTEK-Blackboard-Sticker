using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Drawing = System.Drawing;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using File = System.IO.File;
using IWshRuntimeLibrary;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using ZongziTEK_Blackboard_Sticker.Helpers;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using Page = System.Windows.Controls.Page;
using ZongziTEK_Blackboard_Sticker.Pages;
using System.Collections;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DrawingAttributes drawingAttributes;

        bool isSettingsLoaded = false;
        public MainWindow()
        {
            InitializeComponent();

            //小黑板 1
            drawingAttributes = new DrawingAttributes();
            inkCanvas.DefaultDrawingAttributes = drawingAttributes;
            drawingAttributes.Color = Colors.White;
            drawingAttributes.Width = 1.75;
            drawingAttributes.Height = 1.75;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.FitToCurve = true;
            squarePicker.SelectedColor = inkCanvas.DefaultDrawingAttributes.Color;

            //窗体
            Height = System.Windows.SystemParameters.WorkArea.Height;
            Width = System.Windows.SystemParameters.WorkArea.Width;
            Top = 0;
            Left = 0;

            //加载文件
            LoadSettings();
            LoadStrokes();
            LoadTimetableorCurriculum();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadLauncher();
                });
            });

            //看板
            textBlockTime.Text = DateTime.Now.ToString(("HH:mm:ss"));
            clockTimer = new DispatcherTimer();
            clockTimer.Tick += Clock;
            clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            clockTimer.Start();

            FrameInfo.Navigate(frameInfoPages[0]);
            frameInfoNavigationTimer = new DispatcherTimer();
            frameInfoNavigationTimer.Tick += FrameInfoNavigationTimer_Tick;
            frameInfoNavigationTimer.Interval = TimeSpan.FromSeconds(5);
            frameInfoNavigationTimer.Start();

            //课程表
            timetableTimer = new DispatcherTimer();
            timetableTimer.Tick += CheckTimetable;
            timetableTimer.Interval = new TimeSpan(0, 0, 1);
            timetableTimer.Start();

            TimetableEditor.EditorButtonUseCurriculum_Click += EditorButtonSettingUseCurriculum;

            //颜色主题
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            SystemEvents_UserPreferenceChanged(null, null);

            //小黑板 2
            CheckIsBlackboardLocked();
        }
        #region Window
        private void window_StateChanged(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
            WindowsHelper.SetBottom(window);
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchLookMode();
        }

        private void window_Activated(object sender, EventArgs e)
        {
            WindowsHelper.SetBottom(window);
        }

        public static bool CloseIsFromButton = false;
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseIsFromButton)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 ZongziTEK 黑板贴", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("真的狠心关闭 ZongziTEK 黑板贴 吗？", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (MessageBox.Show("是否取消关闭 ZongziTEK 黑板贴 ？", "ZongziTEK 黑板贴", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                        {
                            e.Cancel = false;
                        }
                    }
                }
            }
        }

        private void iconSwitchLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid.SetColumn(BorderMain, 0);
            iconSwitchLeft.Visibility = Visibility.Collapsed;
            iconSwitchRight.Visibility = Visibility.Visible;
        }

        private void iconSwitchRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid.SetColumn(BorderMain, 1);
            iconSwitchRight.Visibility = Visibility.Collapsed;
            iconSwitchLeft.Visibility = Visibility.Visible;
        }
        #endregion

        #region Blackboard
        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
            {
                //if (!confirmingClear)
                //{
                if (borderColorPicker.Visibility == Visibility.Collapsed) borderColorPicker.Visibility = Visibility.Visible;
                else borderColorPicker.Visibility = Visibility.Collapsed;
                //}
            }
            else
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            borderColorPicker.Visibility = Visibility.Collapsed;
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        //bool confirmingClear = false;

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            borderColorPicker.Visibility = Visibility.Collapsed;

            //borderClearConfirm.Visibility = Visibility.Visible;
            //confirmingClear = true;
            //touchGrid.Visibility = Visibility.Collapsed;
        }

        private void btnClearCancel_Click(object sender, RoutedEventArgs e)
        {
            //borderClearConfirm.Visibility = Visibility.Collapsed;
            //touchGrid.Visibility = Visibility.Visible;
            //confirmingClear = false;
        }

        private void btnClearOK_Click(object sender, RoutedEventArgs e)
        {
            //confirmingClear = false;

            inkCanvas.Strokes.Clear();

            Flyout flyout = FlyoutService.GetFlyout(clearButton) as Flyout;
            if (flyout != null) flyout.Hide();

            string path;

            if (Settings.Storage.IsFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.DataPath;
            }

            if (!path.EndsWith("\\") || !path.EndsWith("/"))
            {
                path += "\\";
            }

            if (File.Exists(path + "sticker.icstk"))
            {
                File.Delete(path + "sticker.icstk");
            }
            //borderClearConfirm.Visibility = Visibility.Collapsed;
            //touchGrid.Visibility = Visibility.Visible;
        }

        private void squarePicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            borderShowColor.Background = new SolidColorBrush(squarePicker.SelectedColor);
            inkCanvas.DefaultDrawingAttributes.Color = squarePicker.SelectedColor;
        }
        private void btnWhite_Click(object sender, RoutedEventArgs e)
        {
            squarePicker.SelectedColor = ((SolidColorBrush)btnWhite.Background).Color;
        }

        private void btnBlue_Click(object sender, RoutedEventArgs e)
        {
            squarePicker.SelectedColor = ((SolidColorBrush)btnBlue.Background).Color;
        }

        private void btnYellow_Click(object sender, RoutedEventArgs e)
        {
            squarePicker.SelectedColor = ((SolidColorBrush)btnYellow.Background).Color;
        }

        private void btnRed_Click(object sender, RoutedEventArgs e)
        {
            squarePicker.SelectedColor = ((SolidColorBrush)btnRed.Background).Color;
        }
        private void btnCloseColorPicker_Click(object sender, RoutedEventArgs e)
        {
            borderColorPicker.Visibility = Visibility.Collapsed;
        }

        private void ToggleButtonLock_Click(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Blackboard.IsLocked = ToggleButtonLock.IsChecked.Value;
            CheckIsBlackboardLocked();

            SaveSettings();
        }

        private void CheckIsBlackboardLocked()
        {
            if (Settings.Blackboard.IsLocked)
            {
                //if (confirmingClear)
                //{
                btnClearCancel_Click(null, null);
                //}

                BorderLockBlackboard.Visibility = Visibility.Visible;

                if (Settings.Look.IsLightTheme) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);

                eraserButton.Visibility = Visibility.Collapsed;
                borderColorPicker.Visibility = Visibility.Collapsed;

            }
            else
            {
                BorderLockBlackboard.Visibility = Visibility.Collapsed;

                if (Settings.Look.IsLightTheme) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);

                eraserButton_Click(null, null);
                penButton_Click(null, null);

                eraserButton.Visibility = Visibility.Visible;
            }
        }

        private void BorderLockBlackboard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HighlightLockState();
        }

        private bool isHighlightingLockState = false;
        private async void HighlightLockState()
        {
            if (!isHighlightingLockState)
            {
                isHighlightingLockState = true;

                if (Settings.Look.IsLightTheme)
                {
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);
                    await Task.Delay(200);
                    ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);
                }

                isHighlightingLockState = false;
                CheckIsBlackboardLocked();
            }
        }

        private void SaveStrokes()
        {
            FileStream fileStream = new FileStream(GetDataPath() + "sticker.icstk", FileMode.Create);
            inkCanvas.Strokes.Save(fileStream);
            fileStream.Close();
        }

        private void LoadStrokes()
        {
            if (File.Exists(GetDataPath() + "sticker.icstk"))
            {
                FileStream fileStream = new FileStream(GetDataPath() + "sticker.icstk", FileMode.Open);
                inkCanvas.Strokes = new StrokeCollection(fileStream);
                fileStream.Close();
            }

            if (Settings.Look.IsLightTheme) SetTheme("Light");
            else SetTheme("Dark");
        }

        /*private void inkCanvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(0.95, 0.95, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(1.2, 0);
                    stroke.Transform(matrix, false);
                }
            }
            else
            {
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAt(1/0.95, 1/0.95, Mouse.GetPosition(inkCanvas).X, Mouse.GetPosition(inkCanvas).Y);
                    matrix.Translate(0, 1.2);
                    stroke.Transform(matrix, false);
                }
            }
            SaveStrokes();
        }*/

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
            borderColorPicker.Visibility = Visibility.Collapsed;

            // 检查是否是压感笔书写
            foreach (StylusPoint stylusPoint in e.Stroke.StylusPoints)
            {
                if (stylusPoint.PressureFactor != 0.5 && stylusPoint.PressureFactor != 0)
                {
                    return;
                }
            }

            try
            {
                StylusPointCollection stylusPoints = new StylusPointCollection();
                int n = e.Stroke.StylusPoints.Count - 1;
                double pressure = 0.1;
                int x = 10;
                if (n == 1) return;
                if (n >= x)
                {
                    for (int i = 0; i < n - x; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)0.5;
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                    for (int i = n - x; i <= n; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)((0.5 - pressure) * (n - i) / x + pressure);
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                }
                else
                {
                    for (int i = 0; i <= n; i++)
                    {
                        StylusPoint point = new StylusPoint();

                        point.PressureFactor = (float)(0.4 * (n - i) / n + pressure);
                        point.X = e.Stroke.StylusPoints[i].X;
                        point.Y = e.Stroke.StylusPoints[i].Y;
                        stylusPoints.Add(point);
                    }
                }
                e.Stroke.StylusPoints = stylusPoints;
            }
            catch
            {

            }

            SaveStrokes();
        }

        private void inkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            SaveStrokes();
        }

        #region Brush effect from WXRIW
        // A StylusPlugin that renders ink with a linear gradient brush effect.
        class CustomDynamicRenderer : DynamicRenderer
        {
            [ThreadStatic]
            static private Brush brush = null;

            [ThreadStatic]
            static private Pen pen = null;

            private Point prevPoint;

            protected override void OnStylusDown(RawStylusInput rawStylusInput)
            {
                // Allocate memory to store the previous point to draw from.
                prevPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
                base.OnStylusDown(rawStylusInput);
            }
            //protected override void OnDraw(System.Windows.Media.DrawingContext drawingContext, System.Windows.Input.StylusPointCollection stylusPoints, System.Windows.Media.Geometry geometry, System.Windows.Media.Brush fillBrush)
            //{


            //    ImageSource img = new BitmapImage(new Uri("pack://application:,,,/Resources/maobi.png"));

            //    //前一个点的绘制。
            //    Point prevPoint = new Point(double.NegativeInfinity,
            //                                double.NegativeInfinity);


            //    var w = Global.StrokeWidth + 15;    //输出时笔刷的实际大小


            //    Point pt = new Point(0, 0);
            //    Vector v = new Vector();            //前一个点与当前点的距离
            //    var subtractY = 0d;                 //当前点处前一点的Y偏移
            //    var subtractX = 0d;                 //当前点处前一点的X偏移
            //    var pointWidth = Global.StrokeWidth;
            //    double x = 0, y = 0;
            //    for (int i = 0; i < stylusPoints.Count; i++)
            //    {
            //        pt = (Point)stylusPoints[i];
            //        v = Point.Subtract(prevPoint, pt);

            //        Debug.WriteLine("X " + pt.X + "\t" + pt.Y);

            //        subtractY = (pt.Y - prevPoint.Y) / v.Length;    //设置stylusPoints两个点之间需要填充的XY偏移
            //        subtractX = (pt.X - prevPoint.X) / v.Length;

            //        if (w - v.Length < Global.StrokeWidth)          //控制笔刷大小
            //        {
            //            pointWidth = Global.StrokeWidth;
            //        }
            //        else
            //        {
            //            pointWidth = w - v.Length;                  //在两个点距离越大的时候，笔刷所展示的大小越小
            //        }


            //        for (double j = 0; j < v.Length; j = j + 1d)    //填充stylusPoints两个点之间
            //        {
            //            x = 0; y = 0;

            //            if (prevPoint.X == double.NegativeInfinity || prevPoint.Y == double.NegativeInfinity || double.PositiveInfinity == prevPoint.X || double.PositiveInfinity == prevPoint.Y)
            //            {
            //                y = pt.Y;
            //                x = pt.X;
            //            }
            //            else
            //            {
            //                y = prevPoint.Y + subtractY;
            //                x = prevPoint.X + subtractX;
            //            }

            //            drawingContext.DrawImage(img, new Rect(x - pointWidth / 2, y - pointWidth / 2, pointWidth, pointWidth));    //在当前点画笔刷图片
            //            prevPoint = new Point(x, y);


            //            if (double.IsNegativeInfinity(v.Length) || double.IsPositiveInfinity(v.Length))
            //            { break; }
            //        }
            //    }
            //    stylusPoints = null;
            //}
            protected override void OnDraw(DrawingContext drawingContext,
                                           StylusPointCollection stylusPoints,
                                           Geometry geometry, Brush fillBrush)
            {
                // Create a new Brush, if necessary.
                //brush ??= new LinearGradientBrush(Colors.Red, Colors.Blue, 20d);

                // Create a new Pen, if necessary.
                //pen ??= new Pen(brush, 2d);

                // Draw linear gradient ellipses between 
                // all the StylusPoints that have come in.
                for (int i = 0; i < stylusPoints.Count; i++)
                {
                    Point pt = (Point)stylusPoints[i];
                    Vector v = Point.Subtract(prevPoint, pt);

                    // Only draw if we are at least 4 units away 
                    // from the end of the last ellipse. Otherwise, 
                    // we're just redrawing and wasting cycles.
                    if (v.Length > 4)
                    {
                        // Set the thickness of the stroke based 
                        // on how hard the user pressed.
                        double radius = stylusPoints[i].PressureFactor * 10d;
                        drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                        prevPoint = pt;
                    }
                }
            }
        }
        public class Global
        {
            public static double StrokeWidth = 2.5;
        }
        public class CustomRenderingInkCanvas : InkCanvas
        {
            CustomDynamicRenderer customRenderer = new CustomDynamicRenderer();

            public CustomRenderingInkCanvas() : base()
            {
                // Use the custom dynamic renderer on the
                // custom InkCanvas.
                this.DynamicRenderer = customRenderer;
            }

            protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
            {
                // Remove the original stroke and add a custom stroke.
                this.Strokes.Remove(e.Stroke);
                CustomStroke customStroke = new CustomStroke(e.Stroke.StylusPoints);
                this.Strokes.Add(customStroke);

                // Pass the custom stroke to base class' OnStrokeCollected method.
                InkCanvasStrokeCollectedEventArgs args =
                    new InkCanvasStrokeCollectedEventArgs(customStroke);
                base.OnStrokeCollected(args);
            }
        }// A class for rendering custom strokes
        class CustomStroke : Stroke
        {
            Brush brush;
            Pen pen;

            public CustomStroke(StylusPointCollection stylusPoints)
                : base(stylusPoints)
            {
                // Create the Brush and Pen used for drawing.
                brush = new LinearGradientBrush(Colors.Red, Colors.Blue, 20d);
                pen = new Pen(brush, 2d);
            }
            //protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
            //{


            //            ImageSource img = new BitmapImage(new Uri("pack://application:,,,/Resources/maobi.png"));

            //    //前一个点的绘制。
            //    Point prevPoint = new Point(double.NegativeInfinity,
            //                                double.NegativeInfinity);


            //    var w = Global.StrokeWidth + 15;    //输出时笔刷的实际大小


            //    Point pt = new Point(0, 0);
            //    Vector v = new Vector();            //前一个点与当前点的距离
            //    var subtractY = 0d;                 //当前点处前一点的Y偏移
            //    var subtractX = 0d;                 //当前点处前一点的X偏移
            //    var pointWidth = Global.StrokeWidth;
            //    double x = 0, y = 0;
            //    for (int i = 0; i < stylusPoints.Count; i++)
            //    {
            //        pt = (Point)stylusPoints[i];
            //        v = Point.Subtract(prevPoint, pt);

            //        Debug.WriteLine("X " + pt.X + "\t" + pt.Y);

            //        subtractY = (pt.Y - prevPoint.Y) / v.Length;    //设置stylusPoints两个点之间需要填充的XY偏移
            //        subtractX = (pt.X - prevPoint.X) / v.Length;

            //        if (w - v.Length < Global.StrokeWidth)          //控制笔刷大小
            //        {
            //            pointWidth = Global.StrokeWidth;
            //        }
            //        else
            //        {
            //            pointWidth = w - v.Length;                  //在两个点距离越大的时候，笔刷所展示的大小越小
            //        }


            //        for (double j = 0; j < v.Length; j = j + 1d)    //填充stylusPoints两个点之间
            //        {
            //            x = 0; y = 0;

            //            if (prevPoint.X == double.NegativeInfinity || prevPoint.Y == double.NegativeInfinity || double.PositiveInfinity == prevPoint.X || double.PositiveInfinity == prevPoint.Y)
            //            {
            //                y = pt.Y;
            //                x = pt.X;
            //            }
            //            else
            //            {
            //                y = prevPoint.Y + subtractY;
            //                x = prevPoint.X + subtractX;
            //            }

            //            drawingContext.DrawImage(img, new Rect(x - pointWidth / 2, y - pointWidth / 2, pointWidth, pointWidth));    //在当前点画笔刷图片
            //            prevPoint = new Point(x, y);


            //            if (double.IsNegativeInfinity(v.Length) || double.IsPositiveInfinity(v.Length))
            //            { break; }
            //        }
            //    }
            //    stylusPoints = null;
            //}
            protected override void DrawCore(DrawingContext drawingContext,
                                             DrawingAttributes drawingAttributes)
            {
                // Allocate memory to store the previous point to draw from.
                Point prevPoint = new Point(double.NegativeInfinity,
                                            double.NegativeInfinity);

                // Draw linear gradient ellipses between
                // all the StylusPoints in the Stroke.
                for (int i = 0; i < this.StylusPoints.Count; i++)
                {
                    Point pt = (Point)this.StylusPoints[i];
                    Vector v = Point.Subtract(prevPoint, pt);

                    // Only draw if we are at least 4 units away
                    // from the end of the last ellipse. Otherwise,
                    // we're just redrawing and wasting cycles.
                    if (v.Length > 4)
                    {
                        // Set the thickness of the stroke
                        // based on how hard the user pressed.
                        double radius = this.StylusPoints[i].PressureFactor * 10d;
                        drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                        prevPoint = pt;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Timetable & Curriculum
        private void LoadTimetableorCurriculum()
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                LoadTimetable();
            }
            else
            {
                LoadCurriculum();
            }
        }

        private void EditorButtonSettingUseCurriculum()
        {
            ToggleSwitchUseTimetable.IsOn = false;
        }

        private void ToggleSwitchTempTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            LoadTimetableorCurriculum();
        }

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
            Curriculums.Temp.Curriculums = textBoxTempCurriculums.Text;

            string text = JsonConvert.SerializeObject(Curriculums, Formatting.Indented);

            try
            {
                File.WriteAllText(GetDataPath() + curriculumsFileName, text);
            }
            catch { }
        }

        private void LoadCurriculum()
        {
            if (File.Exists(GetDataPath() + curriculumsFileName))
            {
                try
                {
                    string text = File.ReadAllText(GetDataPath() + curriculumsFileName);
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
            textBoxTempCurriculums.Text = Curriculums.Temp.Curriculums;

            string day = DateTime.Today.DayOfWeek.ToString();

            if (!ToggleSwitchTempTimetable.IsOn)
            {
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
            else
            {
                textBlockCurriculum.Text = Curriculums.Temp.Curriculums;
            }
        }

        private void editCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                new TimetableEditor().ShowDialog();
                LoadTimetableorCurriculum();
            }
            else
            {
                ScrollViewerShowingCurriculum.Visibility = Visibility.Collapsed;
                editCurriculumButton.Visibility = Visibility.Collapsed;

                saveCurriculumButton.Visibility = Visibility.Visible;
                scrollViewerCurriculum.Visibility = Visibility.Visible;
            }
        }

        private void saveCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurriculum();

            ScrollViewerShowingCurriculum.Visibility = Visibility.Visible;
            editCurriculumButton.Visibility = Visibility.Visible;

            saveCurriculumButton.Visibility = Visibility.Collapsed;
            scrollViewerCurriculum.Visibility = Visibility.Collapsed;

            LoadTimetableorCurriculum();
        }
        #endregion

        #region Timetable
        public static Timetable Timetable = new Timetable();
        public static string timetableFileName = "Timetable.json";
        private DispatcherTimer timetableTimer;

        private void LoadTimetable()
        {
            if (File.Exists(GetDataPath() + timetableFileName))
            {
                try
                {
                    string text = File.ReadAllText(GetDataPath() + timetableFileName);
                    Timetable = JsonConvert.DeserializeObject<Timetable>(text);
                }
                catch { }
            }

            string day = DateTime.Today.DayOfWeek.ToString();

            if (!ToggleSwitchTempTimetable.IsOn)
            {
                switch (day)
                {
                    case "Monday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Monday);
                        break;
                    case "Tuesday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Tuesday);
                        break;
                    case "Wednesday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Wednesday);
                        break;
                    case "Thursday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Thursday);
                        break;
                    case "Friday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Friday);
                        break;
                    case "Saturday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Saturday);
                        break;
                    case "Sunday":
                        textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Sunday);
                        break;
                }
            }
            else
            {
                textBlockCurriculum.Text = Timetable.ToCurriculums(Timetable.Temp);
            }

            lessonIndex = -1;
        }
        private int lessonIndex = -1;
        private void CheckTimetable(object sender, EventArgs e)
        {
            timetableTimer.Stop();

            List<Lesson> today = Timetable.Monday;
            string day = DateTime.Today.DayOfWeek.ToString();
            if (!ToggleSwitchTempTimetable.IsOn)
            {
                switch (day)
                {
                    case "Monday":
                        today = Timetable.Monday;
                        break;
                    case "Tuesday":
                        today = Timetable.Tuesday;
                        break;
                    case "Wednesday":
                        today = Timetable.Wednesday;
                        break;
                    case "Thursday":
                        today = Timetable.Thursday;
                        break;
                    case "Friday":
                        today = Timetable.Friday;
                        break;
                    case "Saturday":
                        today = Timetable.Saturday;
                        break;
                    case "Sunday":
                        today = Timetable.Sunday;
                        break;
                }
            }
            else
            {
                today = Timetable.Temp;
            }

            TimeSpan currentTime = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);

            if (today != null)
            {
                for (int i = 0; i < today.Count; i++)
                {
                    Lesson lesson = today[i];
                    if (currentTime >= lesson.StartTime)
                    {
                        lessonIndex = i;
                        if (currentTime == lesson.EndTime)
                        {
                            if (lessonIndex + 1 < today.Count) ShowClassOverNotification(today, lessonIndex);
                            else ShowLastClassOverNotification();
                            break;
                        }
                    }
                    if (lessonIndex + 1 < today.Count)
                    {
                        if (currentTime == today[lessonIndex + 1].StartTime - TimeSpan.FromSeconds(10))
                        {
                            if (lessonIndex + 1 < today.Count) ShowClassBeginPreNotification(today, lessonIndex);
                        }
                    }
                    if (currentTime < lesson.EndTime) break;
                }
            }

            timetableTimer.Start();
        }

        private void ShowClassBeginPreNotification(List<Lesson> today, int index)
        {
            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                int nextLessonIndex = index + 1;

                string startTimeString = today[nextLessonIndex].StartTime.ToString(@"hh\:mm");
                string endTimeString = today[nextLessonIndex].EndTime.ToString(@"hh\:mm");

                string title = today[nextLessonIndex].Subject + "课 即将开始";
                string subtitle = "此课程将从 " + startTimeString + " 开始，到 " + endTimeString + " 结束";

                ShowNotificationBNS(title, subtitle, 3, false);
            }
        }

        private void ShowClassOverNotification(List<Lesson> today, int index)
        {
            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                int nextLessonIndex = index + 1;

                string startTimeString = today[nextLessonIndex].StartTime.ToString(@"hh\:mm");

                string title = "下一节 " + today[nextLessonIndex].Subject + "课";
                string subtitle = "课堂结束，下一节课将于 " + startTimeString + " 开始";

                ShowNotificationBNS(title, subtitle, 3, false);
            }
        }

        private void ShowLastClassOverNotification()
        {
            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                ShowNotificationBNS("课堂结束", "这是最后一节课", 3, false);
            }
        }
        #endregion
        #endregion

        #region Clock
        private void Clock(object sender, EventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private DispatcherTimer clockTimer;

        private void iconShowBigClock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new FullScreenClock().Show();
        }
        #endregion

        #region Launcher
        private async void LoadLauncher()
        {
            ScrollViewerLauncher.Visibility = Visibility.Collapsed;
            ProgressBarLauncher.Visibility = Visibility.Visible;

            Dictionary<string, Drawing.Bitmap> fileInfo = new();
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
            if (!new DirectoryInfo(LinkPath).Exists)
            {
                try { new DirectoryInfo(LinkPath).Create(); }
                catch { }
            }
            try
            {
                string[] files = Directory.GetFiles(LinkPath);
                foreach (string filePath in files)
                {
                    if (!filePath.EndsWith(".lnk"))
                    {
                        continue;
                    }
                    WshShell shell = new();
                    IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(filePath);
                    /*if (!File.Exists(wshShortcut.TargetPath))
                    {
                        continue;
                    }*/
                    try
                    {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                        Drawing.Bitmap icon = Drawing.Icon.ExtractAssociatedIcon(wshShortcut.TargetPath).ToBitmap();
#pragma warning restore CS8602 // 解引用可能出现空引用。
                        //fileInfo.Add(wshShortcut.TargetPath, icon);
                        fileInfo.Add(filePath, icon);
                    }
                    catch { }
                }

                foreach (KeyValuePair<string, Drawing.Bitmap> file in fileInfo)
                {
                    //启动台里面的按钮
                    Button LinkButton = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 36,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                        BorderThickness = new Thickness(0)
                    };

                    //按钮里面的布局
                    SimpleStackPanel ContentStackPanel = new()
                    {
                        Spacing = 8,
                        Orientation = Orientation.Horizontal
                    };

                    //图标
                    Image image = new()
                    {
                        Height = 19,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.
                        CreateBitmapSourceFromHBitmap(file.Value.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    image.Source = bitmapSource;

                    //名字
                    TextBlock textBlockFileName = new()
                    {
                        Text = Path.GetFileName(file.Key),
                        Visibility = Visibility.Collapsed
                    };

                    TextBlock textBlockLinkName = new()
                    {
                        Text = Path.GetFileName(file.Key).Remove(Path.GetFileName(file.Key).LastIndexOf("."), 4),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    //开始组装按钮
                    ContentStackPanel.Children.Add(image);
                    ContentStackPanel.Children.Add(textBlockFileName);
                    ContentStackPanel.Children.Add(textBlockLinkName);
                    LinkButton.Content = ContentStackPanel;
                    LinkButton.Click += LinkButton_Click;

                    //往启动台里面添加按钮
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            StackPanelLauncher.Children.Add(LinkButton);
                        }));
                    });
                }

                ScrollViewerLauncher.Visibility = Visibility.Visible;
                ProgressBarLauncher.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("加载启动台时出现错误：\r\n" + e.Message);
            }
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
            string filePath = LinkPath + ((TextBlock)((SimpleStackPanel)((Button)sender).Content).Children[2]).Text + ".lnk";
            WshShell shell = new();
            IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(filePath);
            Process.Start("explorer.exe", wshShortcut.TargetPath);
        }
        private void buttonExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(@"C:\Windows\System32\explorer.exe", "/s,");
            }
            catch (Exception)
            { }
        }

        #endregion

        #region Settings

        #region Panel Show & Hide


        private void iconShowSettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (borderSettingsPanel.Visibility == Visibility.Collapsed) borderSettingsPanel.Visibility = Visibility.Visible;
            else btnHideSettingsPanel_Click(null, null);
        }
        private void btnHideSettingsPanel_Click(object sender, RoutedEventArgs e)
        {
            borderSettingsPanel.Visibility = Visibility.Collapsed;
        }
        private void btnCloseFirstOpening_Click(object sender, RoutedEventArgs e)
        {
            borderFirstOpening.Visibility = Visibility.Collapsed;
            SaveSettings();
        }
        #endregion

        #region Settings Panel

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            System.Windows.Application.Current.Shutdown();
        }
        private void ButtonReloadLauncher_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerLauncher.Visibility = Visibility.Collapsed;
            ProgressBarLauncher.Visibility = Visibility.Visible;

            Button buttonExplorerBackup = buttonExplorer;

            StackPanelLauncher.Children.Clear();

            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    StackPanelLauncher.Children.Add(buttonExplorerBackup);
                }));
            });

            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadLauncher();
                });
            });
            btnHideSettingsPanel_Click(null, null);
        }

        private void ButtonEditLauncher_Click(object sender, RoutedEventArgs e)
        {
            new LauncherEditor().ShowDialog();
            ButtonReloadLauncher_Click(null, null);
        }

        private void ToggleSwitchRunAtStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            if (ToggleSwitchRunAtStartup.IsOn)
            {
                StartAutomaticallyCreate("ZongziTEK_Blackboard_Sticker");
            }
            else
            {
                StartAutomaticallyDel("ZongziTEK_Blackboard_Sticker");
            }
        }
        private void ToggleSwitchTheme_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            if (ToggleSwitchTheme.IsOn)
            {
                //亮
                SetTheme("Light");
                SaveSettings();
            }
            else
            {
                //暗
                SetTheme("Dark");
                SaveSettings();
            }
        }
        private void ToggleSwitchThemeAuto_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.Look.IsSwitchThemeAuto = ToggleSwitchThemeAuto.IsOn;
            SystemEvents_UserPreferenceChanged(null, null);
            SaveSettings();
        }

        private void ToggleSwitchDataLocation_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            if (ToggleSwitchDataLocation.IsOn)
            {
                Settings.Storage.IsFilesSavingWithProgram = true;
                GridDataLocation.Visibility = Visibility.Collapsed;
                SaveSettings();
            }
            else
            {
                Settings.Storage.IsFilesSavingWithProgram = false;
                GridDataLocation.Visibility = Visibility.Visible;
                SaveSettings();
            }
        }
        private void TextBoxDataLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.Storage.DataPath = TextBoxDataLocation.Text;
            SaveSettings();
        }
        private void ButtonDataLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            TextBoxDataLocation.Text = folderBrowser.SelectedPath;
        }
        private void ToggleSwitchUseTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.TimetableSettings.IsTimetableEnabled = ToggleSwitchUseTimetable.IsOn;
            LoadTimetableorCurriculum();
            SaveSettings();
        }

        private void ToggleSwitchTimetableNotification_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.TimetableSettings.IsTimetableNotificationEnabled = ToggleSwitchTimetableNotification.IsOn;
            SaveSettings();
        }

        private void ToggleSwitchUseDefaultBNSPath_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.TimetableSettings.UseDefaultBNSPath = ToggleSwitchUseDefaultBNSPath.IsOn;
            TextBoxBNSPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackboard Notification Service";
            SaveSettings();
        }

        private void TextBoxBNSPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.TimetableSettings.BNSPath = TextBoxBNSPath.Text;
            SaveSettings();
        }

        private void ToggleSwitchLiteMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            SwitchLookMode();
            Settings.Look.UseLiteMode = ToggleSwitchLiteMode.IsOn;
            SaveSettings();
        }

        private void TextBoxCountdownName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            Settings.InfoBoard.CountdownName = TextBoxCountdownName.Text;
            SaveSettings();
        }

        private void DatePickerCountdownDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            if (DatePickerCountdownDate.SelectedDate != null) Settings.InfoBoard.CountdownDate = DatePickerCountdownDate.SelectedDate.Value;
            SaveSettings();
        }

        private void SliderCountdownWarnDays_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isSettingsLoaded) return;
            Settings.InfoBoard.CountdownWarnDays = (int)SliderCountdownWarnDays.Value;
            SaveSettings();
        }
        #endregion

        private void LoadSettings()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + settingsFileName))
            {
                try
                {
                    string text = File.ReadAllText(settingsFileName);
                    Settings = JsonConvert.DeserializeObject<Settings>(text);
                }
                catch { }
            }
            else
            {
                borderFirstOpening.Visibility = Visibility.Visible;
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\ZongziTEK_Blackboard_Sticker" + ".lnk"))
            {
                ToggleSwitchRunAtStartup.IsOn = true;
            }

            if (Settings.Storage.IsFilesSavingWithProgram)
            {
                ToggleSwitchDataLocation.IsOn = true;
                GridDataLocation.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToggleSwitchDataLocation.IsOn = false;
                GridDataLocation.Visibility = Visibility.Visible;
            }

            if (Settings.Look.IsLightTheme)
            {
                ToggleSwitchTheme.IsOn = true;
                SetTheme("Light");
            }
            else
            {
                ToggleSwitchTheme.IsOn = false;
                SetTheme("Dark");
            }

            ToggleSwitchLiteMode.IsOn = Settings.Look.UseLiteMode;
            ToggleSwitchThemeAuto.IsOn = Settings.Look.IsSwitchThemeAuto;

            TextBoxDataLocation.Text = Settings.Storage.DataPath;

            ToggleSwitchUseTimetable.IsOn = Settings.TimetableSettings.IsTimetableEnabled;
            ToggleSwitchTimetableNotification.IsOn = Settings.TimetableSettings.IsTimetableNotificationEnabled;
            ToggleSwitchUseDefaultBNSPath.IsOn = Settings.TimetableSettings.UseDefaultBNSPath;
            TextBoxBNSPath.Text = Settings.TimetableSettings.BNSPath;

            ToggleButtonLock.IsChecked = Settings.Blackboard.IsLocked;

            TextBoxCountdownName.Text = Settings.InfoBoard.CountdownName;
            DatePickerCountdownDate.SelectedDate = Settings.InfoBoard.CountdownDate;
            SliderCountdownWarnDays.Value = Settings.InfoBoard.CountdownWarnDays;

            isSettingsLoaded = true;
        }
        public static Settings Settings = new Settings();
        public static string settingsFileName = "Settings.json";
        public static void SaveSettings()
        {
            string text = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            try
            {
                File.WriteAllText(settingsFileName, text);
            }
            catch { }
        }
        #endregion

        #region Utility
        #region FileDrag

        private bool isCopying = false;
        private void window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            TextBlockDragHint.Text = "松手以将文件添加到桌面";
            ProgressBarDragEnter.Visibility = Visibility.Collapsed;
            BorderDragEnter.Visibility = Visibility.Visible;
        }

        private void window_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            if (!isCopying) BorderDragEnter.Visibility = Visibility.Collapsed;
        }

        private async void window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            isCopying = true;

            ProgressBarDragEnter.Visibility = Visibility.Visible;
            TextBlockDragHint.Text = "正在添加文件到桌面，请稍等";

            BorderDragEnter.Visibility = Visibility.Collapsed;

            string folderFileName;
            try { folderFileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString(); }
            catch
            {
                MessageBox.Show("请将此文件拖到桌面空白处", "", MessageBoxButton.OK, MessageBoxImage.Warning);

                BorderDragEnter.Visibility = Visibility.Collapsed;
                ProgressBarDragEnter.Visibility = Visibility.Collapsed;

                return;
            }

            string dest = Path.GetFileName(folderFileName);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";

            await Task.Run(() =>
            {
                if (folderFileName != desktop + dest)
                {
                    if (new DirectoryInfo(folderFileName).Exists)
                    {
                        if (new DirectoryInfo(desktop + dest).Exists) new DirectoryInfo(desktop + dest).Delete(true);
                        try
                        {
                            FileUtility.CopyFolder(folderFileName, desktop + dest);
                        }
                        catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                    }
                    else
                    {
                        if (File.Exists(desktop + dest)) File.Delete(desktop + dest);
                        try
                        {
                            File.Copy(folderFileName, desktop + dest);
                        }
                        catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                    }
                }
            });

            BorderDragEnter.Visibility = Visibility.Collapsed;
            ProgressBarDragEnter.Visibility = Visibility.Collapsed;

            isCopying = false;
        }
        #endregion
        #endregion

        #region Check
        private void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (Settings.Look.IsSwitchThemeAuto)
            {
                ToggleSwitchTheme.IsOn = IsSystemThemeLight();
            }
        }

        private bool IsSystemThemeLight()
        {
            bool light = false;
            try
            {
                RegistryKey registryKey = Registry.CurrentUser;
                RegistryKey themeKey = registryKey.OpenSubKey("software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                int keyValue = 0;
                if (themeKey != null)
                {
                    keyValue = (int)themeKey.GetValue("SystemUsesLightTheme");
                }
                if (keyValue == 1) light = true;
            }
            catch { }
            return light;
        }
        #endregion

        #region Other Functions

        public static string GetDataPath()
        {
            string path;

            if (Settings.Storage.IsFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.DataPath;
                if (!path.EndsWith("\\") || !path.EndsWith("/"))
                {
                    path += "\\";
                }

                if (!new DirectoryInfo(path).Exists)
                {
                    try { new DirectoryInfo(path).Create(); }
                    catch { }
                }
            }
            return path;
        }

        private void SetTheme(string theme)
        {
            if (theme == "Light")
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Light);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Light.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

                if (ToggleButtonLock.IsChecked.Value) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black);

                Settings.Look.IsLightTheme = true;

                if (inkCanvas.DefaultDrawingAttributes.Color == Colors.White)
                {
                    btnWhite_Click(null, null);
                }

                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    if (stroke.DrawingAttributes.Color == Colors.White)
                    {
                        stroke.DrawingAttributes.Color = Colors.Black;
                    }
                }
            }
            else if (theme == "Dark")
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Dark);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Dark.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);

                if (ToggleButtonLock.IsChecked.Value) ToggleButtonLock.Foreground = new SolidColorBrush(Colors.Black); else ToggleButtonLock.Foreground = new SolidColorBrush(Colors.White);

                Settings.Look.IsLightTheme = false;

                if (inkCanvas.DefaultDrawingAttributes.Color == Colors.Black)
                {
                    inkCanvas.DefaultDrawingAttributes.Color = Colors.White;
                }
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    if (stroke.DrawingAttributes.Color == Colors.Black)
                    {
                        btnWhite_Click(null, null);
                    }
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/STBBRD/ZongziTEK-Blackboard-Sticker");
        }

        public static bool StartAutomaticallyCreate(string exeName)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath;
                shortcut.WorkingDirectory = System.Environment.CurrentDirectory;
                shortcut.WindowStyle = 1;
                shortcut.Description = exeName + "_Ink";
                shortcut.Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool StartAutomaticallyDel(string exeName)
        {
            try
            {
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                return true;
            }
            catch (Exception) { }
            return false;
        }

        private void ShowNotificationBNS(string title, string subtitle, int time, bool isBottom)
        {
            string timeString = time.ToString();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                if (Settings.TimetableSettings.BNSPath.EndsWith("\\"))
                {
                    startInfo.FileName = Settings.TimetableSettings.BNSPath + "bns.exe";
                }
                else
                {
                    startInfo.FileName = Settings.TimetableSettings.BNSPath + "\\bns.exe";
                }

                startInfo.Arguments = "\"" + title + "\"" + " \"" + subtitle + "\" -t " + timeString;
                if (isBottom) startInfo.Arguments += " -bottom";

                Process.Start(startInfo);
            }
            catch { }
        }

        private void SwitchLookMode()
        {
            if (ToggleSwitchLiteMode.IsOn)
            {
                BorderMain.Width = ColumnLauncher.ActualWidth;
                BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                iconSwitchRight_MouseDown(null, null);
                iconSwitchLeft.Visibility = Visibility.Collapsed;

                ColumnCanvas.Width = new GridLength(0);
                ColumnInfoBoard.Width = new GridLength(0);
                ColumnClock.Width = new GridLength(1, GridUnitType.Star);

                frameInfoNavigationTimer.Stop();
            }
            else
            {
                BorderMain.ClearValue(WidthProperty);
                BorderMain.ClearValue(HorizontalAlignmentProperty);
                iconSwitchLeft.Visibility = Visibility.Visible;

                ColumnCanvas.Width = new GridLength(1, GridUnitType.Star);
                ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);
                ColumnClock.Width = GridLength.Auto;

                frameInfoNavigationTimer.Start();
            }
        }
        #endregion

        #region InfoBoard
        private List<Uri> frameInfoPages = new List<Uri>
        {
            new Uri("Pages/DatePage.xaml",UriKind.Relative),
            new Uri("Pages/WeatherPage.xaml", UriKind.Relative),
            new Uri("Pages/CountdownPage.xaml", UriKind.Relative)
        };
        private int frameInfoPageIndex = 0;
        private DispatcherTimer frameInfoNavigationTimer;
        private void BorderSwitchFrameInfoPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchFrameInfoPage();
        }

        private void FrameInfoNavigationTimer_Tick(object sender, EventArgs e)
        {
            SwitchFrameInfoPage();
        }

        private void SwitchFrameInfoPage()
        {
            frameInfoNavigationTimer.Stop();

            frameInfoPageIndex++;
            if (frameInfoPageIndex >= frameInfoPages.Count) frameInfoPageIndex = 0;
            FrameInfo.Navigate(frameInfoPages[frameInfoPageIndex]);

            frameInfoNavigationTimer.Start();
        }
        #endregion
    }
}
