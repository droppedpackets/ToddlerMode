using System;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ToddlerMode
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Set DllImport for cleanly releasing process resources
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);

        public App()
        {
            // Handle any unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Register exit handler
            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Perform final cleanup on app exit
            try
            {
                // Release memory used by the process
                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    SetProcessWorkingSetSize(currentProcess.Handle, (IntPtr)(-1), (IntPtr)(-1));
                }

                // Force cleanup of any remaining resources
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception)
            {
                // Ignore errors during cleanup
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log any unhandled exceptions
            Exception ex = e.ExceptionObject as Exception;
            string errorMessage = ex != null ? ex.Message : "Unknown error occurred";

            MessageBox.Show($"An unexpected error occurred: {errorMessage}\n\nThe application will now exit.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}