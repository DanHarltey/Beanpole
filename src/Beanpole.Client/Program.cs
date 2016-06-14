namespace Beanpole.Client
{
    using HubClients;
    using log4net;
    using log4net.Config;
    using System;
    using System.Configuration;
    using System.ServiceProcess;
    using System.Windows.Forms;

    static class Program
    {
        private static ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            XmlConfigurator.Configure();
            Program.Log.Debug(typeof(Program).ToString() + " Starting");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledExceptionHandler);

            ConfigService config = new ConfigService();

            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                config
            };
#if DEBUG
            config.Start();
            Application.Run();
#else
            ServiceBase.Run(servicesToRun);
#endif
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = args.ExceptionObject as Exception;

            string errorMessage = "Unhandled Exception is terminating: " + args.IsTerminating.ToString();

            Program.Log.Fatal(errorMessage, ex);
        }
    }
}