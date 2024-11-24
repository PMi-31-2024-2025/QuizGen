using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinRT;

namespace QuizGen.Presentation.Helpers;

public static class WindowMinSizeHelper
{
    private delegate IntPtr WinProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private const int GWLP_WNDPROC = -4;
    private const int WM_GETMINMAXINFO = 0x0024;

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WinProc newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    // Keep delegates alive for the lifetime of the application
    private static readonly Dictionary<IntPtr, WinProc> _windowProcs = new();

    public static void SetMinSize(Window window, int minWidth, int minHeight)
    {
        var hwnd = window.As<IWindowNative>().WindowHandle;
        IntPtr oldWndProc = IntPtr.Zero;

        var newProc = new WinProc((hWnd, msg, wParam, lParam) =>
        {
            if (msg == WM_GETMINMAXINFO)
            {
                var dpi = GetDpiForWindow(hWnd);
                float scalingFactor = (float)dpi / 96;

                var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                minMaxInfo.ptMinTrackSize.x = (int)(minWidth * scalingFactor);
                minMaxInfo.ptMinTrackSize.y = (int)(minHeight * scalingFactor);
                Marshal.StructureToPtr(minMaxInfo, lParam, true);
            }
            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        });

        oldWndProc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, newProc);

        // Store the delegate in our dictionary
        _windowProcs[hwnd] = newProc;
    }
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
internal interface IWindowNative
{
    IntPtr WindowHandle { get; }
}