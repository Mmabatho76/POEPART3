using System;
using System.Drawing;
using System.Windows.Forms;

namespace POEPART3
{
    public partial class Form1 : Form
    {
        private ChatbotEngine bot;
        private Label lblTitle, lblSubtitle, lblName;
        private TextBox txtName, txtInput;
        private Button btnStart, btnSend;
        private RichTextBox rtbChat;

        public Form1()
        {
            InitializeComponent();
            BuildInterface();
        }

        private void BuildInterface()
        {
            // exactly your clean GUI builder
            Text = "AZEEBOT";
            Size = new Size(980, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(220, 240, 255);
            // ... rest exactly like you had
        }

        private void BtnStart_Click(object s, EventArgs e)
        {
            string nm = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(nm) || nm == "Enter your name...") nm = "Friend";
            bot = new ChatbotEngine(nm);
            rtbChat.Clear();
            AddBotMessage(bot.GetAsciiArt());
            AddBotMessage($"Welcome {nm}…");
            AudioPlayer.PlayGreeting("greeting.wav");
            txtInput.Enabled = btnSend.Enabled = true;
        }

        private void ProcessUserInput()
        {
            var txt = txtInput.Text.Trim();
            AddUserMessage(txt);
            AddBotMessage(bot.GetResponse(txt));
            txtInput.Clear();
        }

        private void AddUserMessage(string m)
        {
            rtbChat.SelectionColor = Color.Navy;
            rtbChat.AppendText("YOU > " + m + "\n\n");
        }
        private void AddBotMessage(string m)
        {
            rtbChat.SelectionColor = Color.DarkBlue;
            rtbChat.AppendText("AZEEBOT > " + m + "\n\n");
            rtbChat.ScrollToCaret();
        }
    }
}