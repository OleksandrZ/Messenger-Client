using System;
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
        //Сохраняет название чата в котором находится пользователь в данный момент
        //нужно для правильной отправки и получения сообщений
        private string currentChat;

        private IPEndPoint ep;

        private IPAddress ip = IPAddress.Parse("127.0.0.1");

        private Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        public delegate void AddListItem();

        public Form1()
        {
            InitializeComponent();
            listBox2.Items.Add("General Chat");
            ep = new IPEndPoint(ip, 10240);
            currentChat = "General Chat";
            groupBox1.Text = currentChat;
            listBox2.SelectedIndex = 0;
            Register();
            Task.Run(ReceiveMessage);
            //При подключении чат по-умолчанию это General Chat и поэтому спустя небольшую задержку
            //на сервер отправляется запрос на получения всех сообщений из текущего чата
            Task.Run(() =>
            {
                Thread.Sleep(100);
                s.Send(Encoding.Unicode.GetBytes("$456$%^" + currentChat));
            });
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (s.Connected)
            {
                string selectedChat = listBox2.SelectedItem.ToString();
                if (selectedChat.Contains("("))
                {
                    selectedChat = selectedChat.Remove(selectedChat.LastIndexOf('('));
                    int i = listBox2.Items.IndexOf(listBox2.SelectedItem);
                    listBox2.Items.Insert(i, selectedChat);
                    listBox2.SelectedItem = selectedChat;
                    listBox2.Items.RemoveAt(i + 1);
                }
                currentChat = listBox2.SelectedItem.ToString();
                groupBox1.Text = currentChat;
                s.Send(Encoding.Unicode.GetBytes("$456$%^" + currentChat));
            }
        }

        private void ReceiveMessage()
        {
            byte[] buffer = new byte[5000];
            int l;
            try
            {
                do
                {
                    l = s.Receive(buffer);
                    string msg = Encoding.Unicode.GetString(buffer, 0, l);
                    //Добавление новоподключившегося пользователя
                    if (msg.StartsWith("$123$%"))
                    {
                        string[] msgs = msg.Split('^');
                        foreach (var message in msgs)
                        {
                            if (message != "$123$%")
                            {
                                listBox2.Invoke((MethodInvoker)delegate { listBox2.Items.Add(message); });
                            }
                        }
                    }
                    //Удаление отключающегося пользователя
                    else if (msg.StartsWith("$321$%"))
                    {
                        string[] msgs = msg.Split('^');
                        foreach (var message in msgs)
                        {
                            if (message != "$123$%")
                            {
                                listBox2.Invoke((MethodInvoker)delegate { listBox2.Items.Remove(message); });
                            }
                        }
                    }
                    //Добавление всех отправленных сообщений в данный чат
                    else if (msg.StartsWith("$456$%"))
                    {
                        string[] msgs = msg.Split('^');
                        listBox1.Invoke((MethodInvoker)delegate
                        {
                            listBox1.Items.Clear();
                        });
                        foreach (var message in msgs)
                        {
                            if (message != "$456$%")
                            {
                                listBox1.Invoke((MethodInvoker)delegate { listBox1.Items.Add(message); });
                            }
                        }
                    }
                    //Получение сообщения от другого пользователя
                    else
                    {
                        string[] msgs = msg.Split('^');
                        string sender = msgs[0];
                        string receiver = msgs[1];
                        if (sender == currentChat)
                        {
                            string message = msgs[2];
                            string date = msgs[3];
                            listBox1.Invoke((MethodInvoker)delegate
                            {
                                if (listBox1.Items.Count == 1 && listBox1.Items[0].ToString() == "No messages")
                                {
                                    listBox1.Items.Clear();
                                }
                                if (sender == "General Chat")
                                {
                                    listBox1.Items.Add($"{receiver}: {message} ({date})");
                                }
                                else
                                    listBox1.Items.Add($"{sender}: {message} ({date})");
                            });
                        }
                        else
                        {
                            int n = 1;
                            int i = -1;
                            string user = "";
                            foreach (var item in listBox2.Items)
                            {
                                user = item.ToString();
                                if (user.ToString().Contains("("))
                                {
                                    char[] number = new char[20];
                                    user.CopyTo(user.LastIndexOf('(') + 1, number, 0, user.LastIndexOf(')') - user.LastIndexOf('(') - 1);
                                    string str = new string(number);
                                    n = int.Parse(str);
                                    n++;
                                    user = user.Remove(user.LastIndexOf('('));
                                }
                                if (user == sender)
                                {
                                    user += "(" + n + ")";
                                    i = listBox2.Items.IndexOf(item);
                                    break;
                                }
                            }
                            if (i != -1)
                            {
                                listBox2.Items.Insert(i, user);
                                listBox2.Items.RemoveAt(i + 1);
                            }
                        }
                    }
                } while (true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Register()
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

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (s.Connected)
            {
                s.Send(System.Text.Encoding.Unicode.GetBytes($"{currentChat}^{messageContent.Text}^{DateTime.Now.Ticks.ToString()}"));
                if (listBox1.Items.Count == 1 && listBox1.Items[0].ToString() == "No messages")
                {
                    listBox1.Items.Clear();
                }
                listBox1.Items.Add($"{this.Text}: {messageContent.Text} ({DateTime.Now.ToString()})");
            }
        }

        private void messageContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && Control.ModifierKeys == Keys.Control)
            {
                e.Handled = true;
                SendButton_Click(null, null);
                messageContent.Text = "";
            }
        }
    }
}