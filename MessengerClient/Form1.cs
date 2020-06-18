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

namespace MessengerClient
{
    public partial class Form1 : Form
    {
        public delegate void AddListItem();
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        IPEndPoint ep;
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        string currentChat;
        public Form1()
        {
            InitializeComponent();
            listBox2.Items.Add("General Chat");
            ep = new IPEndPoint(ip, 10240);
            currentChat = "General Chat";
            Register();
            Task.Run(ReceiveMessage);
        }

        void Register()
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();

            this.Text = registerForm.Username;
            try
            {
                if (!s.Connected)
                {
                    s.Connect(ep);
                }
                byte[] buffer = new byte[1024];
                s.Send(System.Text.Encoding.ASCII.GetBytes(this.Text));
                Thread.Sleep(50);
                int l = s.Receive(buffer);
                string answer = Encoding.Unicode.GetString(buffer, 0, l);
                if (answer == "Ok")
                {
                    Task.Run(() => ReceiveMessage());
                }
                else
                {
                    MessageBox.Show(answer, "Answer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    Register();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int l;
            do
            {
                l = s.Receive(buffer);
                string msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                if (msg == "$123$%")
                {
                    l = s.Receive(buffer);
                    msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                    listBox2.Invoke((MethodInvoker)delegate { listBox2.Items.Add(msg); });
                }
                else if (msg == "$321$%")
                {
                    l = s.Receive(buffer);
                    msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                    listBox2.Invoke((MethodInvoker)delegate { listBox2.Items.Remove(msg); });
                }
                else if(msg == "$213$%")
                {
                    l = s.Receive(buffer);
                    msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                    int j = -1;
                    string mess = null;
                    foreach (var item in listBox2.Items)
                    {
                        if (msg.Contains("(") && msg.Contains(")"))
                        {
                            char[] number = new char[20];
                            mess = msg.Remove(msg.IndexOf('('));
                            if (mess == msg)
                            {
                                msg.CopyTo(msg.IndexOf('(') + 1, number, 0, msg.IndexOf(')') - msg.IndexOf('(') - 1);
                                string str = new string(number);
                                int i = int.Parse(str);
                                i++;
                                mess += "(" + i.ToString() + ")";
                                j = listBox2.Items.IndexOf(item);

                                break;
                            }
                        }
                        else
                        {
                            if (msg == item.ToString())
                            {
                                msg += "(1)";
                                j = listBox2.Items.IndexOf(item);
                            }
                        }
                    }
                    if (mess != null)
                    {
                        msg = mess;
                    }
                    if (j >= 0)
                    {
                        listBox2.Invoke((MethodInvoker)delegate
                        {
                            listBox2.Items.Insert(j, msg);
                        });
                    }
                }
                else if (msg == "$7$%")
                {
                    s.Send(Encoding.Unicode.GetBytes(currentChat));
                    Thread.Sleep(50);
                    l = s.Receive(buffer);
                    msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                    if (msg == "$213$%")
                    {
                        l = s.Receive(buffer);
                        msg = System.Text.Encoding.Unicode.GetString(buffer, 0, l);
                        int j = -1;
                        string mess = null;
                        foreach (var item in listBox2.Items)
                        {
                            if (msg.Contains("(") && msg.Contains(")"))
                            {
                                char[] number = new char[20];
                                mess = msg.Remove(msg.IndexOf('('));
                                if(mess == msg)
                                {
                                    msg.CopyTo(msg.IndexOf('(') + 1, number, 0, msg.IndexOf(')') - msg.IndexOf('(') - 1);
                                    string str = new string(number);
                                    int i = int.Parse(str);
                                    i++;
                                    mess += "(" + i.ToString() + ")";
                                    j = listBox2.Items.IndexOf(item);
                                    
                                    break;
                                }
                            }
                            else
                            {
                                if(msg == item.ToString())
                                {
                                    msg += "(1)";
                                    j = listBox2.Items.IndexOf(item);
                                }
                            }
                        }
                        if(mess != null)
                        {
                            msg = mess;
                        }
                        if(j >= 0)
                        {
                            listBox2.Invoke((MethodInvoker)delegate {
                                listBox2.Items.Insert(j, msg);
                            });
                        }
                    }
                }
                else listBox2.Invoke((MethodInvoker)delegate
                {
                    listBox1.Items.Add(msg);
                }) ;
            } while (true);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (s.Connected)
            {
                s.Send(System.Text.Encoding.Unicode.GetBytes($"{currentChat}${messageContent.Text}${DateTime.Now.Ticks.ToString()}"));
                messageContent.Text = "";
            }
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (s.Connected)
            {
                object obj = listBox2.SelectedItem;
                listBox1.Items.Clear();
                byte[] buffer = new byte[1024];
                int l;
                if (obj.ToString().Contains('(') && obj.ToString().Contains(')'))
                {
                    obj.ToString().Remove(obj.ToString().IndexOf('('));
                }
                currentChat = obj.ToString();
                groupBox1.Text = currentChat;
                s.Send(Encoding.Unicode.GetBytes("$456$%"));
                Thread.Sleep(50);
                s.Send(Encoding.Unicode.GetBytes(currentChat));
                l = s.Receive(buffer);
                string msg = Encoding.Unicode.GetString(buffer, 0, l);
                if (msg != "No messages")
                {
                    string[] msgs = msg.Split('^');

                    foreach (var message in msgs)
                    {
                        listBox1.Items.Add(message);
                    }
                }
                else
                {
                    MessageBox.Show(msg);
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            
        }
    }
}

