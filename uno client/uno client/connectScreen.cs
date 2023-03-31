using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uno_client
{
    public partial class connectScreen : Form
    {
        public static NetworkStream stream;
        public connectScreen()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e) //connect button takes the ip address in ip box and the port and begins connecting
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    TcpClient server = new TcpClient();
                    server.Connect(IPAddress.Parse(txtIpAddress.Text), int.Parse(textBoxport.Text));
                    stream = server.GetStream();
                    Thread t = new Thread(beginame);
                    t.Start(); // has to be on new thread otherwise ui will freeze due to ui thread always being busy
                    done = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private delegate void UI();
        private void hideForm() // hides the connect screen and opens a game form. does this by calling the begin game function
        {
            Hide(); 
            game GameForm = new game();
            GameForm.Show();
            GameForm.beginGame();
        }
        public void beginame()
        {
            this.Invoke(new UI(hideForm)); // have to invoke as not on ui thread
        }
    }
}
