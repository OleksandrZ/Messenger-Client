using System;
using System.Windows.Forms;

namespace MessengerClient
{
    public partial class RegisterForm : Form
    {
        public string Username { get; set; }

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (username.TextLength > 2 && username.TextLength <= 10)
            {
                Username = username.Text;
                this.Close();
            }
            else if (username.TextLength < 3)
            {
                MessageBox.Show("Your username is too short (3 symobols min)");
            }
            else
            {
                MessageBox.Show("Your username is too long (10 symobols max)");
            }
        }

        private void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }
    }
}