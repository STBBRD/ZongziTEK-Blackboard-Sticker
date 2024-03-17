using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public static class WindowsHelper
    {
        // Set Sticker Bottom
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        // Hide SeewoServiceAssistant
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
    }
}
