using System;
using System.Collections.Generic;
using System.Linq;

namespace Statistiks.Lib
{
    public class StatistiksLib
    {
        private KeyboardHook _kHook;
        private MouseHook _mHook;
        private WindowHook _wHook;

        #region Data
        private Dictionary<string, ulong> _keyboardEvents;
        private Dictionary<MouseMessage, ulong> _mouseEvents;
        private int[,] _mouseMove;
        private ulong _mouseMoveLength;
        private Dictionary<string, ulong> _windowEvents;
        private IntPtr _activeWindow;
        #endregion

        #region Properties
        public Dictionary<string, ulong> KeyboardEvents { get { return _keyboardEvents; } }
        public Dictionary<MouseMessage, ulong> MouseEvents { get { return _mouseEvents; } }
        public Dictionary<string, ulong> WindowEvents { get { return _windowEvents; } }
        public ulong MouseMoveLength { get { return _mouseMoveLength; } }
        #endregion

        public StatistiksLib()
        {
            _keyboardEvents = new Dictionary<string, ulong>();
            _mouseEvents = new Dictionary<MouseMessage, ulong>();
            _mouseMove = new int[2,2];
            _mouseMoveLength = new ulong();
            _windowEvents = new Dictionary<string, ulong>();
            _activeWindow = IntPtr.Zero;
            _kHook = new KeyboardHook();
            _kHook.EventRaised += _kHookEventRaised;
            _mHook = new MouseHook();
            _mHook.EventRaised += _mHookEventRaised;
            _wHook = new WindowHook();
            _wHook.EventRaised += _wHookEventRaised;
        }

        private void _wHookEventRaised(object sender, WindowEventArgs e)
        {
            _activeWindow = e.hWnd;
            if (_windowEvents.ContainsKey(e.ExePath))
                _windowEvents[e.ExePath] += 1;
            else
                _windowEvents.Add(e.ExePath, 1);
        }

        private void _mHookEventRaised(object sender, MouseEventArgs e)
        {
            if (e.Message == MouseMessage.WM_MOUSEMOVE)
                if (_mouseMove.Length == 0) {
                    _mouseMove[0, 0] = e.Point.X;
                    _mouseMove[0, 1] = e.Point.Y;
                    _mouseMoveLength = 0;
                } else {
                    _mouseMoveLength += (ulong)Math.Sqrt(Math.Pow(e.Point.X - _mouseMove[0, 0], 2) + Math.Pow(e.Point.Y - _mouseMove[0, 1], 2)) / 38; // magic 38!
                    _mouseMove[0, 0] = e.Point.X;
                    _mouseMove[0, 1] = e.Point.Y;
                }
            
            if (_mouseEvents.ContainsKey(e.Message))
                _mouseEvents[e.Message] += 1;
            else
                _mouseEvents.Add(e.Message, 1);
        }

        private void _kHookEventRaised(object sender, KeyboardHookEventArgs e)
        {
            if (e.Message == KeyboardMessage.WM_KEYDOWN || e.Message == KeyboardMessage.WM_SYSKEYDOWN)
            {
                var key = e.VkCode.KeyCodeToUnicodeString(e.ScanCode, _activeWindow, _keyboardEvents.Keys.Skip(_keyboardEvents.Count - 3).Take(3).ToArray());
                if (key == "RButton" || key == "LButton" || key == "MButton" || key == "XButton1" || key == "XButton2")
                    return; // TODO: decide what need to do with mouse keys
                if (_keyboardEvents.ContainsKey(key))
                    _keyboardEvents[key] += 1;
                else
                    _keyboardEvents.Add(key, 1);
            }
        }

        public void Unhook()
        {
            _kHook.Unhook();
            _mHook.Unhook();
            _wHook.Unhook();
        }
    }
}
