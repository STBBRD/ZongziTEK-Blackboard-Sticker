using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// FullScreenClock.xaml 的交互逻辑
    /// </summary>
    public partial class FullScreenClock : Window
    {
        public FullScreenClock()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(750),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, opacityAnimation);

            ThicknessAnimation clockMarginAnimation = new()
            {
                From = new Thickness(24),
                To = new Thickness(100),
                Duration = TimeSpan.FromMilliseconds(750),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            ViewboxClock.BeginAnimation(MarginProperty, clockMarginAnimation);

            textBlockBigClock.Text = DateTime.Now.ToString(("HH':'mm':'ss"));

            clockTimer = new DispatcherTimer();
            clockTimer.Tick += new EventHandler(Clock);
            clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            clockTimer.Start();
        }
        private void Clock(object sender, EventArgs e)
        {
            textBlockBigClock.Text = DateTime.Now.ToString(("HH':'mm':'ss"));
        }

        private DispatcherTimer clockTimer;
        bool isBorderCloseBigClockShowing = false;
        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isBorderCloseBigClockShowing)
            {
                isBorderCloseBigClockShowing = true;
                BorderCloseBigClock.Opacity = 0;
                BorderCloseBigClock.Margin = new Thickness(150);
                BorderCloseBigClock.Visibility = Visibility.Visible;

                var BorderCloseBigClockThicknessAnimation = new ThicknessAnimation()
                {
                    From = new Thickness(150),
                    To = new Thickness(200),
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };

                var BorderCloseBigClockDoubleAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
                };

                BorderCloseBigClock.BeginAnimation(MarginProperty, BorderCloseBigClockThicknessAnimation);
                BorderCloseBigClock.BeginAnimation(OpacityProperty, BorderCloseBigClockDoubleAnimation);


                await Task.Delay(1500);

                var BorderCloseBigClockExitThicknessAnimation = new ThicknessAnimation()
                {
                    From = new Thickness(200),
                    To = new Thickness(150),
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
                };

                var BorderCloseBigClockExitDoubleAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
                };

                BorderCloseBigClock.BeginAnimation(MarginProperty, BorderCloseBigClockExitThicknessAnimation);
                BorderCloseBigClock.BeginAnimation(OpacityProperty, BorderCloseBigClockExitDoubleAnimation);

                await Task.Delay(250);

                BorderCloseBigClock.Visibility = Visibility.Collapsed;

                isBorderCloseBigClockShowing = false;
            }
        }

        private async void BorderCloseBigClock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation opacityAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            BeginAnimation(OpacityProperty, opacityAnimation);

            ThicknessAnimation clockMarginAnimation = new()
            {
                From = new Thickness(100),
                To = new Thickness(-48),
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn }
            };
            ViewboxClock.BeginAnimation(MarginProperty, clockMarginAnimation);

            await Task.Delay(500);
            Close();
        }
    }
}
