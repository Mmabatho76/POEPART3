
using POEPART3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace PROG6221POE
{
    public delegate string BotResponseDelegate(string input);

    public class ChatbotEngine
    {
        private string userName;
        private string favouriteTopic = "";
        private string activeTopic = "";
        private string recentFeeling = "";

        private readonly Random random = new Random();

        // State tracking
        private bool awaitingTaskDescription = false;
        private string pendingTaskTitle = "";
        private bool awaitingReminder = false;
        private bool awaitingReminderDate = false;

        private bool quizActive = false;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private List<QuizQuestion> quizQuestions;

        // Activity Log
        private readonly List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();

        // --- Existing Dictionaries ---
        private readonly Dictionary<string, string> generalResponses;
        private readonly Dictionary<string, string> phishingResponses;
        private readonly Dictionary<string, string> passwordResponses;
        private readonly Dictionary<string, string> safeBrowsingResponses;
        private readonly Dictionary<string, List<string>> quickTopicResponses;
        private readonly Dictionary<string, List<string>> detailedTopicResponses;
        private readonly Dictionary<string, List<string>> feelingResponses;
        private readonly Dictionary<string, List<string>> topicKeywords;

        public ChatbotEngine(string userName)
        {
            this.userName = string.IsNullOrWhiteSpace(userName) ? "Friend" : userName;
            DatabaseHelper.InitializeDatabase();

            // --- Initialize all existing response dictionaries here ---
            // (keep exactly the same code from your original constructor)

            generalResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "how are you", $"I am doing well, {this.userName}. I am ready to help you stay safer online." },
                { "what is your purpose", $"My purpose is to guide you through cybersecurity basics in a clear and practical way, {this.userName}." },
                { "what can i ask you about", "You can ask about phishing, password safety, safe browsing, online scams, privacy, suspicious links, public Wi-Fi, two-factor authentication, add tasks, set reminders, or play the quiz." },
                { "who created you", "I was created as part of a cybersecurity awareness chatbot project." },
                { "why is cybersecurity important", "Cybersecurity is important because it protects your identity, accounts, money, private information, and devices from online threats." },
                { "hello", $"Hello {this.userName}. What online safety topic would you like to explore?" },
                { "hi", $"Hi {this.userName}. You can ask me about phishing, passwords, scams, privacy, safe browsing, 2FA, tasks, reminders, or quiz." },
                { "help", "Try: 'What is phishing?', 'Add task', 'Show my tasks', 'Start quiz', 'Show activity log'." }
            };

            phishingResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "what is phishing", "Phishing is when criminals pretend to be trusted people or organisations to trick you into sharing private information." },
                { "how to spot phishing email", "Look for strange sender addresses, urgent wording, spelling errors, suspicious links, unexpected attachments, and requests for passwords or OTPs." },
                { "what to do if i clicked a phishing link", "Change any password you entered, enable 2FA, scan your device, monitor your account, and avoid entering more information on that page." },
                { "examples of phishing", "Examples include fake banking emails, fake parcel delivery messages, fake login pages, fake prize notifications, and fake support alerts." },
                { "what is smishing", "Smishing is phishing through SMS. It often uses fake delivery links, bank warnings, or account verification messages." },
                { "what is vishing", "Vishing is phishing through phone calls. Scammers may pretend to be from a bank, company, or government office." },
                { "how to report phishing", "Report phishing to the organisation being impersonated, your bank if money is involved, or a cybercrime reporting channel." },
                { "what are phishing red flags", "Red flags include pressure, threats, bad grammar, mismatched links, fake login pages, and messages asking for sensitive information." }
            };

            passwordResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "how to create strong password", "Create a password that is long, unique, and hard to guess. Use letters, numbers, symbols, and avoid personal details." },
                { "what is two factor authentication", "Two-factor authentication adds another check after your password, such as an app code, SMS code, or security prompt." },
                { "how often to change passwords", "Change passwords after a breach, suspicious activity, or when you have reused the password somewhere else." },
                { "what is password manager", "A password manager stores and generates strong passwords so you do not have to remember every password yourself." },
                { "should i reuse passwords", "No. Reusing passwords is risky because one leaked password can unlock several accounts." },
                { "how to remember strong passwords", "Use a password manager or create a long passphrase that is easy for you to remember but difficult for others to guess." },
                { "what is multi factor authentication", "Multi-factor authentication uses more than one proof of identity, such as a password plus a phone, fingerprint, or security key." },
                { "common password mistakes", "Common mistakes include using names, birthdays, password123, qwerty, short passwords, shared passwords, and sticky notes." }
            };

            safeBrowsingResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "how to identify safe websites", "Check for HTTPS, correct spelling in the website address, a proper layout, and avoid pages filled with pop-ups or fake buttons." },
                { "what is https", "HTTPS helps protect data sent between your browser and a website by encrypting the connection." },
                { "how to avoid fake websites", "Type important web addresses manually, use bookmarks, check the domain, and avoid unrealistic adverts or suspicious links." },
                { "what are cookies safe", "Cookies are often normal, but tracking cookies can follow your activity. Review browser settings and clear unwanted cookies." },
                { "how to browse safely on public wifi", "Avoid banking or sensitive logins on public Wi-Fi unless you use protection such as a VPN and secure websites." },
                { "what is incognito mode", "Incognito mode hides local browsing history on your device, but it does not make you invisible online." },
                { "how to check if link is safe", "Hover over the link, inspect the full address, avoid strange shortened links, and use link scanners if unsure." },
                { "what is browser security", "Browser security includes updating your browser, blocking pop-ups, avoiding unsafe downloads, and limiting suspicious extensions." }
            };

            quickTopicResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", new List<string> { phishingResponses["what is phishing"], phishingResponses["how to spot phishing email"], phishingResponses["what are phishing red flags"] } },
                { "password", new List<string> { passwordResponses["how to create strong password"], passwordResponses["what is password manager"], passwordResponses["should i reuse passwords"] } },
                { "safe browsing", new List<string> { safeBrowsingResponses["how to identify safe websites"], safeBrowsingResponses["how to avoid fake websites"], safeBrowsingResponses["how to browse safely on public wifi"] } },
                { "privacy", new List<string> { "Privacy is about deciding what personal information you share and who can access it.", "Check your app permissions, location sharing, and social media visibility often.", "Avoid sharing your ID number, home address, live location, daily routine, or personal documents online." } },
                { "scam", new List<string> { "Scams often use fear, pressure, fake prizes, or fake authority to rush you into a bad decision.", "Never share OTPs, banking information, passwords, or personal documents with strangers online.", "Verify suspicious offers using official websites, trusted numbers, or direct company channels." } },
                { "2fa", new List<string> { passwordResponses["what is two factor authentication"], "2FA helps protect your account even if someone steals your password.", "Authenticator apps are usually safer than SMS codes because SMS can be affected by SIM-swap fraud." } }
            };

            detailedTopicResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", new List<string> { "A good phishing defence is to pause before reacting. If a message creates panic or urgency, verify through the official website instead of clicking the link.", "Phishing websites may look almost real. Look closely at the domain name, spelling, layout, and whether the page asks for unnecessary information.", "If you entered information on a suspicious site, change the affected password, enable 2FA, and watch the account for unusual activity." } },
                { "password", new List<string> { "A long passphrase can be easier to remember and harder to crack than a short complicated password.", "Your email password is extremely important because many other accounts can be reset through your email.", "Do not store passwords in screenshots, chats, plain notes, or unprotected documents." } },
                { "safe browsing", new List<string> { "Avoid cracked software, unknown downloads, random APKs, and suspicious extensions because they often carry malware.", "Public Wi-Fi is convenient but risky. Avoid private logins on public networks unless you are using secure protection.", "Browser updates matter because they often fix weaknesses that attackers could exploit." } },
                { "privacy", new List<string> { "Privacy is not about hiding everything. It is about controlling what information belongs online and what should stay private.", "Apps may ask for permissions they do not truly need. Review camera, microphone, location, contacts, and file access.", "Attackers can combine small details like birthdays, schools, and routines to guess security questions or impersonate you." } },
                { "scam", new List<string> { "Common scams include fake job offers, fake investments, courier scams, romance scams, prize scams, and fake account verification messages.", "Treat OTPs like passwords. If someone asks for your OTP, they may be trying to access your account.", "Scammers copy real logos and wording, so verify suspicious messages through official channels." } },
                { "2fa", new List<string> { "2FA is strongest when your email is protected too, because email controls password resets for many accounts.", "Backup codes should be stored somewhere safe in case you lose access to your phone.", "SMS 2FA is better than no 2FA, but authenticator apps are usually stronger." } }
            };

            feelingResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "worried", new List<string> { "That worry is understandable. Online threats can feel intimidating, but simple habits can reduce a lot of danger.", "Start with the basics: strong passwords, two-factor authentication, and careful link checking." } },
                { "frustrated", new List<string> { "I get it. Cybersecurity can feel like extra work, but it is easier than recovering a stolen account.", "Let us keep it simple and handle one safety habit at a time." } },
                { "curious", new List<string> { "That curiosity is useful. The more you understand cyber tricks, the harder you are to fool.", "Good mindset. Learning the warning signs makes you much safer online." } },
                { "confused", new List<string> { "No problem. Cybersecurity terms can be confusing at first, but we can break them down clearly.", "Tell me which part is unclear and I will explain it more simply." } }
            };

            topicKeywords = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", new List<string> { "phishing", "fake email", "suspicious email", "email scam", "smishing", "vishing" } },
                { "password", new List<string> { "password", "passwords", "passcode", "credentials", "login details", "strong password" } },
                { "safe browsing", new List<string> { "safe browsing", "browser", "website", "https", "link", "download", "public wifi", "public wi-fi" } },
                { "privacy", new List<string> { "privacy", "private data", "personal info", "personal information", "permissions", "tracking" } },
                { "scam", new List<string> { "scam", "scams", "fraud", "otp", "fake offer", "banking details" } },
                { "2fa", new List<string> { "2fa", "two factor", "two-factor", "mfa", "authenticator", "verification code" } }
            };

            InitializeQuizQuestions();
        }

        // --- Main Response Logic ---
        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Please type something first.";
            input = input.ToLower().Trim();

            // Handle ongoing states first
            if (quizActive) return HandleQuizInput(input);
            if (awaitingTaskDescription) return HandleTaskDescription(input);
            if (awaitingReminder) return HandleReminderChoice(input);
            if (awaitingReminderDate) return HandleReminderDate(input);

            // Exit commands
            if (input == "exit" || input == "quit" || input == "goodbye" || input == "bye")
            {
                LogActivity("Session ended");
                return $"Goodbye {userName}. Stay safe online!";
            }

            // --- NLP & Intent Detection ---
            if (MatchesAny(input, new[] { "show activity log", "what have you done", "history" }))
                return GetActivityLog();

            if (MatchesAny(input, new[] { "start quiz", "begin quiz", "play quiz", "test my knowledge" }))
                return StartQuiz();

            if (MatchesAny(input, new[] { "add task", "new task", "create task", "remind me to" }))
                return StartNewTask(input);

            if (MatchesAny(input, new[] { "show tasks", "list tasks", "view my tasks" }))
                return ListAllTasks();

            if (MatchesAny(input, new[] { "mark task complete", "finish task", "done task" }))
                return "Type the task number or title to mark it complete.";

            if (MatchesAny(input, new[] { "delete task", "remove task" }))
                return "Type the task number or title to delete it.";

            // Existing logic
            string feelingReply = DetectFeeling(input);
            string memoryReply = HandleMemory(input);
            if (!string.IsNullOrWhiteSpace(memoryReply)) return memoryReply;

            if (IsFollowUp(input)) return ContinueTopic();

            string partOneReply = DetectPartOneResponse(input);
            string topicReply = DetectTopic(input);

            if (!string.IsNullOrWhiteSpace(feelingReply) && !string.IsNullOrWhiteSpace(topicReply))
                return feelingReply + Environment.NewLine + Environment.NewLine + topicReply;
            if (!string.IsNullOrWhiteSpace(partOneReply)) return partOneReply;
            if (!string.IsNullOrWhiteSpace(feelingReply)) return feelingReply;
            if (!string.IsNullOrWhiteSpace(topicReply)) return topicReply;

            if (!string.IsNullOrWhiteSpace(favouriteTopic))
                return $"I didn't fully understand that, but I remember you like {favouriteTopic}. Ask me more about it.";

            return "I’m not sure what you mean. Try asking about cybersecurity, tasks, reminders, or type 'help'.";
        }

        // --- Helper Methods ---
        private bool MatchesAny(string input, string[] patterns)
        {
            return patterns.Any(p => Regex.IsMatch(input, $@"\b{Regex.Escape(p)}\b", RegexOptions.IgnoreCase));
        }

        private void LogActivity(string action)
        {
            activityLog.Insert(0, new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Action = action
            });
            if (activityLog.Count > 10) activityLog.RemoveAt(activityLog.Count - 1);
        }

        private string GetActivityLog()
        {
            if (!activityLog.Any()) return "No activity recorded yet.";
            var lines = activityLog.Select((e, i) => $"{i + 1}. [{e.Timestamp:HH:mm}] {e.Action}");
            return "📋 Recent Activity:\n" + string.Join("\n", lines);
        }

        // --- Task Assistant Logic ---
        private string StartNewTask(string input)
        {
            pendingTaskTitle = Regex.Replace(input, @"add task|new task|remind me to", "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(pendingTaskTitle))
            {
                pendingTaskTitle = "";
                return "What is the task you want to add?";
            }
            awaitingTaskDescription = true;
            return $"Task title: '{pendingTaskTitle}'. Please enter a short description for this task.";
        }

        private string HandleTaskDescription(string input)
        {
            awaitingTaskDescription = false;
            DatabaseHelper.AddTask(pendingTaskTitle, input);
            LogActivity($"Task added: {pendingTaskTitle}");
            awaitingReminder = true;
            return $"✅ Task added: '{pendingTaskTitle}'. Would you like to set a reminder? (Yes/No)";
        }

        private string HandleReminderChoice(string input)
        {
            awaitingReminder = false;
            if (input.StartsWith("y"))
            {
                awaitingReminderDate = true;
                return "When would you like the reminder? (e.g., 'in 3 days', '2026-07-10', 'tomorrow')";
            }
            return "No reminder set. You can view your tasks anytime by typing 'show tasks'.";
        }

        private string HandleReminderDate(string input)
        {
            awaitingReminderDate = false;
            string dateStr = ParseReminderDate(input);
            DatabaseHelper.AddTask(pendingTaskTitle, "", dateStr);
            LogActivity($"Reminder set for '{pendingTaskTitle}' on {dateStr}");
            return $"⏰ Reminder set for '{pendingTaskTitle}' on {dateStr}.";
        }

        private string ParseReminderDate(string input)
        {
            if (input.Contains("tomorrow")) return DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            if (Regex.IsMatch(input, @"in (\d+) days", RegexOptions.IgnoreCase))
            {
                int days = int.Parse(Regex.Match(input, @"(\d+)").Value);
                return DateTime.Now.AddDays(days).ToString("yyyy-MM-dd");
            }
            return DateTime.TryParse(input, out var dt) ? dt.ToString("yyyy-MM-dd") : DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
        }

        private string ListAllTasks()
        {
            var tasks = DatabaseHelper.GetAllTasks();
            if (!tasks.Any()) return "You have no tasks saved yet.";
            var lines = tasks.Select((t, i) => $"{i + 1}. {t.Title} | Reminder: {t.ReminderDate ?? "None"} | Status: {(t.IsCompleted ? "✅ Done" : "🔄 Pending")}");
            LogActivity("Viewed task list");
            return "📝 Your Tasks:\n" + string.Join("\n", lines);
        }

        // --- Quiz Logic ---
        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "A) Reply with your password", "B) Delete it", "C) Report as phishing", "D) Ignore it" },
                    CorrectAnswer = "C",
                    Explanation = "Reporting helps stop the scammer from tricking others."
                },
                new QuizQuestion
                {
                    QuestionText = "Which password is the strongest?",
                    Options = new List<string> { "A) password123", "B) John1990", "C) K9$pQ2!xZ", "D) qwerty" },
                    CorrectAnswer = "C",
                    Explanation = "Long, mixed letters, numbers and symbols are much harder to guess."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: HTTPS means the connection is encrypted.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswer = "A",
                    Explanation = "HTTPS protects data sent between your browser and the website."
                },
                new QuizQuestion
                {
                    QuestionText = "What is 2FA?",
                    Options = new List<string> { "A) Two Fast Accounts", "B) Two-Factor Authentication", "C) Double Password", "D) Safe File Access" },
                    CorrectAnswer = "B",
                    Explanation = "It adds a second layer of security beyond just your password."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: It is safe to share your OTP with someone who calls you.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswer = "B",
                    Explanation = "OTPs are like temporary passwords — never share them."
                },
                new QuizQuestion
                {
                    QuestionText = "Which is a sign of phishing?",
                    Options = new List<string> { "A) Official company logo", "B) Urgent threats or demands", "C) Correct spelling", "D) Known sender address" },
                    CorrectAnswer = "B",
                    Explanation = "Scammers use urgency to make you act without thinking."
                },
                new QuizQuestion
                {
                    QuestionText = "What is safer on public Wi-Fi?",
                    Options = new List<string> { "A) Online banking", "B) Checking email", "C) Using a VPN", "D) Entering credit card details" },
                    CorrectAnswer = "C",
                    Explanation = "A VPN encrypts your connection so others can’t see your data."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Reusing the same password everywhere is safe.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswer = "B",
                    Explanation = "If one site gets hacked, all your accounts are at risk."
                },
                new QuizQuestion
                {
                    QuestionText = "What does a password manager do?",
                    Options = new List<string> { "A) Stores and generates strong passwords", "B) Removes viruses", "C) Speeds up internet", "D) Blocks ads" },
                    CorrectAnswer = "A",
                    Explanation = "It helps you use unique passwords without remembering all of them."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Incognito mode keeps you invisible to websites.",
                    Options = new List<string> { "A) True", "B) False" },
                    CorrectAnswer = "B",
                    Explanation = "It only hides history on your device — websites still see you."
                }
            };
        }

        private string StartQuiz()
        {
            quizActive = true;
            currentQuestionIndex = 0;
            score = 0;
            LogActivity("Started cybersecurity quiz");
            return "🎮 Let's start the quiz! Answer with A, B, C or D.\n\n" + GetCurrentQuestion();
        }

        private string GetCurrentQuestion()
        {
            var q = quizQuestions[currentQuestionIndex];
            return $"Question {currentQuestionIndex + 1} of {quizQuestions.Count}:\n{q.QuestionText}\n{string.Join("\n", q.Options)}";
        }

        private string HandleQuizInput(string input)
        {
            var q = quizQuestions[currentQuestionIndex];
            if (input.Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
            {
                score++;
                string result = $"✅ Correct! {q.Explanation}";
                currentQuestionIndex++;
                if (currentQuestionIndex >= quizQuestions.Count)
                {
                    quizActive = false;
                    LogActivity($"Completed quiz | Score: {score}/{quizQuestions.Count}");
                    result += $"\n\n🏆 Final Score: {score}/{quizQuestions.Count}\n{GetScoreFeedback()}";
                }
                else
                {
                    result += "\n\n" + GetCurrentQuestion();
                }
                return result;
            }
            else
            {
                return $"❌ Wrong answer. The correct answer is {q.CorrectAnswer}. {q.Explanation}\n\n" + GetCurrentQuestion();
            }
        }

        private string GetScoreFeedback()
        {
            double percent = (double)score / quizQuestions.Count * 100;
            if (percent >= 80) return "Great job! You're a cybersecurity pro!";
            if (percent >= 50) return "Good effort! Keep learning to stay safer.";
            return "Review the topics and try again — safety is a skill!";
        }

        // --- Keep all your original helper methods below ---
        private string DetectPartOneResponse(string input)
        {
            string reply;
            reply = MatchDictionary(input, generalResponses); if (reply != null) return reply;
            reply = MatchDictionary(input, phishingResponses); if (reply != null) { activeTopic = "phishing"; return reply; }
            reply = MatchDictionary(input, passwordResponses); if (reply != null) { activeTopic = "password"; return reply; }
            reply = MatchDictionary(input, safeBrowsingResponses); if (reply != null) { activeTopic = "safe browsing"; return reply; }
            return "";
        }

        private string DetectTopic(string input)
        {
            string topic = FindTopic(input);
            if (string.IsNullOrWhiteSpace(topic)) return "";
            activeTopic = topic;
            if (input.Contains("explain") || input.Contains("details") || input.Contains("deep") || input.Contains("more about"))
                return GetRandomItem(detailedTopicResponses[topic]);
            return GetRandomItem(quickTopicResponses[topic]);
        }

        private string DetectFeeling(string input)
        {
            foreach (var feeling in feelingResponses.Keys)
            {
                if (input.Contains(feeling))
                {
                    recentFeeling = feeling;
                    return GetRandomItem(feelingResponses[feeling]);
                }
            }
            return "";
        }

        private string HandleMemory(string input)
        {
            if (input.Contains("my name is"))
            {
                userName = input.Replace("my name is", "").Trim();
                if (string.IsNullOrWhiteSpace(userName)) userName = "Friend";
                return $"Got it. I'll remember your name as {userName}.";
            }
            if (input.Contains("interested in") || input.Contains("i like") || input.Contains("i care about"))
            {
                string topic = FindTopic(input);
                if (!string.IsNullOrWhiteSpace(topic))
                {
                    favouriteTopic = topic;
                    activeTopic = topic;
                    return $"Thanks for sharing, {userName}! I'll remember you like {favouriteTopic}.";
                }
            }
            if (input.Contains("remember") || input.Contains("what do you know about me"))
            {
                string info = $"I remember your name is {userName}";
                if (!string.IsNullOrWhiteSpace(favouriteTopic)) info += $", you like {favouriteTopic}";
                if (!string.IsNullOrWhiteSpace(recentFeeling)) info += $", and you recently sounded {recentFeeling}";
                return info + ".";
            }
            return "";
        }

        private bool IsFollowUp(string input)
        {
            return input.Contains("tell me more") || input.Contains("another tip") || input.Contains("explain more")
                || input.Contains("go deeper") || input.Contains("more detail") || input.Contains("continue");
        }

        private string ContinueTopic()
        {
            if (string.IsNullOrWhiteSpace(activeTopic))
                return "Choose a topic first: phishing, passwords, scams, privacy, safe browsing, or 2FA.";
            if (detailedTopicResponses.ContainsKey(activeTopic))
                return GetRandomItem(detailedTopicResponses[activeTopic]);
            return "Ask me about phishing, passwords, scams, privacy, safe browsing, or 2FA.";
        }

        private string FindTopic(string input)
        {
            foreach (var group in topicKeywords)
                if (group.Value.Any(k => input.Contains(k))) return group.Key;
            return "";
        }

        private string MatchDictionary(string input, Dictionary<string, string> responses)
        {
            return responses.TryGetValue(input, out var val) ? val : null;
        }

        private string GetRandomItem(List<string> items)
        {
            return items[random.Next(items.Count)];
        }

        public string GetAsciiArt() => AsciiArt.GetAsciiArt();
    }

    // --- Supporting Classes ---
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
    }
}