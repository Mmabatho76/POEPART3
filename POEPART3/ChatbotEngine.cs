using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEPART3
{
   
        public delegate string BotResponseDelegate(string input);

        public class ChatbotEngine
        {
            // Stores the user's name
            private string userName;
            private string favouriteTopic = "";
            private string activeTopic = "";
            private string recentFeeling = "";

            private readonly Random random = new Random();

            private readonly Dictionary<string, string> generalResponses; // Dictionary for general chatbot responses
            private readonly Dictionary<string, string> phishingResponses; // Dictionary for phishing-related responses
            private readonly Dictionary<string, string> passwordResponses; // Dictionary for password-related responses
            private readonly Dictionary<string, string> safeBrowsingResponses; // Dictionary for safe browsing-related responses

            private readonly Dictionary<string, List<string>> quickTopicResponses; // Stores quick random responses for topics
            private readonly Dictionary<string, List<string>> detailedTopicResponses; // Stores detailed responses for follow-up questions
            private readonly Dictionary<string, List<string>> feelingResponses; // Stores responses based on user's feelings
            private readonly Dictionary<string, List<string>> topicKeywords; // Stores keywords associated with each topic

            public ChatbotEngine(string userName) // Constructor
            {
                // Prevents empty username
                this.userName = string.IsNullOrWhiteSpace(userName) ? "Friend" : userName;

                generalResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "how are you", $"I am doing well, {this.userName}. I am ready to help you stay safer online." },
                { "what is your purpose", $"My purpose is to guide you through cybersecurity basics in a clear and practical way, {this.userName}." },
                { "what can i ask you about", "You can ask about phishing, password safety, safe browsing, online scams, privacy, suspicious links, public Wi-Fi, and two-factor authentication." },
                { "who created you", "I was created as part of a cybersecurity awareness chatbot project." },
                { "why is cybersecurity important", "Cybersecurity is important because it protects your identity, accounts, money, private information, and devices from online threats." },
                { "hello", $"Hello {this.userName}. What online safety topic would you like to explore?" },
                { "hi", $"Hi {this.userName}. You can ask me about phishing, passwords, scams, privacy, safe browsing, or 2FA." },
                { "help", "Try asking: 'What is phishing?', 'How do I make a strong password?', 'What is safe browsing?', or 'Tell me more about scams'." }
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
            }

            public string GetResponse(string input) // Main chatbot response method
            {
                if (string.IsNullOrWhiteSpace(input))  // Prevents empty user input
                {
                    return "Please type something first.";
                }

                input = input.ToLower().Trim(); // Converts input to lowercase for easier matching

                //Handles exit commands 
                if (input == "exit" || input == "quit" || input == "goodbye" || input == "bye")
                {
                    return "Goodbye " + userName + ".Stay safe online!";
                }

                //Uses delegate for sentiment detection
                BotResponseDelegate feelingDetector = DetectFeeling;
                string feelingReply = feelingDetector(input); //Detects emotion in user message 

                //Handles chatbot memory and recall
                string memoryReply = HandleMemory(input);
                if (!string.IsNullOrWhiteSpace(memoryReply))
                {
                    return memoryReply;
                }

                //Detects follow up requests
                if (IsFollowUp(input))
                {
                    return ContinueTopic();
                }

                //Detects exact keyword responses 
                string partOneReply = DetectPartOneResponse(input);
                string topicReply = DetectTopic(input);

                if (!string.IsNullOrWhiteSpace(feelingReply) && !string.IsNullOrWhiteSpace(topicReply))
                {
                    return feelingReply + Environment.NewLine + Environment.NewLine + topicReply;
                }

                if (!string.IsNullOrWhiteSpace(partOneReply)) return partOneReply;
                if (!string.IsNullOrWhiteSpace(feelingReply)) return feelingReply;
                if (!string.IsNullOrWhiteSpace(topicReply)) return topicReply;

                if (!string.IsNullOrWhiteSpace(favouriteTopic))
                {
                    return "I did not fully understand that, but I remember you are interested in "
                           + favouriteTopic + ". Ask me for another tip about it.";
                }

                return "I could not understand that clearly. Try asking about phishing, passwords, scams, privacy, safe browsing, or 2FA.";
            }

            private string DetectPartOneResponse(string input)
            {
                string reply;

                reply = MatchDictionary(input, generalResponses);
                if (reply != null) return reply;

                reply = MatchDictionary(input, phishingResponses);
                if (reply != null) { activeTopic = "phishing"; return reply; }

                reply = MatchDictionary(input, passwordResponses);
                if (reply != null) { activeTopic = "password"; return reply; }

                reply = MatchDictionary(input, safeBrowsingResponses);
                if (reply != null) { activeTopic = "safe browsing"; return reply; }

                return "";
            }

            private string DetectTopic(string input)
            {
                string topic = FindTopic(input);

                if (string.IsNullOrWhiteSpace(topic))
                {
                    return "";
                }

                activeTopic = topic;

                if (input.Contains("explain") || input.Contains("details") || input.Contains("deep") || input.Contains("more about"))
                {
                    return GetRandomItem(detailedTopicResponses[topic]);
                }

                return GetRandomItem(quickTopicResponses[topic]);
            }

            private string DetectFeeling(string input)
            {
                foreach (string feeling in feelingResponses.Keys)
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

                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        userName = "Friend";
                    }

                    return "Got it. I will remember your name as " + userName + ".";
                }

                if (input.Contains("interested in") || input.Contains("i like") || input.Contains("i care about"))
                {
                    string topic = FindTopic(input);

                    if (!string.IsNullOrWhiteSpace(topic))
                    {
                        favouriteTopic = topic;
                        activeTopic = topic;

                        return "Thanks for sharing that, " + userName +
                               ". I will remember that you are interested in " + favouriteTopic + ".";
                    }
                }

                if (input.Contains("remember") || input.Contains("what do you know about me"))
                {
                    if (!string.IsNullOrWhiteSpace(favouriteTopic) && !string.IsNullOrWhiteSpace(recentFeeling))
                    {
                        return "I remember your name is " + userName +
                               ", you are interested in " + favouriteTopic +
                               ", and you recently sounded " + recentFeeling + ".";
                    }

                    if (!string.IsNullOrWhiteSpace(favouriteTopic))
                    {
                        return "I remember your name is " + userName +
                               " and your main interest is " + favouriteTopic + ".";
                    }

                    return "I remember your name is " + userName + ".";
                }

                return "";
            }

            private bool IsFollowUp(string input)
            {
                return input.Contains("tell me more")
                       || input.Contains("another tip")
                       || input.Contains("explain more")
                       || input.Contains("go deeper")
                       || input.Contains("more detail")
                       || input.Contains("continue");
            }

            private string ContinueTopic()
            {
                if (string.IsNullOrWhiteSpace(activeTopic))
                {
                    return "Choose a topic first: phishing, passwords, scams, privacy, safe browsing, or 2FA.";
                }

                if (detailedTopicResponses.ContainsKey(activeTopic))
                {
                    return GetRandomItem(detailedTopicResponses[activeTopic]);
                }

                return "Ask me about phishing, passwords, scams, privacy, safe browsing, or 2FA.";
            }

            private string FindTopic(string input)
            {
                foreach (var topicGroup in topicKeywords)
                {
                    foreach (string keyword in topicGroup.Value)
                    {
                        if (input.Contains(keyword))
                        {
                            return topicGroup.Key;
                        }
                    }
                }

                return "";
            }

            private string MatchDictionary(string input, Dictionary<string, string> responses)
            {
                if (responses.ContainsKey(input))
                {
                    return responses[input];
                }


                return null;
            }

            private string GetRandomItem(List<string> items)
            {
                int index = random.Next(items.Count);
                return items[index];
            }

            public string GetAsciiArt()
            {
                return AsciiArt.GetAsciiArt();
            }
        }
    }

