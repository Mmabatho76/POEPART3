# POEPART3

AZEEBOT – Cybersecurity Awareness Assistant
PROG6221 / POE PART 3
South Africa – Gauteng

============================================
PURPOSE
============================================
This Windows Forms application acts as an interactive cybersecurity chatbot.
Features:
- Answer security‑related questions
- Run short knowledge quiz
- Create / view / mark / delete tasks
- Activity history log
- SQLite database storage

============================================
REQUIREMENTS
============================================
- Microsoft Visual Studio 2022
- .NET Framework 4.8 or later
- NuGet package: Microsoft.Data.Sqlite 10.0.9
  (No vulnerable old SQLite versions included)

============================================
HOW TO INSTALL & RUN
============================================
1. Open solution: POEPART3.sln
2. Install package:
   Tools → NuGet Package Manager →
   Install: Microsoft.Data.Sqlite
3. Build solution
4. Run project
- Database file AzeeBotTasks.db creates automatically
- No extra setup needed

============================================
USAGE GUIDE
============================================
Main interface sections:
- Chat area: type commands or questions
- Task panel: Add / Mark / Delete / View Log
- Quiz: type “quiz” or “start quiz”

Supported commands:
• hello / hi
• add task / new task
• show tasks / list tasks
• mark complete [ID]
• delete task [ID]
• quiz / start quiz
• log / history
• exit / quit

Security topics: phishing, passwords, 2FA, OTP, malware

============================================
PROJECT STRUCTURE
============================================
POEPART3/
 ├─ Form1.cs / Form1.Designer.cs
 ├─ ChatbotEngine.cs      → logic + quiz + replies
 ├─ DatabaseHelper.cs     → SQLite read/write
 ├─ QuizQuestion.cs
 ├─ TaskItem.cs
 ├─ ActivityLogEntry.cs
 ├─ packages.config
 ├─ README.txt           AZEEBOT – Cybersecurity Awareness Assistant
PROG6221 / POE PART 3
South Africa – Gauteng

============================================
PURPOSE
============================================
This Windows Forms application acts as an interactive cybersecurity chatbot.
Features:
- Answer security‑related questions
- Run short knowledge quiz
- Create / view / mark / delete tasks
- Activity history log
- SQLite database storage

============================================
REQUIREMENTS
============================================
- Microsoft Visual Studio 2022
- .NET Framework 4.8 or later
- NuGet package: Microsoft.Data.Sqlite 10.0.9
  (No vulnerable old SQLite versions included)

============================================
HOW TO INSTALL & RUN
============================================
1. Open solution: POEPART3.sln
2. Install package:
   Tools → NuGet Package Manager →
   Install: Microsoft.Data.Sqlite
3. Build solution
4. Run project
- Database file AzeeBotTasks.db creates automatically
- No extra setup needed

============================================
USAGE GUIDE
============================================
Main interface sections:
- Chat area: type commands or questions
- Task panel: Add / Mark / Delete / View Log
- Quiz: type “quiz” or “start quiz”

Supported commands:
• hello / hi
• add task / new task
• show tasks / list tasks
• mark complete [ID]
• delete task [ID]
• quiz / start quiz
• log / history
• exit / quit

Security topics: phishing, passwords, 2FA, OTP, malware

============================================
PROJECT STRUCTURE
============================================
POEPART3/
 ├─ Form1.cs / Form1.Designer.cs
 ├─ ChatbotEngine.cs      → logic + quiz + replies
 ├─ DatabaseHelper.cs     → SQLite read/write
 ├─ QuizQuestion.cs
 ├─ TaskItem.cs
 ├─ ActivityLogEntry.cs
 ├─ packages.config
 ├─ README.txt            
 └─ bin/obj folders
