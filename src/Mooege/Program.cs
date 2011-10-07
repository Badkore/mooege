/*
 * Copyright (C) 2011 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Mooege.Common;
using Mooege.Core.MooNet.Accounts;
using Mooege.Core.MooNet.Commands;
using Mooege.Core.MooNet.Online;
using Mooege.Net.GS;
using Mooege.Net.MooNet;

namespace Mooege
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public static MooNetServer MooNetServer;
        public static GameServer GameServer;

        public static void Main(string[] args)
        {
            // Watch for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            
            // Don't forget this..
            LogManager.Enabled = true;
            LogManager.AttachLogTarget(new ConsoleTarget(Level.Trace));
            LogManager.AttachLogTarget(new FileTarget(Level.Trace, "mooege-log.txt"));

            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintBanner();
            PrintLicense();
            Console.ResetColor();

            Logger.Info("mooege v{0} warming-up..", Assembly.GetExecutingAssembly().GetName().Version);
            StartupServers();
        }

        private static void StartupServers()
        {
            MooNetServer = new MooNetServer();
            GameServer = new GameServer();

            var bnetServerThread = new Thread(MooNetServer.Run) { IsBackground = true };
            bnetServerThread.Start();

            var gameServerThread = new Thread(GameServer.Run) { IsBackground = true };
            gameServerThread.Start();

            while (true)
            {
                var line = Console.ReadLine();
                CommandManager.Parse(line);                
            }
        }

        private static void PrintBanner()
        {
            Console.WriteLine(@"  _ __ ___    ___    ___    ___   __ _   ___ ");
            Console.WriteLine(@" | '_ ` _ \  / _ \  / _ \  / _ \ / _` | / _ \");
            Console.WriteLine(@" | | | | | || (_) || (_) ||  __/| (_| ||  __/");
            Console.WriteLine(@" |_| |_| |_| \___/  \___/  \___| \__, | \___|");
            Console.WriteLine(@"                                 |___/       ");
            Console.WriteLine();
        }

        private static void PrintLicense()
        {
            Console.WriteLine("Copyright (C) 2011 mooege project");
            Console.WriteLine("mooege comes with ABSOLUTELY NO WARRANTY.");
            Console.WriteLine("This is free software, and you are welcome to redistribute it under certain conditions; see the LICENSE file for details.");
            Console.WriteLine();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                Logger.FatalException((e.ExceptionObject as Exception), "Application terminating because of unhandled exception.");
            else
                Logger.ErrorException((e.ExceptionObject as Exception), "Caught unhandled exception.");
            Console.ReadLine();
        }

        #region general commands 

        [ServerCommand("stats")]
        public static void Stats(string parameters)
        {
            // warning: only use mono-enabled counters here - http://www.mono-project.com/Mono_Performance_Counters

            if(parameters.ToLower()=="help")
            {
                Console.WriteLine("stats [detailed]");
                return;
            }

            var output = new StringBuilder();
            output.AppendFormat("Total Accounts: {0}, Online Players: {1}", AccountManager.TotalAccounts, PlayerManager.OnlinePlayers.Count);

            if (parameters.ToLower() != "detailed")
            {
                Console.WriteLine(output.ToString());
                return;
            }

            var processorTime = new PerformanceCounter {CategoryName = "Processor", CounterName = "% Processor Time", InstanceName = "_Total"};
            var exceptionsThrown = new PerformanceCounter { CategoryName = ".NET CLR Exceptions", CounterName = "# of Exceps Thrown", InstanceName=Process.GetCurrentProcess().ProcessName };
            var physicalThreads = new PerformanceCounter { CategoryName = ".NET CLR LocksAndThreads", CounterName = "# of current physical Threads", InstanceName = Process.GetCurrentProcess().ProcessName };
            var logicalThreads = new PerformanceCounter { CategoryName = ".NET CLR LocksAndThreads", CounterName = "# of current logical Threads", InstanceName = Process.GetCurrentProcess().ProcessName };
            var contentionRate = new PerformanceCounter { CategoryName = ".NET CLR LocksAndThreads", CounterName = "Contention Rate / sec", InstanceName = Process.GetCurrentProcess().ProcessName };

            output.AppendFormat(
                "\nGC Allocated Memory: {0} KB Processor Time: {1}% Exceptions Thrown: {2}\nThreads - Physical: {3}, Logical: {4} Contention Rate: {5}/sec",
                GC.GetTotalMemory(true)/1024,processorTime.NextValue(), exceptionsThrown.NextValue(), physicalThreads.NextValue(), logicalThreads.NextValue(),contentionRate.NextValue());

            Console.WriteLine(output.ToString());
        }

        [ServerCommand("version")]
        public static void Version(string parameters)
        {
            Console.WriteLine("v{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        [ServerCommand("shutdown")]
        public static void Shutdown(string parameters)
        {
            if (MooNetServer == null && GameServer == null) return;

            if (MooNetServer != null)
            {
                Logger.Warn("Shutting down MooNet-Server..");
                MooNetServer.Shutdown();
                MooNetServer = null;
            }

            if (GameServer != null)
            {
                Logger.Warn("Shutting down Game-Server..");
                GameServer.Shutdown();
                GameServer = null;
            }
        }

        #endregion
    }
}
