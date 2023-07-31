using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using LithiumSpammer.Utilities;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leaf.xNet;
using LithiumSpammer.raidlib;
using System.Threading;
using System.Web.Profile;
using static LithiumSpammer.raidlib.Client;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Web.UI;
using Guna.UI2.WinForms;
using Newtonsoft.Json.Linq;

namespace LithiumSpammer
{
    public partial class Spammer : Form
    {
        public string[] tokens = new string[0];
		Client[] clients = new Client[0];
		public string[] socks5proxies = new string[0];
        public string[] socks4proxies = new string[0];
        public string[] httpproxies = new string[0];
        Token_Generator TokenGenerator = new Token_Generator();
        Token_Checker TokenChecker = new Token_Checker();

        public Spammer()
        {
            InitializeComponent();
            proxytype_dd.SelectedIndex = 0;
			guna2ComboBox1.SelectedIndex = 0;
			if (!File.Exists("tokens.txt"))
			{
				File.Create("tokens.txt");
			}
			else
			{
				string text = File.ReadAllText("tokens.txt");
				tokens = text.Split('\n');
                if (tokens.Length == 1)
                {
                    if (tokens[0] == "")
                    {
                        tokens = new string[0];
                    }
                }
				tokens_label.Text = "Tokens: " + tokens.Length;
				updateClients();
			}
			if (!File.Exists("accounts.txt"))
			{
				File.Create("accounts.txt");
			}
			if (!File.Exists("socks5.txt"))
			{
				File.Create("socks5.txt");
			}
			else
			{
				string[] array = (socks5proxies = File.ReadAllLines("socks5.txt"));
				socks5_label.Text = "Socks5: " + array.Length;
			}
			if (!File.Exists("socks4.txt"))
			{
				File.Create("socks4.txt");
			}
			else
			{
				string[] array = (socks4proxies = File.ReadAllLines("socks4.txt"));
				socks4_label.Text = "Socks4: " + array.Length;
			}
			if (!File.Exists("http.txt"))
			{
				File.Create("http.txt");
			}
			else
			{
				string[] array = (httpproxies = File.ReadAllLines("http.txt"));
				http_label.Text = "Http: " + array.Length;
			}
		}

        public void updateClients()
        {
            clients = new Client[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                clients[i] = new Client(tokens[i]);
                clients[i].GotUID += Client_GotUID;
            }
        }


        private void Client_GotUID(object sender, object profile)
        {
            string profileJson = profile.ToString();
            var profileData = JsonConvert.DeserializeObject<ProfileData>(profileJson);

            clientsDataGridView.Invoke((MethodInvoker)delegate
            {
                clientsDataGridView.Rows.Add(profileData?.user?.Id, profileData?.user?.Name, profileData?.user?.Email);
            });
        }

        private class UserData
        {
            public string Id { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
        }

        private class ProfileData
        {
            public UserData user { get; set; }
        }

        public ProxyClient getProxy()
		{
			Random random = new Random();
			if (proxytype_dd.SelectedText == "Socks5")
			{
				return Socks5ProxyClient.Parse(socks5proxies[random.Next(0, socks5proxies.Length - 1)]);
			}
			if (proxytype_dd.SelectedText == "Socks4")
			{
				return Socks4ProxyClient.Parse(socks4proxies[random.Next(0, socks4proxies.Length - 1)]);
			}
			return HttpProxyClient.Parse(httpproxies[random.Next(0, httpproxies.Length - 1)]);
		}

		public string getProxyType()
		{
			return proxytype_dd.SelectedText;
		}

		private void exit_btn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void minimalize_btn_Click(object sender, EventArgs e)
        {
            base.WindowState = FormWindowState.Minimized;
        }

        private void cleartokens_btn_Click(object sender, EventArgs e)
        {
			File.WriteAllText("tokens.txt", "");
			Array.Clear(tokens, 0, tokens.Length);
			tokens_label.Text = "Tokens: 0";
			updateClients();
		}

