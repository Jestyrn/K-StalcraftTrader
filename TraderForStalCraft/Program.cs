using TraderForStalCraft.Scripts;
using TraderForStalCraft.Scripts.HelperScripts;

namespace TraderForStalCraft
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var fileManager = new FileManager();
            Application.Run(new MainForm(fileManager));
        }
    }
}