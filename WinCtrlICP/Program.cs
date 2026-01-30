using System.Runtime.InteropServices;

namespace WinCtrlICP
{
    internal static class Program
    {
        private const string AppMutexName = @"Local\WinCtrlICP-3F6F2D4F";
        private const string ActivateEvent = @"Local\WinCtrlICP-Activate-3F6F2D4F";

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static void AddExeDirToDllSearchPath()
        {
            var exeDir = AppContext.BaseDirectory;
            SetDllDirectory(exeDir);
        }

        [STAThread]
        static void Main()
        {
            AddExeDirToDllSearchPath();
            bool createdNew;
            using var mutex = new Mutex(true, AppMutexName, out createdNew);

            if (!createdNew)
            {
                try
                {
                    using var ev = EventWaitHandle.OpenExisting(ActivateEvent);
                    ev.Set();
                }
                catch { }
                return;
            }

            using var activateHandle = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEvent);

            ApplicationConfiguration.Initialize();
            var form = new F16DEDWriterForm();

            // background waiter: bring the window up if someone else launches us
            var waiter = new Thread(() =>
            {
                while (true)
                {
                    activateHandle.WaitOne();
                    form.BeginInvoke(new Action(form.ActivateFromExternalSignal));
                }
            })
            { IsBackground = true };
            waiter.Start();

            Application.Run(form);
        }
    }
}