        private void clearproxies_btn_Click(object sender, EventArgs e)
        {
			File.WriteAllText("http.txt", "");
			File.WriteAllText("socks4.txt", "");
			File.WriteAllText("socks5.txt", "");
			Array.Clear(socks4proxies, 0, socks4proxies.Length);
			Array.Clear(socks5proxies, 0, socks5proxies.Length);
			Array.Clear(httpproxies, 0, httpproxies.Length);
			socks4_label.Text = "Socks4: 0";
			socks5_label.Text = "Socks5: 0";
			http_label.Text = "Http: 0";
		}

		public void scrapproxies_btn_Click(object sender, EventArgs e)
		{
			WebClient webClient = new WebClient();
			string text = webClient.DownloadString("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt");
			string text2 = webClient.DownloadString("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt");
			string text3 = webClient.DownloadString("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt");
			socks4_label.Text = "Socks4: " + text2.Length;
			socks5_label.Text = "Socks5: " + text3.Length;
			http_label.Text = "Http: " + text.Length;
			File.WriteAllText("http.txt", text);
			File.WriteAllText("socks4.txt", text2);
			File.WriteAllText("socks5.txt", text3);
        }

        private void UpdateTokensLabel(string text)
        {
            if (tokens_label.InvokeRequired)
            {
                tokens_label.BeginInvoke(new Action<string>(UpdateTokensLabel), text);
            }
            else
            {
                tokens_label.Text = text;
            }
        }

        private void UpdateGeneratedTokensMtb(string text)
        {
            if (generatedtokens_mtb.InvokeRequired)
            {
                generatedtokens_mtb.BeginInvoke(new Action<string>(UpdateGeneratedTokensMtb), text);
            }
            else
            {
                generatedtokens_mtb.Text += text + "\n";
            }
        }

        private async void generate_btn_ClickAsync(object sender, EventArgs e)
		{
			string element = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			Random random = new Random();
			int waitMS = guna2TrackBar1.Value;
			await Task.Run(() =>
			{
				Parallel.For(0, (int)amountgen_numud.Value, i =>
				{
					string username = usernamegen_tb.Text;
                    if (randomusername_cb.Checked)
                    {
                        username = new string((from s in Enumerable.Repeat(element, decimal.ToInt32(randlength_numud.Value))
						select s[random.Next(s.Length)]).ToArray());
                    }
					string password = new string((from s in Enumerable.Repeat(element, 12)
												  select s[random.Next(s.Length)]).ToArray());
					string email = new string((from s in Enumerable.Repeat(element, decimal.ToInt32(randlength_numud.Value))
											   select s[random.Next(s.Length)]).ToArray()) + "@gmail.com";
					string token = TokenGenerator.GenerateToken(username, password, email, autojoingen_cb.Checked, invitecodegen_tb.Text);

					Invoke((MethodInvoker)delegate
                    {
                        if (token != "" && !token.StartsWith("{"))
                        {
                            tokens.Append(token);

                            UpdateTokensLabel("Tokens: " + (tokens.Length + 1));
                            UpdateGeneratedTokensMtb(token);

                            File.AppendAllText("tokens.txt", token + "\n");
                            File.AppendAllText("accounts.txt", email + ":" + password + "\n");
                            updateClients();
                        }
                        else
                        {
                            UpdateGeneratedTokensMtb(token);
                        }
                    });
				});
			});
		}

