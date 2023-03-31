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

namespace uno_client
{
    public partial class wildSelection : Form
    {
        public string colour = "";
        public wildSelection()
        {
            InitializeComponent();
        }
        private void btnRed_Click(object sender, EventArgs e)
        {
            colour = "red";
        }

        private void btnGreen_Click(object sender, EventArgs e)
        {
            colour = "green";
        }

        private void btnYellow_Click(object sender, EventArgs e)
        {
            colour = "yellow";
        }

        private void btnBlue_Click(object sender, EventArgs e)
        {
            colour = "blue";
        }
    }
}
