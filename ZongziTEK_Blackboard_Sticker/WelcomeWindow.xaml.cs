using iNKORE.UI.WPF.Modern.Media.Animation;
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
using System.Windows.Shapes;
using ZongziTEK_Blackboard_Sticker.Pages.WelcomePages;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// WelcomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
        }

        private int lastPageIndex = 0;

        private List<Type> pages = new()
        {
            typeof(WelcomePage0),
            typeof(WelcomePage1),
            typeof(WelcomePage2),
            typeof(WelcomePage3),
            typeof(WelcomePage4)
        };

        private void NavigationViewRoot_SelectionChanged(iNKORE.UI.WPF.Modern.Controls.NavigationView sender, iNKORE.UI.WPF.Modern.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            int currentPageIndex = NavigationViewRoot.MenuItems.IndexOf(NavigationViewRoot.SelectedItem);

            if (currentPageIndex > lastPageIndex) FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromRight;
            else FrameTransitionEffect.Effect = SlideNavigationTransitionEffect.FromLeft;

            FrameRoot.Navigate(pages[currentPageIndex]);

            lastPageIndex = currentPageIndex;
        }
    }
}
