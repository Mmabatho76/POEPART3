using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POEPART3
{
    public partial class Form1 : Form
    {
       

        private ChatbotEngine bot;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblName;

        private TextBox txtName;
        private TextBox txtInput;

        private Button btnStart;
        private Button btnSend;

        private RichTextBox rtbChat;

        public Form1()
        {
            InitializeComponent();

            //Builds all GUI components dynamically 
            BuildInterface();
        }

        private void BuildInterface()
        {
            Text = "AZEEBOT";

            //Window size
            Size = new Size(980, 720);

            //Opens form in centre of screen
            StartPosition = FormStartPosition.CenterScreen;

            BackColor = Color.FromArgb(220, 240, 255);

            FormBorderStyle = FormBorderStyle.Sizable;

            MaximizeBox = true;

            // Main title
            lblTitle = new Label();

            lblTitle.Text = "AZEEBOT";

            lblTitle.ForeColor = Color.FromArgb(30, 90, 140);

            lblTitle.Font = new Font("Trebuchet MS", 30, FontStyle.Bold);

            lblTitle.Location = new Point(35, 20);

            lblTitle.AutoSize = true;

            Controls.Add(lblTitle);

            // Small description under title
            lblSubtitle = new Label();

            lblSubtitle.Text = "Cybersecurity Awareness Assistant";

            lblSubtitle.ForeColor = Color.FromArgb(70, 120, 170);

            lblSubtitle.Font = new Font("Trebuchet MS", 11);

            lblSubtitle.Location = new Point(40, 76);

            lblSubtitle.AutoSize = true;

            Controls.Add(lblSubtitle);

            // Name label
            lblName = new Label();

            lblName.Text = "USER";

            lblName.ForeColor = Color.FromArgb(40, 90, 140);

            lblName.Font = new Font("Trebuchet MS", 10, FontStyle.Bold);

            lblName.Location = new Point(40, 118);

            lblName.AutoSize = true;

            Controls.Add(lblName);

            // Name textbox
            txtName = new TextBox();

            txtName.Location = new Point(105, 114);

            txtName.Size = new Size(300, 35);

            txtName.Font = new Font("Trebuchet MS", 11);

            txtName.BackColor = Color.White;

            txtName.ForeColor = Color.Gray;

            txtName.BorderStyle = BorderStyle.FixedSingle;

            //Placeholder text
            txtName.Text = "Enter your name...";

            //Removes placeholder text when textbox is clicked 
            txtName.Enter += (s, e) =>
            {
                if (txtName.Text == "Enter your name...")
                {
                    txtName.Text = "";

                    txtName.ForeColor = Color.Black;
                }
            };

            //Restores placeholder if textbox is empty
            txtName.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    txtName.Text = "Enter your name...";

                    txtName.ForeColor = Color.Gray;
                }
            };

            Controls.Add(txtName);

            // Start button
            btnStart = new Button();

            btnStart.Text = "BEGIN";

            btnStart.Location = new Point(425, 111);

            btnStart.Size = new Size(145, 42);

            btnStart.BackColor = Color.FromArgb(130, 190, 240);

            btnStart.ForeColor = Color.White;

            btnStart.FlatStyle = FlatStyle.Flat;

            btnStart.FlatAppearance.BorderSize = 0;

            btnStart.Font = new Font("Trebuchet MS", 10, FontStyle.Bold);

            btnStart.Cursor = Cursors.Hand;

            btnStart.Click += BtnStart_Click;

            Controls.Add(btnStart);

            // Chat display area
            rtbChat = new RichTextBox();

            rtbChat.Location = new Point(40, 185);

            rtbChat.Size = new Size(870, 395);

            //prevents user from editing chat
            rtbChat.ReadOnly = true;

            rtbChat.BackColor = Color.FromArgb(245, 250, 255);

            rtbChat.ForeColor = Color.FromArgb(40, 70, 110);

            rtbChat.BorderStyle = BorderStyle.FixedSingle;

            // Use a monospace font and a clean chatbot apperance 
            // and fixed-width content keep their alignment.
            rtbChat.Font = new Font("Consolas", 10);
            rtbChat.WordWrap = true;
            rtbChat.ScrollBars = RichTextBoxScrollBars.Vertical;

            Controls.Add(rtbChat);

            rtbChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // User input field
            txtInput = new TextBox();

            txtInput.Location = new Point(40, 610);

            txtInput.Size = new Size(710, 40);

            txtInput.Font = new Font("Trebuchet MS", 11);

            txtInput.BackColor = Color.White;

            txtInput.ForeColor = Color.Black;

            txtInput.BorderStyle = BorderStyle.FixedSingle;

            txtInput.Enabled = false;

            txtInput.KeyDown += TxtInput_KeyDown;

            Controls.Add(txtInput);

            txtInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Send button
            btnSend = new Button();

            btnSend.Text = "SEND";

            btnSend.Location = new Point(770, 606);

            btnSend.Size = new Size(140, 42);

            btnSend.BackColor = Color.FromArgb(130, 190, 240);

            btnSend.ForeColor = Color.White;

            btnSend.FlatStyle = FlatStyle.Flat;

            btnSend.FlatAppearance.BorderSize = 0;

            btnSend.Font = new Font("Trebuchet MS", 10, FontStyle.Bold);

            btnSend.Cursor = Cursors.Hand;

            btnSend.Enabled = false;

            btnSend.Click += BtnSend_Click;

            Controls.Add(btnSend);

            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();

            // Prevents placeholder text from becoming username
            if (string.IsNullOrWhiteSpace(name) || name == "Enter your name...")
            {
                name = "Friend";
            }

            bot = new ChatbotEngine(name);

            rtbChat.Clear();

            AddBotMessage(bot.GetAsciiArt());

            AddBotMessage("Connection established successfully.");

            AddBotMessage("Welcome aboard, " + name + ".");

            AddBotMessage("You can ask about phishing, passwords, online scams, privacy, or safer browsing habits.");

            AddBotMessage("Try typing things like: 'I am curious about privacy' or 'tell me more about scams'.");

            AudioPlayer.PlayGreeting("greeting.wav");

            txtInput.Enabled = true;

            btnSend.Enabled = true;

            txtInput.Focus();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            ProcessUserInput();
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            // Sends message when Enter is pressed
            if (e.KeyCode == Keys.Enter)
            {
                ProcessUserInput();

                e.SuppressKeyPress = true;
            }
        }

        private void ProcessUserInput()
        {
            if (bot == null)
            {
                MessageBox.Show("Launch the assistant first.");

                return;
            }

            string userInput = txtInput.Text.Trim();

            // Stops blank messages
            if (string.IsNullOrWhiteSpace(userInput))
            {
                AddBotMessage("Please enter a message.");

                return;
            }

            AddUserMessage(userInput);

            //Exit check
            string lowerInput = userInput.ToLower();

            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "goodbye" || lowerInput == "bye")
            {
                AddBotMessage("Chat session has ended.");

                txtInput.Enabled = false;
                btnSend.Enabled = false;

                txtInput.Clear();
                return; //stop further processing

            }

            string response = bot.GetResponse(userInput);
            AddBotMessage(response);


            txtInput.Clear();
            txtInput.Focus();
        }
        private void AddUserMessage(string message)
        {
            rtbChat.SelectionColor = Color.FromArgb(50, 100, 160);

            rtbChat.AppendText("YOU > "
                               + message
                               + Environment.NewLine
                               + Environment.NewLine);

            rtbChat.SelectionColor = Color.FromArgb(40, 70, 110);
        }

        private void AddBotMessage(string message)
        {
            rtbChat.SelectionColor = Color.FromArgb(40, 70, 110);

            rtbChat.AppendText("AZEEBOT > "
                               + message
                               + Environment.NewLine
                               + Environment.NewLine);

            rtbChat.SelectionColor = Color.FromArgb(40, 70, 110);

            rtbChat.ScrollToCaret();
        }

    }
}
