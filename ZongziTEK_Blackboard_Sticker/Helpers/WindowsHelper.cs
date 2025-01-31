using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public static class WindowsHelper
    {
        #region SetWindowPos
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        #endregion

        #region Hide SeewoServiceAssistant
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int DESKTOP_HORZRES = 118;
        private const int DESKTOP_VERTRES = 117;

        public static Size GetScreenResolution()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int width = GetDeviceCaps(hdc, DESKTOP_HORZRES);
            int height = GetDeviceCaps(hdc, DESKTOP_VERTRES);

            return new Size(width, height);
        }

        public static bool MinimizeSeewoServiceAssistant()
        {
            // 获取屏幕宽度
            double screenWidth = GetScreenResolution().Width;

            // 查找进程
            foreach (var process in Process.GetProcessesByName("SeewoServiceAssistant"))
            {
                IntPtr windowHandle = FindWindow("Chrome_WidgetWin_0", "希沃管家");

                if (windowHandle != IntPtr.Zero)
                {
                    // 获取窗口大小
                    if (GetWindowRect(windowHandle, out RECT rect))
                    {
                        int windowWidth = rect.Right - rect.Left;

                        // 判断窗口宽度是否小于屏幕大小的三分之一
                        if (windowWidth < screenWidth / 3)
                        {
                            // 最小化窗口
                            ShowWindow(windowHandle, SW_MINIMIZE);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region SetWindowStyle
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TRANSPARENT = 0x00000020;

        public static void SetWindowStyleToolWindow(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, -20);
            SetWindowLong(hwnd, -20, extendedStyle | WS_EX_TOOLWINDOW);
        }
        #endregion

        #region SetWindowChrome & SetAllowTransparency
        public static void SetWindowChrome(Window window)
        {
            WindowChrome windowChrome = new()
            {
                GlassFrameThickness = new(-1),
                CaptionHeight = 0
            };
            WindowChrome.SetWindowChrome(window, windowChrome);

            window.AllowsTransparency = false;
        }

        public static void SetAllowTransparency(Window window)
        {
            window.AllowsTransparency = true;
            WindowChrome.SetWindowChrome(window, null);
        }
        #endregion

        #region StyleStruct
        public struct StyleStruct
        {
            public int styleOld;
            public int styleNew;
        }
        #endregion
    }
}
