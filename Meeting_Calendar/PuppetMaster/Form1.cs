using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class Form1 : Form
    {
        PuppetMaster pm = new PuppetMaster();
        public Form1()
        {
            InitializeComponent();
        }

        private void runScriptButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        string[] command = line.Split(' ');
                        switch (command[0])
                        {
                            case "Server":
                                pm.createServer(command[1], command[2], Int32.Parse(command[3]), Int32.Parse(command[4]), Int32.Parse(
                                    command[5]));
                                break;
                            case "Client":
                                pm.createClient(command[1], command[2], command[3], command[4]);
                                break;
                            case "AddRoom":
                                pm.addRoom(command[1], Int32.Parse(command[2]), command[3]);
                                break;
                            case "Status":
                                //TODO
                                break;
                            case "Wait":
                                //TODO
                                break;
                            case "Freeze":
                                //TODO
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (IOException)
                {
                }
            }

        }

        private void scriptTextbox_TextChanged(object sender, EventArgs e)
        {

        }

        private void clientsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        
        }

        private void serversListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
