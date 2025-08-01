using System.Runtime.InteropServices;
using System.Text;

class Program
{
    const int GWL_STYLE = -16;
    const int WS_BORDER = 0x00800000;
    const int WS_CAPTION = 0x00C00000;
    const int WS_THICKFRAME = 0x00040000;
    const int WS_MINIMIZE = 0x20000000;
    const int WS_MAXIMIZEBOX = 0x00010000;
    const int WS_SYSMENU = 0x00080000;

    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfo
    {
        public IntPtr Monitor;
        public RECT Bounds;
    }

    public class WindowInfo
    {
        public IntPtr Handle;
        public string Title;
        public uint ProcessId;
    }

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    static void Main()
    {
        while (true)
        {
            Console.OutputEncoding = Encoding.Unicode;

            var windows = GetVisibleWindows();
            Console.WriteLine("Select a window:");
            for (int i = 0; i < windows.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {windows[i].Title} (PID: {windows[i].ProcessId})");
            }

            Console.Write("\nWindow number: ");
            if (!int.TryParse(Console.ReadLine(), out int winIndex) || winIndex < 1 || winIndex > windows.Count)
            {
                Console.WriteLine("Invalid selection.");
                Console.ReadKey();
                Console.Clear();
                continue;
            }

            var selectedWindow = windows[winIndex - 1];

            bool MonitorChosen = false;
            var monitors = GetAllMonitors();
            int monIndex = 0;

            while (!MonitorChosen)
            {
                Console.WriteLine("\nSelect a monitor:");
                for (int i = 0; i < monitors.Count; i++)
                {
                    var m = monitors[i].Bounds;
                    Console.WriteLine($"{i + 1}. Monitor {i + 1}: {m.Right - m.Left}x{m.Bottom - m.Top} at ({m.Left}, {m.Top})");
                }

                Console.Write("\nMonitor number: ");
                if (!int.TryParse(Console.ReadLine(), out monIndex) || monIndex < 1 || monIndex > monitors.Count)
                {
                    Console.WriteLine("Invalid selection.");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }
                else
                {
                    MonitorChosen = true;
                }
            }
            

            var monitor = monitors[monIndex - 1].Bounds;

            int style = GetWindowLong(selectedWindow.Handle, GWL_STYLE);
            style &= ~(WS_BORDER | WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZEBOX | WS_SYSMENU);
            SetWindowLong(selectedWindow.Handle, GWL_STYLE, style);

            int width = monitor.Right - monitor.Left;
            int height = monitor.Bottom - monitor.Top;

            SetWindowPos(selectedWindow.Handle, IntPtr.Zero, monitor.Left, monitor.Top, width, height,
            SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);

            Console.WriteLine("\nWindow moved and set to borderless fullscreen.");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static List<WindowInfo> GetVisibleWindows()
    {
        var result = new List<WindowInfo>();
        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                int length = GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    StringBuilder sb = new(length + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    string title = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        GetWindowThreadProcessId(hWnd, out uint pid);
                        result.Add(new WindowInfo { Handle = hWnd, Title = title, ProcessId = pid });
                    }
                }
            }
            return true;
        }, IntPtr.Zero);
        return result;
    }

    static List<MonitorInfo> GetAllMonitors()
    {
        var monitors = new List<MonitorInfo>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                monitors.Add(new MonitorInfo
                {
                    Monitor = hMonitor,
                    Bounds = lprcMonitor
                });
                return true;
            }, IntPtr.Zero);
        return monitors;
    }
}
