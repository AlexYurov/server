﻿#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using AgentInterface.Settings;
using Topshelf;
using UlteriusServer.Forms.Utilities;
using UlteriusServer.Utilities;
using UlteriusServer.Utilities.Usage;

#endregion

namespace UlteriusServer
{
    internal class Program
    {
        //Evan will have to support me and my cat once this gets released into the public.
        /// <summary>
        ///     Hide the console window from the user
        /// </summary>
        private static void HideWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, Hide);
        }

        private static void Main(string[] args)
        {
            //Fix screensize issues for Screen Share
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            HideWindow(); 
            try
            {
                if (
                    Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1) return;

                ProfileOptimization.SetProfileRoot(AppEnvironment.DataPath);
                ProfileOptimization.StartProfile("Startup.Profile");

                if (args.Length > 0)
                {
                    HostFactory.Run(x =>
                    {
                        x.RunAsLocalSystem();
                        x.EnableSessionChanged();
                        x.EnableServiceRecovery(r => { r.RestartService(1); });
                        x.SetDescription("The server that powers Ulterius");
                        x.SetDisplayName("Ulterius Server");
                        x.SetServiceName("UlteriusServer");
                        x.Service<UlteriusAgent>(s =>
                        {
                            s.ConstructUsing(name => new UlteriusAgent());
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                            s.WhenSessionChanged((se, e, id) => { se.HandleEvent(e, id); });
                        });
                    });
                }
                else
                {
                    var ulterius = new Ulterius();
                    ulterius.Start();
                    var hardware = new HardwareSurvey();
                    hardware.Setup();
                    if (Tools.RunningPlatform() == Tools.Platform.Windows)
                    {
                        UlteriusTray.ShowTray();
                    }
                    else
                        Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something unexpected occurred.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.Read();
            }
        }

        #region win32

        private const int Hide = 0;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();


        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        #endregion
    }
}