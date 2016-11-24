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
        const int MIN_WIDTH = 1280; //HD 16:9 minimum (1280x720)
        const int MIN_HEIGHT = 720;
        const string currentArgument = "/V /S";
        const string listArgument = "/V /L";
        const string changeArgument = "/X:{0} /Y:{1} /D";
        const string qResPath = @"..\QRes.exe";
        const string gamePath = @"C:\Program Files (x86)\guytest\GuyGame\GuyGame.exe";

        public class Resolucao
        {
            public int Index { get; set; }
            public string Value { get; set; }

            public Resolucao(int index, string val)
            {
                Index = index;
                Value = val;
            }

            public override bool Equals(Object obj)
            {
                // Check for null values and compare run-time types.
                if (obj == null || GetType() != obj.GetType())
                    return false;

                Resolucao res = (Resolucao)obj;
                return Value.Equals(res.Value);
            }
        }

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
        static List<Resolucao> resolutionsXY = new List<Resolucao>();

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

            for(int i=0; i<resolutionsX.Count; i++)
            {
                string val = resolutionsX[i] + "x" + resolutionsY[i];
                Resolucao res = new Resolucao(i, val);
                if (resolutionsXY.FindIndex(res.Equals) < 0)
                {
                    int x = -1;
                    int y = -1;
                    Int32.TryParse(resolutionsX[i], out x);
                    Int32.TryParse(resolutionsY[i], out y);
                    if (x >= MIN_WIDTH && y >= MIN_HEIGHT)
                        resolutionsXY.Add(res);
                }
            }

            resolutionsXY.Reverse();

            comboBox1.DataSource = resolutionsXY;
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Index";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            Int32.TryParse(comboBox1.SelectedValue.ToString(), out selectedIndex);

            if (selectedIndex < 0)
            {
                MessageBox.Show("Resolution selected ERROR!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Hide this console window
            this.Hide();

            //GET THE CURRENT WIDTH AND HEIGHT IN PIXELS AND SAVE
            proc.StartInfo.Arguments = currentArgument;
            startQres(saveResolution);

            //SET THE "BEST" RESOLUTION AVAILABLE (OR ALLOW USER TO PICK ONE)
            proc.StartInfo.Arguments = String.Format(changeArgument, resolutionsX[selectedIndex], resolutionsY[selectedIndex]);
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

    }
}
