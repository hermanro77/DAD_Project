using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
            //Console.WriteLine("Write commands");
            //string[] command;
            //do
            //{
            //    command = Console.ReadLine().Split(' ');
            //    Console.WriteLine("Command:", command);
            //    switch (command[0])
            //    {
            //        case "AddRoom":

            //            break;

            //        case "Server":

            //            break;
            //        case "Client":

            //            break;

            //        case "Status":

            //            break;

            //        case "Wait":

            //            break;

            //        case "Freeze":

            //            break;

            //        case "Unfreeze":

            //            break;

            //        case "Crash":
            //            PM.serverKill(command[1]);
            //            break;

            //        default:
            //            break;
            //    }
            //} while (command != null);
        }
    }

}
