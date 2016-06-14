namespace Beanpole.Client
{
    using log4net;
    using System;
    using System.IO;
    using System.Threading;

    internal class ConfigUpdater : Disposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigUpdater));

        private readonly ConfigOverrideManager configManager;
        private readonly RemoteFile file;

        private readonly object fileWatcherLock = new object();
        private readonly FileSystemWatcher fileWatcher;
        private readonly Timer timer;

        public ConfigUpdater(MachineConfig machineConfig, ConfigOverrideManager configManager)
        {
            if (configManager == null)
            {
                throw new ArgumentNullException(nameof(configManager));
            }

            this.configManager = configManager;

            this.file = new RemoteFile(machineConfig);
            string dic = Path.GetDirectoryName(this.configManager.ConfigPath);

            this.fileWatcher = new FileSystemWatcher(dic);
            this.fileWatcher.Changed += this.FileChanged;
            // sometimes we are deployed before the web.config is wrote
            this.fileWatcher.Created += this.FileChanged;
            this.fileWatcher.EnableRaisingEvents = true;

            TimeSpan reloadTime = TimeSpan.FromMinutes(1);
            this.timer = new Timer(ConfigUpdater.TimerCallback, this, TimeSpan.Zero, reloadTime);
        }

        public void UpdateToLastest()
        {
            lock (this.file)
            {
                if (this.file.Update())
                {
                    ConfigUpdater.Log.Debug("Remote files has updates");
                    this.UpdateFile();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
            }

            if (this.fileWatcher != null)
            {
                this.fileWatcher.Changed -= this.FileChanged;
                this.fileWatcher.Dispose();
            }

            base.Dispose(disposing);
        }

        private void TimerCallback()
        {
            ConfigUpdater.Log.Debug("TimerCallback");

            try
            {
                this.UpdateToLastest();
            }
            catch (Exception ex)
            {
                // do not rethrow as this will terminate the program
                ConfigUpdater.Log.Error("Exception on the TimerCallback.", ex);
            }
        }

        private void UpdateFile()
        {
            lock (this.fileWatcherLock)
            {
                this.fileWatcher.EnableRaisingEvents = false;

                try
                {
                    ConfigUpdater.Log.Debug("Loading local file");

                    ConfigUpdater.Log.Debug("Loading remote file");
                    string updateUsing = this.file.Content;

                    this.configManager.Replace(updateUsing);
                    ConfigUpdater.Log.Info("Updated local file");
                }
                catch (IOException ex)
                {
                    ConfigUpdater.Log.Warn(ex);
                }

                this.fileWatcher.EnableRaisingEvents = true;
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (string.Equals(e.FullPath, this.configManager.ConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                ConfigUpdater.Log.Debug("File update detected");
                this.UpdateFile();
            }
        }

        private static void TimerCallback(object state)
        {
            ConfigUpdater obj = (ConfigUpdater)state;
            obj.TimerCallback();
        }
    }
}