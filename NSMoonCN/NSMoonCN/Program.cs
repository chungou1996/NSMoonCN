using NSMoonCN.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSMoonCN
{
    static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            switch (args.Name.Split(',')[0])
            {
                case "DevComponents.DotNetBar2":
                    assembly = Assembly.Load(Resources.DevComponents_DotNetBar2);
                    break;
                case "NSMoonPak":
                    assembly = Assembly.Load(Resources.NSMoonPak);
                    break;
                default:
                    assembly = null;
                    break;
            }
            return assembly;
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
