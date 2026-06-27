using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEPART3
{
 

        public static class AsciiArt
        {
            public static string GetAsciiArt()
            {
                // No leading newline here so the art appears at the
                // current caret position without an extra blank line.
                return @"╔══════════════════════════════════════════════════════╗
║                  AZEEBOT SYSTEM                      ║
╠══════════════════════════════════════════════════════╣
║        _____ _____  _____  ______ _____              ║
║       / ____|  __ \|  __ \|  ____|  __ \             ║
║      | (___ | |  | | |__) | |__  | |  | |            ║
║       \___ \| |  | |  _  /|  __| | |  | |            ║
║       ____) | |__| | | \ \| |____| |__| |            ║
║      |_____/|_____/|_|  \_\______|_____/             ║
║                                                      ║
║         Cybersecurity Awareness Chatbot              ║
║            Stay Safe. Stay Smart.                    ║
╚══════════════════════════════════════════════════════╝";
            }
        }
    }

