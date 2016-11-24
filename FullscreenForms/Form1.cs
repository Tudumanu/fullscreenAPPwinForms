using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace FullscreenForms
{
    public partial class GuyGame : Form
    {
        const string currentArgument = "/V /S";
        const string listArgument = "/V /L";
        const string changeArgument = "/X:{0} /Y:{1} /D";
        const string qResPath = @"..\QRes.exe";
        const string gamePath = @"C:\Program Files (x86)\guytest\GuyGame\GuyGame.exe";

        static Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = qResPath,
                Arguments = currentArgument,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        static Process game = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = gamePath
            }
        };

        static char[] splitChar = { 'x', ',' };
        static string currX = "";
        static string currY = "";
        static List<string> resolutionsX = new List<string>();
        static List<string> resolutionsY = new List<string>();

        public GuyGame()
        {
            InitializeComponent();
            this.Shown += GuyGame_Shown;
        }

        private void GuyGame_Shown(Object sender, EventArgs e)
        {
            //CHECK IF QRES AND GAME WAS FOUND
            bool filesOk = true;

            string errors = "";

            if (!File.Exists(game.StartInfo.FileName))
            {
                errors += "Game not found.\n";
                filesOk = false;
            }

            if (!File.Exists(proc.StartInfo.FileName))
            {
                errors += "Process not found.\n";
                filesOk = false;
            }

            if (!filesOk)
            {
                errors += "\nERROR: Missing files to run the game, try to reinstall!\n";
                MessageBox.Show(errors, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            //GET THE AVAILABLE MODES OS WIDHT AN HEIGHT
            proc.StartInfo.Arguments = listArgument;
            startQres(addResolutionsArray);
            comboBox1.DataSource = resolutionsX;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(comboBox1.SelectedIndex.ToString() + " a" + comboBox1.SelectedItem.ToString() + " b" + comboBox1.SelectedText.ToString() + " c" + comboBox1.SelectedValue.ToString());
            return;

            //Hide this console window
            this.Hide();

            //GET THE CURRENT WIDTH AND HEIGHT IN PIXELS AND SAVE
            proc.StartInfo.Arguments = currentArgument;
            startQres(saveResolution);

            //SET THE "BEST" RESOLUTION AVAILABLE (OR ALLOW USER TO PICK ONE)
            int index = resolutionsX.FindIndex("1024".Equals);
            proc.StartInfo.Arguments = String.Format(changeArgument, resolutionsX[index], resolutionsY[index]);
            proc.Start();
            proc.WaitForExit();

            //OPEN THE GAME AND WAIT FOR EXIT
            //System.Threading.Thread.Sleep(5000);
            game.Start();
            game.WaitForExit(); //BUG: if AIR installer appears (instead of the game), it will not wait for exit and resolution will be changed...

            //WHEN GAME CLOSES, RETURN TO THE SAVED WIDTH AND HEIGHT
            proc.StartInfo.Arguments = String.Format(changeArgument, currX, currY);
            proc.Start();
            proc.WaitForExit();

            this.Close();
        }

        static void startQres(Action<string, string> myAction)
        {
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                string[] words = line.Split(splitChar);

                if (words.Length > 1)
                {
                    myAction(words[0], words[1]);
                }
            }
        }

        static void addResolutionsArray(string x, string y)
        {
            resolutionsX.Add(x);
            resolutionsY.Add(y);
        }

        static void saveResolution(string x, string y)
        {
            currX = x;
            currY = y;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
