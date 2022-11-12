using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

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
            drawingAttributes.Width = 3;
            drawingAttributes.Height = 2;
            drawingAttributes.StylusTip = StylusTip.Ellipse;
            drawingAttributes.FitToCurve = true;
            drawingAttributes.IgnorePressure = true;
        }

        private void penButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        private void window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 20;
            Left = SystemParameters.WorkArea.Width - 460;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight - 440;
        }

        private void scrShot_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
