using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
// using System.Windows.Forms;
class Program
{

    public class WindowInfos
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
        public uint ProcessId { get; set; }
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var windows = GetOpenWindows();
        if (windows.Count == 0)
        {
            Console.WriteLine("No visible windows found.");
            return;
        }

        Console.WriteLine("Open Windows:");
        
        foreach (var window in windows)
        {
            Console.WriteLine($"Title: {window.Title}, Handle: {window.Handle}, Process ID: {window.ProcessId}");
        }

        Console.Write("\nEnter the number of the window to move: ");
        if (!int.TryParse(Console.ReadLine(), out int windowIndex) || windowIndex < 1 || windowIndex > windows.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var selectedWindow = windows[windowIndex - 1];

        // var screens = Screen.AllScreens;

        Console.ReadKey();
    }

    public static List<WindowInfos> GetOpenWindows()
    {
        List<WindowInfos> windowList = new();

        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                int length = GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    StringBuilder builder = new(length + 1);
                    GetWindowText(hWnd, builder, builder.Capacity);
                    string title = builder.ToString();
                    if (!string.IsNullOrWhiteSpace(title))
                    {

                        GetWindowThreadProcessId(hWnd, out uint processId);

                        windowList.Add(new WindowInfos
                        {
                            Handle = hWnd,
                            Title = title,
                            ProcessId = processId
                        });
                    }
                }
            }
            return true; 
        }, IntPtr.Zero);

        return windowList;
    }
}