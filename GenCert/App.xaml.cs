using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GenCert
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Here is the once-per-class call to initialize the log object
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool lDisplaySplash = true;

        public App()
        {
            XmlConfigurator.Configure(); //only once

            if (lDisplaySplash)
            {


            }

            //http://www.codeproject.com/Articles/1089718/Global-Exceptions-Handling-in-WPF
            Dispatcher.UnhandledException += App_DispatcherUnhandledException;
            // ovo iznad i ispod je vrlo slicno u 99% slucajeva za WPF applikacije - jedino ako ima vise UI WPF interfejsa (sto je moguce) se razlikuje
            DispatcherUnhandledException += App_DispatcherUnhandledException; //Example 2

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; //Example 4
            System.Windows.Forms.Application.ThreadException += WinFormApplication_ThreadException; //Example 5
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException; // Example 1
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Example 3

        }
        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            log.Warn(e.Exception);   // This could be used here to log ALL errors, even those caught by a Try/Catch block
        }

        // Example 2
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            log.Fatal(e.Exception);
            e.Handled = true;

            CrashReporterGlobalField._ReportCrash.Send(e.Exception);
            if (CrashReporterGlobalField._ReportCrash.IsQuit)
            {
                // shut down the application nicely.
                App.Current.Shutdown(-1);
            }

        }

        // Example 3
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            //MessageBox.Show("3. CurrentDomain_UnhandledException");

            var exception = e.ExceptionObject as Exception;
            log.Fatal(exception);

            MessageBox.Show(exception.Message + "\n" + "Application must be close !!!", "CurrentDomain UnhandledException",
                MessageBoxButton.OK, MessageBoxImage.Error);

            if (e.IsTerminating)
            {
                //Now is a good time to write that critical error file!
                //MessageBox.Show("Goodbye world!");

                // This exception cannot be handled and you cannot reliably use Shutdown to gracefully shutdown.
                // The only way to suppress the CLR error dialog is to supply "1" to the exit code.
                Environment.Exit(1);
            }
        }

        // Example 4
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            log.Fatal(e.Exception);
            MessageBox.Show(e.Exception.Message, "TaskScheduler UnobservedTaskException",
                MessageBoxButton.OK, MessageBoxImage.Error);

            //CrashReporterGlobalField._ReportCrash.Send(e.Exception);   //your code

            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    CrashReporterGlobalField._ReportCrash.Send(e.Exception);   //your code
            //}); 

            e.SetObserved();
        }

        // Example 5
        void WinFormApplication_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //MessageBox.Show("5. WinFormApplication_ThreadException");
            log.Fatal(e.Exception);

            CrashReporterGlobalField._ReportCrash.Send(e.Exception);

        }
    }
}
