using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using WindowsInput;

namespace TraderForStalCraft.Scripts
{
    internal class EmulatorClicks
    {
        private Random random;

        private int delayM;
        private int delayK;

        public EmulatorClicks(decimal delayM = 0, decimal delayK = 0, bool hasRandom = false)
        {
            if (hasRandom)
            {
                random = new Random();
                this.delayK = Convert.ToInt32(delayK);
                this.delayM = Convert.ToInt32(delayM);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        public void MoveMouseSmoothly(int targetX, int targetY, int steps = 20)
        {
            if (random == null)
            {
                Cursor.Position = new Point(targetX, targetY);
                Thread.Sleep(50);

                mouse_event(MOUSEEVENTF_LEFTDOWN, targetX, targetY, 0, IntPtr.Zero);
                Thread.Sleep(300);
                mouse_event(MOUSEEVENTF_LEFTUP, targetX, targetY, 0, IntPtr.Zero);
                Thread.Sleep(50);

                return;
            }
            else
            {
                Point current = Cursor.Position;
                for (int i = 1; i <= steps; i++)
                {
                    double ratio = (double)i / steps;
                    int newX = current.X + (int)((targetX - current.X) * ratio);
                    int newY = current.Y + (int)((targetY - current.Y) * ratio);

                    newX += random.Next(-2, 3);
                    newY += random.Next(-2, 3);

                    Cursor.Position = new Point((int)newX, (int)newY);
                    Thread.Sleep(random.Next(0, delayM));
                }

                mouse_event(MOUSEEVENTF_LEFTDOWN, targetX, targetY, 0, IntPtr.Zero);
                Thread.Sleep(random.Next(150, 170));
                mouse_event(MOUSEEVENTF_LEFTUP, targetX, targetY, 0, IntPtr.Zero);

                return;
            }
        }

        public void MoveScrollBar(int scrollAmount, int step = 20)
        {
            InputSimulator input = new InputSimulator();

            for (int i = 0; i < step; i++)
            {
                input.Mouse.VerticalScroll(scrollAmount / step);
                Thread.Sleep(5);
            }
        }

        public void InputSearchText(string currentItem)
        {
            InputSimulator input = new InputSimulator();
            int countSymbols = currentItem.Length;
            char[] chars = currentItem.ToCharArray();

            if (random == null)
            {
                foreach (var symbol in chars)
                {
                    input.Keyboard.TextEntry(symbol);
                }
                return;
            }
            else
            {
                foreach (var symbol in chars)
                {
                    input.Keyboard.TextEntry(symbol);
                    Thread.Sleep(random.Next(0, delayK));
                }
                return;
            }
        }

        public void ClearSearchField()
        {
            InputSimulator input = new InputSimulator();

            input.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_A);
            Thread.Sleep(10);
            input.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
        }
    }
}