        private async void create_btn_Click(object sender, EventArgs e)
		{
			await Task.Run(() =>
			{
				string username = guna2TextBox3.Text;
				string password = guna2TextBox4.Text;
				string email = guna2TextBox5.Text;
				string element = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
				Random random = new Random();
                if (randusername_cb.Checked)
                {
                    username = new string((from s in Enumerable.Repeat(element, decimal.ToInt32(randlength_numud.Value))
                    select s[random.Next(s.Length)]).ToArray());
                }
                if (randpswd_cb.Checked)
                {
                    password = new string((from s in Enumerable.Repeat(element, 12)
                    select s[random.Next(s.Length)]).ToArray());
                }
                if (randemail_cb.Checked)
				{
					email = new string((from s in Enumerable.Repeat(element, decimal.ToInt32(randlength_numud.Value))
					select s[random.Next(s.Length)]).ToArray()) + "@gmail.com";
				}
				string token = TokenGenerator.GenerateToken(username, password, email, autojoincreate_cb.Checked, invitecodecreate_tb.Text);

                if (token != "" && !token.StartsWith("{"))
                {
                    tokens.Append(token);

                    UpdateTokensLabel("Tokens: " + (tokens.Length + 1));
                    UpdateGeneratedTokensMtb(token);

                    File.AppendAllText("tokens.txt", token + "\n");
                    File.AppendAllText("accounts.txt", email + ":" + password + "\n");
                    updateClients();
                }
                else
                {
                    UpdateGeneratedTokensMtb(token);
                }
            });
		}

