using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Game_Caro
{
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();
        }
        string ReadFile(string file)
        {
            StreamReader sr = new StreamReader(file);
            string str;
            string result = "";
            str = sr.ReadLine();
            while (str != null)
            {
                result += str + '\n';
                str = sr.ReadLine();
            }
            sr.Close();
            return result;
        }

        private void History_Load(object sender, EventArgs e)
        {
            HistoryTab.Text = ReadFile("History.txt");
        }
    }
}
