using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Bandidos_kliens_app
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Panel tartalmának törlése
            panel1.Controls.Clear();

            // UserControl1 példányosítása és hozzáadása a panelhez
            UserControl1 userControl1 = new UserControl1();
            userControl1.Dock = DockStyle.Fill; // Teljes kitöltés a panelben
            panel1.Controls.Add(userControl1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Panel tartalmának törlése
            panel1.Controls.Clear();

            // UserControl2 példányosítása és hozzáadása a panelhez
            UserControl2 userControl2 = new UserControl2();
            userControl2.Dock = DockStyle.Fill; // Teljes kitöltés a panelben
            panel1.Controls.Add(userControl2);
        }
    }
}
