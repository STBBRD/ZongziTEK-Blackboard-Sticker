using iNKORE.UI.WPF.Modern.Controls;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;
using Drawing = System.Drawing;
using File = System.IO.File;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// LauncherEditor.xaml 的交互逻辑
    /// </summary>
    public partial class LauncherEditor : Window
    {
        public LauncherEditor()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadList();
                });
            });
        }

        private async void LoadList()
        {
            ListScrollViewer.Visibility = Visibility.Collapsed;
            ListProgressBar.Visibility = Visibility.Visible;

            Dictionary<string, Drawing.Bitmap> fileInfo = new();
            string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"\LauncherLinks\";
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
                    //列表项
                    Button LinkButton = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Height = 48,
                        Background = new SolidColorBrush(Color.FromArgb(255, 251, 251, 251))
                    };

                    //按钮里面的布局
                    SimpleStackPanel ContentStackPanel = new()
                    {
                        Spacing = 8,
                        Margin = new(8, 8, 8, 8),
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    Grid ContentGrid = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    //图标
                    Image image = new()
                    {
                        Height = 19
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
                        Text = Path.GetFileName(file.Key).Remove(Path.GetFileName(file.Key).LastIndexOf("."), 4)
                    };

                    //删除按钮
                    Button DeleteButton = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Width = 36,
                        Height = 36,
                        Background = Brushes.Transparent,
                        Padding = new Thickness(0),
                        BorderThickness = new Thickness(0),
                    };

                    SymbolIcon DeleteIcon = new()
                    {
                        Symbol = Symbol.Delete
                    };

                    DeleteButton.Content = DeleteIcon;


                    //开始组装按钮
                    ContentStackPanel.Children.Add(image);
                    ContentStackPanel.Children.Add(textBlockFileName);
                    ContentStackPanel.Children.Add(textBlockLinkName);
                    LinkButton.Content = ContentGrid;
                    ContentGrid.Children.Add(ContentStackPanel);
                    ContentGrid.Children.Add(DeleteButton);
                    DeleteButton.Click += DeleteButton_Click;

                    //往启动台里面添加按钮
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ListStackPanel.Children.Add(LinkButton);
                        }));
                    });
                }

                ListScrollViewer.Visibility = Visibility.Visible;
                ListProgressBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("加载列表时出现错误：\r\n" + e.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认删除吗", "编辑启动台", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string LinkPath = AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\";
                string filePath = LinkPath + ((TextBlock)((SimpleStackPanel)((Grid)((Button)sender).Parent).Children[0]).Children[2]).Text + ".lnk";
                try
                {
                    File.Delete(filePath);
                    ButtonRefresh_Click(null, null);
                }
                catch (Exception ex) { MessageBox.Show("删除该项时出现错误：\r\n" + ex.Message); }
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ListStackPanel.Children.Clear();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    LoadList();
                });
            });
        }

        //New

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            TextBoxFilePath.Text = "";
            TextBoxLinkName.Text = "";
            BorderNew.Visibility = Visibility.Visible;
        }
        private void ButtonCancelNew_Click(object sender, RoutedEventArgs e)
        {
            BorderNew.Visibility = Visibility.Collapsed;
        }

        private void ButtonSaveNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(AppDomain.CurrentDomain.BaseDirectory + @"LauncherLinks\" + TextBoxLinkName.Text + ".lnk");
                shortcut.TargetPath = TextBoxFilePath.Text;
                shortcut.WorkingDirectory = Path.GetFullPath(TextBoxFilePath.Text);
                shortcut.Description = "ZongziTEK";
                shortcut.Save();
                BorderNew.Visibility = Visibility.Collapsed;
                ButtonRefresh_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("新建项时出现错误：\r\n" + ex.Message); }
        }

        private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileBrowser = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "选择文件",
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            fileBrowser.ShowDialog();
            TextBoxFilePath.Text = fileBrowser.FileName;
        }

        private void TextBoxFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBoxLinkName.Text = Path.GetFileNameWithoutExtension(TextBoxFilePath.Text);
            }
            catch (Exception) { }
        }
    }
}
