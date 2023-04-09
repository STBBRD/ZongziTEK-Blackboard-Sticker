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
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using File = System.IO.File;
using IWshRuntimeLibrary;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using ModernWpf;
using Windows.UI.Input.Inking;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DrawingAttributes drawingAttributes;

        bool isLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
            drawingAttributes = new DrawingAttributes();
            inkCanvas.DefaultDrawingAttributes = drawingAttributes;
            drawingAttributes.Color = Colors.White;
            drawingAttributes.Width = 1.75;
            drawingAttributes.Height = 1.75;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.FitToCurve = true;
            //drawingAttributes.IgnorePressure = true;

            Height = System.Windows.SystemParameters.WorkArea.Height;
            Width = System.Windows.SystemParameters.WorkArea.Width;
            Top = 0;
            Left = 0;

            textBlockTime.Text = DateTime.Now.ToString(("HH:mm:ss"));
            LoadSettings();
            LoadCurriculum();
            LoadStrokes();
            InitializeLauncher();

            ColumnLauncher.Width = new GridLength(Width * 0.15);
            rowZero.MaxHeight = Height - 114;

            clockTimer = new DispatcherTimer();
            clockTimer.Tick += new EventHandler(Clock);
            clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            clockTimer.Start();

            isLoaded = true;
        }
        #region Window
        private void window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        public static bool CloseIsFromButton = false;
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseIsFromButton)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 ZongziTEK 黑板贴", "ZongziTEK 黑板贴", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
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

            SaveCurriculum();

        }

        private void iconSwitchLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid.SetColumn(MainGrid, 0);
            borderLeftToolBar.Visibility = Visibility.Collapsed;
            borderRightToolBar.Visibility = Visibility.Visible;
        }

        private void iconSwitchRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid.SetColumn(MainGrid, 1);
            borderRightToolBar.Visibility = Visibility.Collapsed;
            borderLeftToolBar.Visibility = Visibility.Visible;
        }
        #endregion

        #region Ink Canvas
        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
            {
                if (!confirmingClear)
                {
                    if (borderColorPicker.Visibility == Visibility.Collapsed) borderColorPicker.Visibility = Visibility.Visible;
                    else borderColorPicker.Visibility = Visibility.Collapsed;
                }
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

        bool confirmingClear = false;

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            borderColorPicker.Visibility = Visibility.Collapsed;
            borderClearConfirm.Visibility = Visibility.Visible;

            confirmingClear = true;

            touchGrid.Visibility = Visibility.Collapsed;
        }
        private void btnClearCancel_Click(object sender, RoutedEventArgs e)
        {
            borderClearConfirm.Visibility = Visibility.Collapsed;
            touchGrid.Visibility = Visibility.Visible;

            confirmingClear = false;
        }

        private void btnClearOK_Click(object sender, RoutedEventArgs e)
        {
            confirmingClear = false;

            inkCanvas.Strokes.Clear();

            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "sticker.icstk"))
            {
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "sticker.icstk");
            }
            borderClearConfirm.Visibility = Visibility.Collapsed;
            touchGrid.Visibility = Visibility.Visible;
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
        private void SaveStrokes()
        {
            string path;

            if (Settings.Storage.isFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.dataPath;
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

            FileStream fileStream = new FileStream(path + "sticker.icstk", FileMode.Create);
            inkCanvas.Strokes.Save(fileStream);
            fileStream.Close();
        }
        private void LoadStrokes()
        {
            string path;

            if (Settings.Storage.isFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.dataPath;
                if (!path.EndsWith("\\") || !path.EndsWith("/"))
                {
                    path += "\\";
                }
            }

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

            string path;

            if (Settings.Storage.isFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.dataPath;
                if (!path.EndsWith("\\") || !path.EndsWith("/"))
                {
                    path += "\\";
                }
            }

            try
            {
                File.WriteAllText(path + curriculumsFileName, text);
            }
            catch { }
        }

        private void LoadCurriculum()
        {
            string path;

            if (Settings.Storage.isFilesSavingWithProgram)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                path = Settings.Storage.dataPath;
                if (!path.EndsWith("\\") || !path.EndsWith("/"))
                {
                    path += "\\";
                }
            }

            if (File.Exists(path + curriculumsFileName))
            {
                try
                {
                    string text = File.ReadAllText(path + curriculumsFileName);
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

        private void iconShowBigClock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new FullScreenClock().Show();
        }
        #endregion

        #region Launcher
        private void InitializeLauncher()
        {
            if (!File.Exists(@"C:\Program Files (x86)\Seewo\EasiNote5\swenlauncher\swenlauncher.exe")) buttonEasiNote5.Visibility = Visibility.Collapsed;
            if (!File.Exists(@"C:\Program Files (x86)\Seewo\EasiCamera\sweclauncher\sweclauncher.exe")) buttonEasiCamera.Visibility = Visibility.Collapsed;
        }
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

        #region Settings

        #region Panel Show & Hide


        private void iconShowSettingsPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            borderSettingsPanel.Visibility = Visibility.Visible;
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
        private void ToggleSwitchRunAtStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
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
            if (!isLoaded) return;
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
        private void ToggleSwitchDataLocation_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            if (ToggleSwitchDataLocation.IsOn)
            {
                Settings.Storage.isFilesSavingWithProgram = true;
                GridDataLocation.Visibility = Visibility.Collapsed;
                SaveSettings();
            }
            else
            {
                Settings.Storage.isFilesSavingWithProgram = false;
                GridDataLocation.Visibility = Visibility.Visible;
                SaveSettings();
            }
        }
        private void TextBoxDataLocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            Settings.Storage.dataPath = TextBoxDataLocation.Text;
            SaveSettings();
        }
        private void ButtonDataLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            TextBoxDataLocation.Text = folderBrowser.SelectedPath;
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

            if (Settings.Storage.isFilesSavingWithProgram)
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

            TextBoxDataLocation.Text = Settings.Storage.dataPath;
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
        private void window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            TextBlockDragHint.Text = "松手以将文件添加到桌面";
            BorderDragEnter.Visibility = Visibility.Visible;
        }

        private void window_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            BorderDragEnter.Visibility = Visibility.Collapsed;
        }

        private void window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            ProgressBarDragEnter.Visibility = Visibility.Visible;
            TextBlockDragHint.Text = "正在添加文件到桌面，请稍等";
            string folderFileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            string dest = Path.GetFileName(folderFileName);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";
            if (folderFileName != desktop + dest)
            {
                if (new DirectoryInfo(folderFileName).Exists)
                {
                    if (new DirectoryInfo(desktop + dest).Exists) new DirectoryInfo(desktop + dest).Delete(true);
                    try { FileUtility.CopyFolder(folderFileName, desktop + dest); } catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                }
                else
                {
                    if (File.Exists(desktop + dest)) File.Delete(desktop + dest);
                    try { File.Copy(folderFileName, desktop + dest); } catch (Exception ex) { MessageBox.Show(Convert.ToString(ex)); }
                }
            }
            BorderDragEnter.Visibility = Visibility.Collapsed;
            ProgressBarDragEnter.Visibility = Visibility.Collapsed;
        }
        #endregion
        #endregion

        #region Other Functions

        private void SetTheme(string theme)
        {
            if (theme == "Light")
            {
                ThemeManager.SetRequestedTheme(window, ElementTheme.Light);
                ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("Style/Light.xaml", UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                Settings.Look.IsLightTheme = true;

                if (inkCanvas.DefaultDrawingAttributes.Color == Colors.White) inkCanvas.DefaultDrawingAttributes.Color = Colors.Black;
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
                Settings.Look.IsLightTheme = false;

                if (inkCanvas.DefaultDrawingAttributes.Color == Colors.Black) inkCanvas.DefaultDrawingAttributes.Color = Colors.White;
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
        #endregion
    }
}
