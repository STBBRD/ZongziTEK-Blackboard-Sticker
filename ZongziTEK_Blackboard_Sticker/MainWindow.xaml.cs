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
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using File = System.IO.File;
using IWshRuntimeLibrary;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using ZongziTEK_Blackboard_Sticker.Helpers;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;
using System.Reflection;
using AutoUpdaterDotNET;
using System.Windows.Media.Animation;
using ZongziTEK_Blackboard_Sticker.Pages;
using System.Windows.Interop;

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

            // 小黑板 1
            drawingAttributes = new DrawingAttributes();
            inkCanvas.DefaultDrawingAttributes = drawingAttributes;
            drawingAttributes.Color = Colors.White;
            drawingAttributes.Width = 1.75;
            drawingAttributes.Height = 1.75;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.FitToCurve = true;
            squarePicker.SelectedColor = inkCanvas.DefaultDrawingAttributes.Color;

            // 窗体
            SetWindowMaximized();

            /*windowTimer.Tick += windowTimer_Tick; // 强力置底，可能导致界面闪烁，故注释
            windowTimer.Start();*/

            // 加载文件
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

            // 检查更新
            if (Settings.Update.IsUpdateAutomatic) CheckUpdate();

            // 看板
            textBlockTime.Text = DateTime.Now.ToString(("HH:mm:ss"));
            clockTimer = new DispatcherTimer();
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            clockTimer.Start();

            LoadFrameInfoPagesList();
            frameInfoNavigationTimer.Tick += FrameInfoNavigationTimer_Tick;
            frameInfoNavigationTimer.Interval = TimeSpan.FromSeconds(4);
            frameInfoNavigationTimer.Start();

            // 课程表
            timetableTimer = new DispatcherTimer();
            timetableTimer.Tick += CheckTimetable;
            timetableTimer.Interval = new TimeSpan(0, 0, 1);
            timetableTimer.Start();

            TimetableEditor.EditorButtonUseCurriculum_Click += EditorButtonSettingUseCurriculum;

            // 颜色主题
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            SystemEvents_UserPreferenceChanged(null, null);

            // 小黑板 2
            CheckIsBlackboardLocked();

            // 显示版本号
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            TextBlockVersion.Text = version.ToString();

            // 隐藏管家助手
            timerHideSeewoServiceAssistant.Tick += TimerHideSeewoServiceAssistant_Tick;
            timerHideSeewoServiceAssistant.Interval = TimeSpan.FromSeconds(1);
        }
        #region Window
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsHelper.SetWindowStyleToolWindow(hwnd);
        }

        private void window_StateChanged(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;

            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchLookMode();

            if (Settings.Look.IsAnimationEnhanced)
            {
                DoubleAnimation windowAnimation = new DoubleAnimation()
                {
                    From = window.ActualWidth,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };

                window.BeginAnimation(LeftProperty, windowAnimation);
            }
        }

        private void window_Activated(object sender, EventArgs e)
        {
            if (Settings.Automation.IsBottomMost) WindowsHelper.SetBottom(window);
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
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            //处理 DPI 变化
            if (MessageBox.Show("检测到系统 DPI 变化，为确保黑板贴显示正常，需要重启黑板贴。\r\n是否立即重启黑板贴？", "ZongziTEK 黑板贴", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Restart();
            }
        }

        private void SetWindowMaximized()
        {
            Height = System.Windows.SystemParameters.WorkArea.Height;
            Width = System.Windows.SystemParameters.WorkArea.Width;
            Top = 0;
            Left = 0;
        }

        private void SetWindowScaleTransform(double Multiplier)
        {
            windowScale.ScaleX = Multiplier;
            windowScale.ScaleY = Multiplier;
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

        /*private DispatcherTimer windowTimer = new DispatcherTimer() // 强力置底，可能导致界面闪烁，故注释
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500)
        };

        private void windowTimer_Tick(object sender, EventArgs e)
        {
            WindowsHelper.SetBottom(window);
        }*/
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

                StackPanelHighlightBlackboardLockState.Visibility = Visibility.Visible;
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
                StackPanelHighlightBlackboardLockState.Visibility = Visibility.Collapsed;

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

        public static int timetableToShow_index = (int)DateTime.Today.DayOfWeek;

        private void LoadTimetableorCurriculum()
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                LoadTimetable();
                textBlockCurriculum.Visibility = Visibility.Collapsed;
                StackPanelShowTimetable.Visibility = Visibility.Visible;
            }
            else
            {
                LoadCurriculum();
                textBlockCurriculum.Visibility = Visibility.Visible;
                StackPanelShowTimetable.Visibility = Visibility.Collapsed;
            }
            CheckTimetableMenuItems();
        }

        private void EditorButtonSettingUseCurriculum()
        {
            ToggleSwitchUseTimetable.IsOn = false;
        }

        /*private void ToggleSwitchTempTimetable_Toggled(object sender, RoutedEventArgs e)
        {
            LoadTimetableorCurriculum();
        }*/

        private void MenuItemShowMondayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 1;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowTuesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 2;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowWednesdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 3;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowThursdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 4;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowFridayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 5;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowSaturdayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 6;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowSundayTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 0;
            LoadTimetableorCurriculum();
        }

        private void MenuItemShowTempTimetable_Click(object sender, RoutedEventArgs e)
        {
            timetableToShow_index = 7;
            LoadTimetableorCurriculum();
        }

        private void CheckTimetableMenuItems() // 通过菜单项的 IsChecked 状态来告知用户正在展示哪个课表
        {
            foreach (MenuItem menuItem in MenuChooseTimetableToShow.Items)
            {
                menuItem.IsChecked = false;
            }

            switch (timetableToShow_index)
            {
                case 1: // 周一
                    MenuItemShowMondayTimetable.IsChecked = true;
                    break;
                case 2: // 周二
                    MenuItemShowTuesdayTimetable.IsChecked = true;
                    break;
                case 3: // 周三
                    MenuItemShowWednesdayTimetable.IsChecked = true;
                    break;
                case 4: // 周四
                    MenuItemShowThursdayTimetable.IsChecked = true;
                    break;
                case 5: // 周五
                    MenuItemShowFridayTimetable.IsChecked = true;
                    break;
                case 6: // 周六
                    MenuItemShowSaturdayTimetable.IsChecked = true;
                    break;
                case 0: // 周日
                    MenuItemShowSundayTimetable.IsChecked = true;
                    break;
                case 7: // 临时
                    MenuItemShowTempTimetable.IsChecked = true;
                    break;
            }
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

            textBlockCurriculum.FontSize = Settings.TimetableSettings.FontSize;

            switch (timetableToShow_index)
            {
                case 1: // 周一
                    textBlockCurriculum.Text = Curriculums.Monday.Curriculums;
                    break;
                case 2: // 周二
                    textBlockCurriculum.Text = Curriculums.Tuesday.Curriculums;
                    break;
                case 3: // 周三
                    textBlockCurriculum.Text = Curriculums.Wednesday.Curriculums;
                    break;
                case 4: // 周四
                    textBlockCurriculum.Text = Curriculums.Thursday.Curriculums;
                    break;
                case 5: // 周五
                    textBlockCurriculum.Text = Curriculums.Friday.Curriculums;
                    break;
                case 6: // 周六
                    textBlockCurriculum.Text = Curriculums.Saturday.Curriculums;
                    break;
                case 0: // 周日
                    textBlockCurriculum.Text = Curriculums.Sunday.Curriculums;
                    break;
                case 7: // 临时
                    textBlockCurriculum.Text = Curriculums.Temp.Curriculums;
                    break;
            }
        }

        private bool isTimetableEditorOpen = false;

        private void editCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.TimetableSettings.IsTimetableEnabled)
            {
                if (!isTimetableEditorOpen)
                {
                    TimetableEditor timetableEditor = new();
                    timetableEditor.Closed += TimetableEditor_Closed;
                    isTimetableEditorOpen = true;
                    timetableEditor.Show();
                }
            }
            else
            {
                ScrollViewerShowCurriculum.Visibility = Visibility.Collapsed;
                editCurriculumButton.Visibility = Visibility.Collapsed;

                saveCurriculumButton.Visibility = Visibility.Visible;
                ScrollViewerCurriculum.Visibility = Visibility.Visible;
            }
        }

        private void TimetableEditor_Closed(object sender, EventArgs e)
        {
            LoadTimetableorCurriculum();
            isTimetableEditorOpen = false;
        }

        private void saveCurriculumButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurriculum();

            ScrollViewerShowCurriculum.Visibility = Visibility.Visible;
            editCurriculumButton.Visibility = Visibility.Visible;

            saveCurriculumButton.Visibility = Visibility.Collapsed;
            ScrollViewerCurriculum.Visibility = Visibility.Collapsed;

            LoadTimetableorCurriculum();
        }
        #endregion

        #region Timetable
        public static Timetable Timetable = new Timetable();
        public static string timetableFileName = "Timetable.json";
        private DispatcherTimer timetableTimer;
        List<Lesson> timetableToShow = new List<Lesson>();

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

            switch (timetableToShow_index)
            {
                case 1: // 周一
                    timetableToShow = Timetable.Monday;
                    break;
                case 2: // 周二
                    timetableToShow = Timetable.Tuesday;
                    break;
                case 3: // 周三
                    timetableToShow = Timetable.Wednesday;
                    break;
                case 4: // 周四
                    timetableToShow = Timetable.Thursday;
                    break;
                case 5: // 周五
                    timetableToShow = Timetable.Friday;
                    break;
                case 6: // 周六
                    timetableToShow = Timetable.Saturday;
                    break;
                case 0: // 周日
                    timetableToShow = Timetable.Sunday;
                    break;
                case 7: // 临时
                    timetableToShow = Timetable.Temp;
                    break;
            }

            StackPanelShowTimetable.Children.Clear();
            if (timetableToShow.Count == 0)
            {
                TextBlock textBlock = new TextBlock()
                {
                    FontSize = Settings.TimetableSettings.FontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = (SolidColorBrush)FindResource("ForegroundColor"),
                    Text = "无课程"
                };

                StackPanelShowTimetable.Children.Add(textBlock);
            }
            else
            {
                foreach (Lesson lesson in timetableToShow)
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        FontSize = Settings.TimetableSettings.FontSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = (SolidColorBrush)FindResource("ForegroundColor"),
                        Text = lesson.Subject
                    };
                    if (lesson.IsSplitBelow)
                    {
                        textBlock.Margin = new Thickness(0, 0, 0, 8);
                    }

                    StackPanelShowTimetable.Children.Add(textBlock);
                }
            }

            lessonIndex = -1;
        }

        private int lessonIndex = -1; // 第几节课
        private bool isInClass = false; // 是否是上课时段

        private void CheckTimetable(object sender, EventArgs e)
        {
            timetableTimer.Stop();

            TimeSpan currentTime = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);

            if (timetableToShow != null && timetableToShow.Count != 0)
            {
                // 获取上课状态 lessonIndex 和 isInClass
                foreach (var lesson in timetableToShow)
                {
                    if (currentTime >= lesson.StartTime) // 在这节课开始后
                    {
                        if (timetableToShow.IndexOf(lesson) + 1 < timetableToShow.Count) // 不是最后一节课
                        {
                            if (currentTime < timetableToShow[timetableToShow.IndexOf(lesson) + 1].StartTime) // 在下一节课上课前
                            {
                                lessonIndex = timetableToShow.IndexOf(lesson);
                                isInClass = currentTime < lesson.EndTime;
                                break;
                            }
                        }
                        else // 是最后一节课
                        {
                            lessonIndex = timetableToShow.Count - 1;
                            isInClass = currentTime < lesson.EndTime;
                            break;
                        }
                    }
                    else if (timetableToShow.IndexOf(lesson) == 0) // 在第一节课开始前
                    {
                        lessonIndex = -1;
                        isInClass = false;
                        break;
                    }
                }

                // 弹出上下课提醒
                if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
                {
                    if (lessonIndex != -1 && currentTime == timetableToShow[lessonIndex].EndTime) // 下课时
                    {
                        if (lessonIndex + 1 < timetableToShow.Count) // 不是最后一节课
                        {
                            ShowClassOverNotification(timetableToShow, lessonIndex);
                        }
                        else ShowLastClassOverNotification(timetableToShow[lessonIndex].IsStrongClassOverNotificationEnabled);
                    }
                    if (lessonIndex + 1 < timetableToShow.Count && !isInClass && currentTime == timetableToShow[lessonIndex + 1].StartTime - TimeSpan.FromSeconds(Settings.TimetableSettings.BeginNotificationPreTime)) // 有下一节课，在下一节课开始的数秒前
                    {
                        ShowClassBeginPreNotification(timetableToShow, lessonIndex);
                    }
                }

                // 在界面中高亮当前课程或下一节课
                int lessonToHighlightIndex = -1; // lessonToHighlightIndex 为 -1 时，不高亮任何课程
                SolidColorBrush noHighlightBrush = (SolidColorBrush)FindResource("ForegroundColor");
                SolidColorBrush highlightBrush = (SolidColorBrush)FindResource(ThemeKeys.SystemControlBackgroundAccentBrushKey);

                if (isInClass) // 上课时，高亮当前课程
                {
                    lessonToHighlightIndex = lessonIndex;
                }
                else if (lessonIndex + 1 < timetableToShow.Count) // 在第一节课前或课间，高亮下一节课
                {
                    lessonToHighlightIndex = lessonIndex + 1;
                }
                else // 最后一节课下课后，不高亮任何课程
                {
                    lessonToHighlightIndex = -1;
                }

                foreach (TextBlock textBlock in StackPanelShowTimetable.Children)
                {
                    if (StackPanelShowTimetable.Children.IndexOf(textBlock) == lessonToHighlightIndex) // 高亮要高亮的课程
                    {
                        textBlock.Foreground = highlightBrush;
                    }
                    else // 取消高亮不要高亮的课程
                    {
                        textBlock.Foreground = noHighlightBrush;
                    }
                }
            }
            else if (StackPanelShowTimetable.Children.Count > 0)
            {
                foreach (TextBlock textBlock in StackPanelShowTimetable.Children)
                {
                    textBlock.Foreground = (SolidColorBrush)FindResource("ForegroundColor");
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

                ShowNotificationBNS(title, subtitle, Settings.TimetableSettings.BeginNotificationTime, false);
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

                if (!today[index].IsStrongClassOverNotificationEnabled)
                {
                    ShowNotificationBNS(title, subtitle, Settings.TimetableSettings.OverNotificationTime, false);
                }
                else
                {
                    title = "下一节是 " + today[nextLessonIndex].Subject + "课";
                    new StrongNotificationWindow(title, subtitle).Show();
                }
            }
        }

        private void ShowLastClassOverNotification(bool isStrongNotificationEnabled)
        {
            if (Settings.TimetableSettings.IsTimetableNotificationEnabled)
            {
                if (!isStrongNotificationEnabled)
                {
                    ShowNotificationBNS("课堂结束", "", Settings.TimetableSettings.OverNotificationTime, false);
                }
                else
                {
                    new StrongNotificationWindow("课堂结束", "").Show();
                }
            }
        }
        #endregion
        #endregion

        #region Clock
        private void ClockTimer_Tick(object sender, EventArgs e)
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
            catch (Exception) { }
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

        private bool isLauncherEditorOpen = false;

        private void ButtonEditLauncher_Click(object sender, RoutedEventArgs e)
        {
            if (!isLauncherEditorOpen)
            {
                LauncherEditor launcherEditor = new();
                launcherEditor.Closed += LauncherEditor_Closed;
                isLauncherEditorOpen = true;
                launcherEditor.Show();
            }
        }

        private void LauncherEditor_Closed(object sender, EventArgs e)
        {
            ButtonReloadLauncher_Click(null, null);
            isLauncherEditorOpen = false;
        }

        #endregion

        #region Settings

        #region Panel Show & Hide

        private bool isSettingsWindowOpen = false;

        private void iconShowSettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (borderSettingsPanel.Visibility == Visibility.Collapsed) borderSettingsPanel.Visibility = Visibility.Visible;
            else btnHideSettingsPanel_Click(null, null);

            ButtonRefreshBNSStatus_Click(null, null);*/
            if (!isSettingsWindowOpen)
            {
                SettingsWindow settingsWindow = new();
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.Show();
                isSettingsWindowOpen = true;
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            isSettingsWindowOpen = false;
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
        private void ToggleSwitchBottomMost_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Automation.IsBottomMost = ToggleSwitchBottomMost.IsOn;
            SaveSettings();

            if (!ToggleSwitchBottomMost.IsOn)
            {
                if (MessageBox.Show("需要重新启动黑板贴来使此设置生效\n确定要重新启动黑板贴吗", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Restart();
                }
                else
                {
                    ToggleSwitchBottomMost.IsOn = true;
                }
            }
        }

        private void SliderWindowScale_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetWindowScaleTransform(SliderWindowScale.Value);

            if (!isSettingsLoaded) return;

            Settings.Look.WindowScaleMultiplier = SliderWindowScale.Value;
            SaveSettings();
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

        private void SliderOverNotificationTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isSettingsLoaded) return;

            Settings.TimetableSettings.OverNotificationTime = SliderOverNotificationTime.Value;
            SaveSettings();
        }

        /*private void ToggleSwitchLiteMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            SwitchLookMode();
            Settings.Look.UseLiteMode = ToggleSwitchLiteMode.IsOn;
            SaveSettings();
        }

        private void ToggleSwitchLiteModeWithInfoBoard_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Look.IsLiteModeWithInfoBoard = ToggleSwitchLiteModeWithInfoBoard.IsOn;
            SaveSettings();

            SwitchLookMode();
        }*/

        private void ComboBoxLookMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Look.LookMode = ComboBoxLookMode.SelectedIndex;
            SaveSettings();

            SwitchLookMode();
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

        /*private void ToggleSwitchUseDefaultBNSPath_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            //Settings.TimetableSettings.UseDefaultBNSPath = ToggleSwitchUseDefaultBNSPath.IsOn;
            //TextBoxBNSPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackboard Notification Service";
            SaveSettings();
        }

        private void TextBoxBNSPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;
            //Settings.TimetableSettings.BNSPath = TextBoxBNSPath.Text;
            SaveSettings();
        }*/

        private void SliderTimetableFontSize_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.TimetableSettings.FontSize = SliderTimetableFontSize.Value;
            SaveSettings();

            LoadTimetableorCurriculum();
        }

        private void ButtonRefreshBNSStatus_Click(object sender, RoutedEventArgs e)
        {
            if (GetBNSPath() == null)
            {
                TextBlockBNSStatus.Text = "未检测到黑板通知服务，上下课提醒将不会出现";
                ButtonRefreshBNSStatus.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockBNSStatus.Text = "黑板通知服务正常";
                ButtonRefreshBNSStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void SliderBeginNotificationTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isSettingsLoaded) return;

            SliderBeginNotificationPreTime.Minimum = SliderBeginNotificationTime.Value;
            SliderBeginNotificationPreTime.Maximum = SliderBeginNotificationTime.Value + 20;

            Settings.TimetableSettings.BeginNotificationTime = SliderBeginNotificationTime.Value;
            SaveSettings();
        }

        private void SliderBeginNotificationPreTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isSettingsLoaded) return;

            Settings.TimetableSettings.BeginNotificationPreTime = SliderBeginNotificationPreTime.Value;
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

        private void CheckBoxInfoBoardDate_Checked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isDatePageEnabled = true;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardCountdown_Checked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isCountdownPageEnabled = true;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardWeather_Checked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isWeatherPageEnabled = true;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardWeatherForecast_Checked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isWeatherForecastPageEnabled = true;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardDate_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isDatePageEnabled = false;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardCountdown_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isCountdownPageEnabled = false;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardWeather_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isWeatherPageEnabled = false;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void CheckBoxInfoBoardWeatherForecast_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.isWeatherForecastPageEnabled = false;
            LoadFrameInfoPagesList();
            SaveSettings();
        }

        private void TextBoxWeatherCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.InfoBoard.WeatherCity = TextBoxWeatherCity.Text;
            SaveSettings();
        }

        private void ToggleSwitchAutoHideSeewoHugoAssistant_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Automation.IsAutoHideHugoAssistantEnabled = ToggleSwitchAutoHideSeewoHugoAssistant.IsOn;
            SaveSettings();

            if (Settings.Automation.IsAutoHideHugoAssistantEnabled && isSeewoServiceAssistantHided == false)
                timerHideSeewoServiceAssistant.Start();
            else
                timerHideSeewoServiceAssistant.Stop();
        }

        private void ToggleSwitchAutoUpdate_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isSettingsLoaded) return;

            Settings.Update.IsUpdateAutomatic = ToggleSwitchAutoUpdate.IsOn;
            SaveSettings();

            if (Settings.Update.IsUpdateAutomatic)
            {
                CheckUpdate();
            }
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

            ToggleSwitchBottomMost.IsOn = Settings.Automation.IsBottomMost;

            SliderWindowScale.Value = Settings.Look.WindowScaleMultiplier;
            SetWindowScaleTransform(SliderWindowScale.Value);

            //ToggleSwitchLiteMode.IsOn = Settings.Look.UseLiteMode;
            //ToggleSwitchLiteModeWithInfoBoard.IsOn = Settings.Look.IsLiteModeWithInfoBoard;
            ComboBoxLookMode.SelectedIndex = Settings.Look.LookMode;
            ToggleSwitchThemeAuto.IsOn = Settings.Look.IsSwitchThemeAuto;

            TextBoxDataLocation.Text = Settings.Storage.DataPath;

            ToggleSwitchUseTimetable.IsOn = Settings.TimetableSettings.IsTimetableEnabled;
            ToggleSwitchTimetableNotification.IsOn = Settings.TimetableSettings.IsTimetableNotificationEnabled;
            //ToggleSwitchUseDefaultBNSPath.IsOn = Settings.TimetableSettings.UseDefaultBNSPath;
            //TextBoxBNSPath.Text = Settings.TimetableSettings.BNSPath;
            SliderTimetableFontSize.Value = Settings.TimetableSettings.FontSize;
            SliderBeginNotificationTime.Value = Settings.TimetableSettings.BeginNotificationTime;
            SliderBeginNotificationPreTime.Minimum = SliderBeginNotificationTime.Value;
            SliderBeginNotificationPreTime.Maximum = SliderBeginNotificationTime.Value + 20;
            SliderBeginNotificationPreTime.Value = Settings.TimetableSettings.BeginNotificationPreTime;
            SliderOverNotificationTime.Value = Settings.TimetableSettings.OverNotificationTime;

            ToggleButtonLock.IsChecked = Settings.Blackboard.IsLocked;

            CheckBoxInfoBoardDate.IsChecked = Settings.InfoBoard.isDatePageEnabled;
            CheckBoxInfoBoardCountdown.IsChecked = Settings.InfoBoard.isCountdownPageEnabled;
            CheckBoxInfoBoardWeather.IsChecked = Settings.InfoBoard.isWeatherPageEnabled;
            CheckBoxInfoBoardWeatherForecast.IsChecked = Settings.InfoBoard.isWeatherForecastPageEnabled;
            TextBoxWeatherCity.Text = Settings.InfoBoard.WeatherCity;
            TextBoxCountdownName.Text = Settings.InfoBoard.CountdownName;
            DatePickerCountdownDate.SelectedDate = Settings.InfoBoard.CountdownDate;
            SliderCountdownWarnDays.Value = Settings.InfoBoard.CountdownWarnDays;

            ToggleSwitchAutoHideSeewoHugoAssistant.IsOn = Settings.Automation.IsAutoHideHugoAssistantEnabled;
            if (Settings.Automation.IsAutoHideHugoAssistantEnabled) timerHideSeewoServiceAssistant.Start();

            ToggleSwitchAutoUpdate.IsOn = Settings.Update.IsUpdateAutomatic;

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
            bool light = true;
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
                else light = false;
            }
            catch { }
            return light;
        }

        #region AutoHideSeewoServiceAssistant
        private DispatcherTimer timerHideSeewoServiceAssistant = new DispatcherTimer();
        private bool isSeewoServiceAssistantHided = false;
        private void TimerHideSeewoServiceAssistant_Tick(object sender, EventArgs e)
        {
            if (WindowsHelper.MinimizeSeewoServiceAssistant())
            {
                timerHideSeewoServiceAssistant.Stop();
                isSeewoServiceAssistantHided = true;
            }
        }
        #endregion

        #endregion

        #region InfoBoard
        private List<Type> frameInfoPages = new();
        private int frameInfoPageIndex = 0;
        private DispatcherTimer frameInfoNavigationTimer = new DispatcherTimer();

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

            FrameInfo.NavigationService.RemoveBackEntry();

            frameInfoPageIndex++;
            if (frameInfoPageIndex >= frameInfoPages.Count) frameInfoPageIndex = 0;
            FrameInfo.Navigate(frameInfoPages[frameInfoPageIndex]);

            frameInfoNavigationTimer.Start();
        }

        private void LoadFrameInfoPagesList()
        {
            frameInfoPages.Clear();

            if (Settings.InfoBoard.isDatePageEnabled) frameInfoPages.Add(typeof(DatePage));
            if (Settings.InfoBoard.isCountdownPageEnabled) frameInfoPages.Add(typeof(CountdownPage));
            if (Settings.InfoBoard.isWeatherPageEnabled) frameInfoPages.Add(typeof(WeatherPage));
            if (Settings.InfoBoard.isWeatherForecastPageEnabled) frameInfoPages.Add(typeof(WeatherForecastPage));

            if (frameInfoPages.Count == 0)
            {
                CheckBoxInfoBoardDate.IsChecked = true;
                return;
            }
            FrameInfo.Navigate(frameInfoPages[0]);
            if (frameInfoPages.Count == 1)
            {
                frameInfoNavigationTimer.Stop();
            }
            else
            {
                frameInfoNavigationTimer.Start();
            }
        }
        #endregion

        #region Other Functions
        public static void Restart()
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            System.Windows.Application.Current.Shutdown();
        }

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
                    btnWhite_Click(null, null);
                }
                foreach (Stroke stroke in inkCanvas.Strokes)
                {
                    if (stroke.DrawingAttributes.Color == Colors.Black)
                    {
                        stroke.DrawingAttributes.Color = Colors.White;
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
                shortcut.Description = exeName + "_link";
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

        private void ShowNotificationBNS(string title, string subtitle, double time, bool isBottom)
        {
            string timeString = time.ToString();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                /*&if (Settings.TimetableSettings.BNSPath.EndsWith("\\"))
                {
                    startInfo.FileName = Settings.TimetableSettings.BNSPath + "bns.exe";
                }
                else
                {
                    startInfo.FileName = Settings.TimetableSettings.BNSPath + "\\bns.exe";
                }*/
                if (GetBNSPath() != null) startInfo.FileName = GetBNSPath();
                else return;

                startInfo.Arguments = "\"" + title + "\"" + " \"" + subtitle + "\" -t " + timeString;
                if (isBottom) startInfo.Arguments += " -bottom";

                Process.Start(startInfo);
            }
            catch { }
        }

        public static string GetBNSPath()
        {
            string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{92ECD1B1-7ACD-4523-836F-D1F98FB9AF39}_is1";
            string valueName = "InstallLocation";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);
                    if (value != null)
                    {
                        return value.ToString() + "bns.exe";
                    }
                }
            }

            return null;
        }

        private void SwitchLookMode()
        {
            switch (Settings.Look.LookMode)
            {
                case 0: // 默认
                    BorderMain.ClearValue(WidthProperty);
                    BorderMain.ClearValue(HorizontalAlignmentProperty);
                    iconSwitchLeft.Visibility = Visibility.Visible;

                    ColumnCanvas.Width = new GridLength(1, GridUnitType.Star);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);
                    ColumnClock.Width = GridLength.Auto;

                    frameInfoNavigationTimer.Start();
                    break;

                case 1: // 简约（顶部为时钟）
                    BorderMain.Width = ColumnLauncher.ActualWidth;
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchRight_MouseDown(null, null);
                    iconSwitchLeft.Visibility = Visibility.Collapsed;

                    ColumnCanvas.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(1, GridUnitType.Star);

                    frameInfoNavigationTimer.Stop();
                    FrameInfo.Navigate(frameInfoPages[0]);  //切换到日期页面防止继续调用天气 API
                    break;

                case 2: // 简约（顶部为看板）
                    BorderMain.Width = ColumnLauncher.ActualWidth;
                    BorderMain.HorizontalAlignment = HorizontalAlignment.Right;
                    iconSwitchRight_MouseDown(null, null);
                    iconSwitchLeft.Visibility = Visibility.Collapsed;

                    ColumnCanvas.Width = new GridLength(0);
                    ColumnClock.Width = new GridLength(0);
                    ColumnInfoBoard.Width = new GridLength(1, GridUnitType.Star);

                    frameInfoNavigationTimer.Start();
                    break;
            }
        }

        private void CheckUpdate()
        {
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider("AutoUpdater.json");
            switch (Settings.Update.UpdateChannel)
            {
                case 0: // Release 频道
                    AutoUpdater.Start($"http://s.zztek.top:1573/zbsupdate.xml");
                    break;
                case 1: // Preview 频道
                    AutoUpdater.Start($"http://s.zztek.top:1573/zbsupdatepreview.xml");
                    break;
            }
            AutoUpdater.ApplicationExitEvent += () =>
            {
                Environment.Exit(0);
            };
        }
        #endregion
    }
}
