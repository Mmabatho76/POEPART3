using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace POEPART3
{
    internal class Program
    {
       
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                ApplicatiAon.SetCompatibleTextRenderingDefault(false);
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
        }
    }

    