		private void loadtokens_btn_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Text files (*.txt)|*.txt";
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] lines = File.ReadAllLines(openFileDialog.FileName);
				tokens = lines;
				tokens_label.Text = "Tokens: " + tokens.Length;
				generatedtokens_mtb.Lines = tokens;
			}
			updateClients();
		}
		
		private void topmost_cb_CheckedChanged(object sender, EventArgs e)
		{
			base.TopMost = topmost_cb.Checked;
		}

		//private async void friend_btn_Click(object sender, EventArgs e)
		//{
		//	foreach (string token in tokens)
		//	{
		//		await Task.Run(() =>
		//		{
		//			try
		//			{
		//				Client bot = new Client(token);
		//				bot.addfriend(guna2TextBox1.Text);
		//			}
		//			catch (Exception ex)
		//			{
		//			}
		//		});
		//	}
		//}

		//private async void unfriend_btn_Click(object sender, EventArgs e)
		//{
		//	foreach (string token in tokens)
		//	{
		//		await Task.Run(() =>
		//		{
		//			try
		//			{
		//				Client bot = new Client(token);
		//				bot.unfriend(guna2TextBox1.Text);
		//			}
		//			catch (Exception ex)
		//			{
		//			}
		//		});
		//	}
		//}

		private async void friend_btn_Click(object sender, EventArgs e)
		{
			List<Task> tasks = new List<Task>();

			foreach (Client bot in clients)
			{
				Task task = Task.Run(() =>
				{
					bot.addFriend(guna2TextBox1.Text);
				});

				tasks.Add(task);
			}

			await Task.WhenAll(tasks);
		}

		private async void unfriend_btn_Click(object sender, EventArgs e)
		{
			List<Task> tasks = new List<Task>();

			foreach (Client bot in clients)
			{
				Task task = Task.Run(() =>
				{
					bot.unFriend(guna2TextBox1.Text);
				});

				tasks.Add(task);
			}

			await Task.WhenAll(tasks);
		}

        private async void guna2Button2_ClickAsync(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                Task task = Task.Run(() =>
                {
                    bot.joinGuild(guna2TextBox2.Text);
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
			List<Task> tasks = new List<Task>();

			foreach (Client bot in clients)
			{
				Task task = Task.Run(() =>
				{
					bot.leaveGuild(guna2TextBox14.Text);
				});

				tasks.Add(task);
			}

			await Task.WhenAll(tasks);
        }

        private async void guna2Button6_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                Task task = Task.Run(() =>
                {
                    bot.clearStatus();
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button7_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();
            string element = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            for (int i = 0; i < clients.Length; i++)
            {
                Client bot = clients[i];
                string username = guna2TextBox8.Text;
                if (guna2CheckBox1.Checked)
                {
                    username = new string((from s in Enumerable.Repeat(element, decimal.ToInt32(guna2NumericUpDown1.Value))
                    select s[random.Next(s.Length)]).ToArray());
                }

                Task task = Task.Run(() =>
                {
                    bot.setUsername(username);
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button3_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                string statusText = guna2TextBox6.Text;
                int emoji = int.Parse(guna2TextBox6.Text);
                if (guna2TextBox6.Text == "") emoji = 0;

                if (statusText != null)
                {
                    Task task = Task.Run(() =>
                    {
                        bot.setStatus(statusText, emoji);
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button12_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                string sid = guna2TextBox14.Text;

                if (sid != null)
                {
                    Task task = Task.Run(() =>
                    {
                        bot.joinVC(sid);
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button13_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                string sid = guna2TextBox14.Text;

                if (sid != null)
                {
                    Task task = Task.Run(() =>
                    {
                        bot.leaveVC(sid);
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button10_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                string sid = guna2TextBox11.Text;
                string mid = guna2TextBox12.Text;
                string rid = guna2TextBox13.Text;

                if (sid != null && mid != null && rid != null)
                {
                    Task task = Task.Run(() =>
                    {
                        bot.addReaction(sid, mid, rid);
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button11_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
                string sid = guna2TextBox11.Text;
                string mid = guna2TextBox12.Text;
                string rid = guna2TextBox13.Text;

                if (sid != null && mid != null && rid != null)
                {
                    Task task = Task.Run(() =>
                    {
                        bot.removeReaction(sid, mid, rid);
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async void guna2Button4_Click(object sender, EventArgs e)
        {
            List<Task> tasks = new List<Task>();

            foreach (Client bot in clients)
            {
				string presence = guna2ComboBox1.Text;

                Task task = Task.Run(() =>
                {
                    bot.setPresence(presence);
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
        private bool stopSendingMessages = false;
        private async void guna2Button9_Click(object sender, EventArgs e)
        {
            stopSendingMessages = false;
            guna2Button9.Enabled = false;
            guna2Button8.Enabled = true;

            while (!stopSendingMessages)
            {
                double WaitMS = guna2TrackBar2.Value;
                List<Task> tasks = new List<Task>();

                foreach (Client bot in clients)
                {
                    string text = guna2TextBox10.Text;
                    string cid = guna2TextBox9.Text;
                    double time = guna2TrackBar2.Value;

                    Task task = Task.Run(() =>
                    {
                        bot.sendMessage(text, cid);
                    });

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                await Task.Delay(TimeSpan.FromMilliseconds(WaitMS));
            }

            guna2Button9.Enabled = true;
            guna2Button8.Enabled = false;
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            stopSendingMessages = true;
        }

        private void guna2TrackBar1_ValueChanged(object sender, EventArgs e)
        {
			label8.Text = guna2TrackBar1.Value.ToString();
		}

        private void guna2Button5_Click(object sender, EventArgs e)
        {
			if (!File.Exists("tokens.txt"))
			{
				File.Create("tokens.txt");
			}
			else
			{
				string text = File.ReadAllText("tokens.txt");
				string[] array = text.Split('\n');
				tokens = array;
				tokens_label.Text = "Tokens: " + array.Length;
				updateClients();
			}
		}

        private void guna2TrackBar2_ValueChanged(object sender, EventArgs e)
        {
            label11.Text = guna2TrackBar2.Value.ToString();
        }

        private void friender_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines("accounts.txt");

            foreach (string line in lines)
            {
                string[] accountParts = line.Split(':');
                string email = accountParts[0].Trim();
                string password = accountParts[1].Trim();

                bool isValid = TokenChecker.isValidAccount(email, password);

                Console.WriteLine(isValid);

                if (isValid)
                {
                    string token = TokenChecker.getAccountToken(email, password);

                    Console.WriteLine(token);

                    Array.Resize(ref tokens, tokens.Length + 1);
                    tokens[tokens.Length - 1] = token;
                    UpdateTokensLabel("Tokens: " + (tokens.Length + 1));

                    File.AppendAllText("tokens.txt", token + "\n");
                }
            }
        }
    }
}