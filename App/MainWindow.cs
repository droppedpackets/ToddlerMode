using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ToddlerMode
{
    /// <summary>
    /// ToddlerMode - A simple application to restrict keyboard input in Windows
    /// and keep the application in full screen to prevent toddlers from
    /// accidentally pressing problematic key combinations.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Import necessary Win32 functions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        private bool _toddlerModeActive = false;
        private Button _toggleButton;
        private TextBlock _statusText;

        // List to track currently pressed keys
        private HashSet<Key> _pressedKeys = new HashSet<Key>();

        public MainWindow()
        {
            InitializeWindow();

            // Create hook callback
            _proc = HookCallback;
        }

        private void InitializeWindow()
        {
            // Set window properties
            Title = "ToddlerMode - Keyboard Restrictor";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Colors.LightSkyBlue);

            // Create main layout grid
            Grid mainGrid = new Grid();

            // Define rows
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Create header text
            TextBlock headerText = new TextBlock
            {
                Text = "ToddlerMode",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            Grid.SetRow(headerText, 0);

            // Create description text
            TextBlock descriptionText = new TextBlock
            {
                Text = "When activated, this app will keep the window in fullscreen mode and allow normal typing\n" +
                       "while blocking system shortcuts and other potentially disruptive key combinations.\n" +
                       "Use Ctrl+Alt+Delete or Ctrl+Alt+Esc to exit Toddler Mode if needed.",
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(50, 20, 50, 30)
            };
            Grid.SetRow(descriptionText, 1);

            // Create toggle button
            _toggleButton = new Button
            {
                Content = "Activate ToddlerMode",
                FontSize = 20,
                Padding = new Thickness(20, 10, 20, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Colors.LightGreen)
            };
            _toggleButton.Click += ToggleToddlerMode;
            Grid.SetRow(_toggleButton, 2);

            // Create status text
            _statusText = new TextBlock
            {
                Text = "Status: Inactive",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(_statusText, 3);

            // Add all elements to the grid
            mainGrid.Children.Add(headerText);
            mainGrid.Children.Add(descriptionText);
            mainGrid.Children.Add(_toggleButton);
            mainGrid.Children.Add(_statusText);

            // Set the content of the window
            Content = mainGrid;

            // Handle closing to ensure hooks are removed
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Ensure we unhook on close
                DeactivateToddlerMode();

                // Perform additional cleanup
                CleanupResources();
            }
            catch (Exception ex)
            {
                // Log any errors during shutdown
                MessageBox.Show($"Error during shutdown: {ex.Message}", "Shutdown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CleanupResources()
        {
            // Ensure keyboard hook is unhooked
            if (_hookID != IntPtr.Zero)
            {
                try
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }
                catch (Exception)
                {
                    // Already tried to unhook in DeactivateToddlerMode, ignore if failed here
                }
            }

            // Clear pressed keys collection
            _pressedKeys.Clear();

            // Force Garbage Collection to clean up resources
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void ToggleToddlerMode(object sender, RoutedEventArgs e)
        {
            if (_toddlerModeActive)
            {
                DeactivateToddlerMode();
            }
            else
            {
                ActivateToddlerMode();
            }

            // Force focus away from the button
            Keyboard.ClearFocus();
        }

        private void ActivateToddlerMode()
        {
            // Set full screen mode
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Topmost = true;

            // Set the hook
            _hookID = SetHook(_proc);

            // Update UI
            _toddlerModeActive = true;
            _toggleButton.Content = "Deactivate ToddlerMode";
            _toggleButton.Background = new SolidColorBrush(Colors.LightPink);
            _statusText.Text = "Status: Active - Keyboard restricted";

            // Hide the button completely in toddler mode
            _toggleButton.Visibility = Visibility.Collapsed;

            // Move focus away from the button to prevent Enter/Space from triggering it
            // Clear keyboard focus entirely
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Keyboard.ClearFocus();
            }));
        }

        private void DeactivateToddlerMode()
        {
            // Remove full screen mode
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            Topmost = false;

            // Unhook if needed
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            // Update UI
            _toddlerModeActive = false;
            _toggleButton.Content = "Activate ToddlerMode";
            _toggleButton.Background = new SolidColorBrush(Colors.LightGreen);
            _statusText.Text = "Status: Inactive";

            // Show the button again
            _toggleButton.Visibility = Visibility.Visible;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, int wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // Read the key information
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                // Check for key down or up messages
                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    _pressedKeys.Add(key);
                }
                else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                {
                    _pressedKeys.Remove(key);
                }

                // Check for Ctrl+Alt+Delete
                bool isControlPressed = _pressedKeys.Contains(Key.LeftCtrl) || _pressedKeys.Contains(Key.RightCtrl);
                bool isAltPressed = _pressedKeys.Contains(Key.LeftAlt) || _pressedKeys.Contains(Key.RightAlt);
                bool isDeletePressed = _pressedKeys.Contains(Key.Delete);
                bool isWindowsKeyPressed = _pressedKeys.Contains(Key.LWin) || _pressedKeys.Contains(Key.RWin);
                bool isEnterPressed = key == Key.Enter || key == Key.Return;
                bool isSpacePressed = key == Key.Space;

                // Allow Ctrl+Alt+Delete to pass through
                if (isControlPressed && isAltPressed && isDeletePressed)
                {
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                // Block problematic key combinations when toddler mode is active
                if (_toddlerModeActive)
                {
                    // Special allowance for ESC key to exit toddler mode for parents
                    if (key == Key.Escape && isControlPressed && isAltPressed)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DeactivateToddlerMode();
                        });
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }

                    // Specifically handle Space and Enter to prevent button activation
                    if ((isEnterPressed || isSpacePressed) && _toggleButton.IsFocused)
                    {
                        // Don't let these keys trigger the button - explicitly block them when button has focus
                        // But still pass them through for other UI elements
                        Keyboard.ClearFocus();
                        return (IntPtr)1;
                    }

                    // List of keys to block in toddler mode
                    bool isBlockedModifier = isWindowsKeyPressed;

                    // Block Alt+Tab and similar window switching
                    if (isAltPressed && (key == Key.Tab || key == Key.F4))
                        return (IntPtr)1;

                    // Block Windows key combinations
                    if (isWindowsKeyPressed)
                        return (IntPtr)1;

                    // Block Ctrl+Esc (Start menu)
                    if (isControlPressed && key == Key.Escape)
                        return (IntPtr)1;

                    // Block function keys
                    if (key >= Key.F1 && key <= Key.F12)
                        return (IntPtr)1;

                    // Allow normal typing keys to pass through
                    // This ensures the toddler can type in the current window
                }
            }

            // Pass the hook on
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}