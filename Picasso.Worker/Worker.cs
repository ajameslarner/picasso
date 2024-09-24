using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Picasso;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    private const int MOD_CONTROL = 0x0002;
    private const int WM_HOTKEY = 0x0312;
    private const int PM_REMOVE = 0x0001;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

        RegisterHotKey(handle, 1, MOD_CONTROL, (uint)Keys.T);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                if (msg.message == WM_HOTKEY && msg.wParam.ToInt32() == 1)
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "Picasso.Palette.exe",
                            UseShellExecute = true,
                            CreateNoWindow = false
                        }
                    };

                    process.Start();
                }
            }
        }

        UnregisterHotKey(handle, 1);
    }

    private struct MSG
    {
        public IntPtr hWnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    private struct POINT
    {
        public int x;
        public int y;
    }
}
