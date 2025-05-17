using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using WindowsInput;
using WindowsInput.Native;

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public interface IInputEmulator
    {
        Task MoveMouseAsync(Point target, bool withClick = true, int steps = 20);
        Task InputTextAsync(string text);
        Task ClearFieldAsync();
    }

    public class InputEmulator : IInputEmulator, IDisposable
    {
        private readonly IInputSimulator _inputSimulator;
        private static Random _random;
        private bool _disposed;
        public static int? _mouseDelay { get; set; }
        public static int? _keyboardDelay { get; set; }


        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, nint dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public InputEmulator(bool enableRandomization = false)
        {
            _inputSimulator = new InputSimulator();
            _random = enableRandomization ? new Random() : null;
        }

        public static void SetDelays(int? mouseDelay, int? keyboardDelay)
        {
            _mouseDelay = mouseDelay;
            _keyboardDelay = keyboardDelay;

            _random = new Random();
        }

        public async Task MoveMouseAsync(Point target, bool withClick = true, int steps = 20)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(InputEmulator));

            // Режим без задержек
            if (!_mouseDelay.HasValue)
            {
                Cursor.Position = target;
                if (withClick) PerformClickImmediate(target);
                return;
            }

            // Режим с задержками
            if (_random == null || steps <= 1)
            {
                Cursor.Position = target;
                if (withClick) await PerformClickWithDelayAsync(target);
                return;
            }

            await SmoothMoveWithDelayAsync(target, steps, withClick);
        }

        private void PerformClickImmediate(Point target)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, target.X, target.Y, 0, nint.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, target.X, target.Y, 0, nint.Zero);
        }

        private async Task PerformClickWithDelayAsync(Point target)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, target.X, target.Y, 0, nint.Zero);
            await Task.Delay(_random?.Next(_mouseDelay.Value / 2, _mouseDelay.Value) ?? _mouseDelay.Value);
            mouse_event(MOUSEEVENTF_LEFTUP, target.X, target.Y, 0, nint.Zero);
        }

        private async Task SmoothMoveWithDelayAsync(Point target, int steps, bool withClick)
        {
            var current = Cursor.Position;
            for (int i = 1; i <= steps; i++)
            {
                if (_disposed) return;

                double ratio = (double)i / steps;
                int newX = current.X + (int)((target.X - current.X) * ratio);
                int newY = current.Y + (int)((target.Y - current.Y) * ratio);

                if (_random != null)
                {
                    newX += _random.Next(-2, 3);
                    newY += _random.Next(-2, 3);
                }

                Cursor.Position = new Point(newX, newY);
                await Task.Delay(_mouseDelay.Value / steps);
            }

            if (withClick)
            {
                await PerformClickWithDelayAsync(target);
            }
        }

        public async Task InputTextAsync(string text)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(InputEmulator));
            if (string.IsNullOrEmpty(text)) return;

            // Режим без задержек
            if (!_keyboardDelay.HasValue)
            {
                _inputSimulator.Keyboard.TextEntry(text);
                return;
            }

            // Режим с задержками
            foreach (char symbol in text)
            {
                if (_disposed) return;

                _inputSimulator.Keyboard.TextEntry(symbol);
                await Task.Delay(_random?.Next(0, _keyboardDelay.Value) ?? _keyboardDelay.Value);
            }
        }

        public async Task ClearFieldAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(InputEmulator));

            _inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            await Task.Delay(_keyboardDelay ?? 10);
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
        }

        public Point RectangleToPoint(Rectangle rect)
        {
            return new Point(rect.Size);
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
