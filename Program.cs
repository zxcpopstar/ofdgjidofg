using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Net.Http;
using System.IO;

namespace CheatHunterBTR
{
    class Program
    {
        private static string DiscordWebhookUrl = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv";

        private static int checkCount = 0;

        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Для работы программы необходимы права администратора.");
                Console.WriteLine("Запустите программу от имени администратора.");
                return;
            }

            Console.WriteLine("Введите свой никнейм в игре!:");
            string playerNickname = Console.ReadLine();

            int javaProcessId = FindJavaProcessId();
            if (javaProcessId == -1)
            {
                Console.WriteLine("Процесс javaw.exe не найден...");
                Console.ReadKey();
                return;
            }

            string[] foundStrings = SearchStringsInProcess(javaProcessId, playerNickname);

            if (foundStrings != null && foundStrings.Length > 0)
            {
                SendDiscordLog(playerNickname, foundStrings);
            }

            Console.WriteLine("Проверка завершена. Нажмите ENTER, чтобы закрыть.");
            Console.ReadKey();
        }

        static int FindJavaProcessId()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId, Name FROM Win32_Process WHERE Name = 'javaw.exe'");
            ManagementObjectCollection processes = searcher.Get();

            foreach (ManagementObject process in processes)
            {
                return (int)process["ProcessId"];
            }

            return -1;
        }

        static string[] SearchStringsInProcess(int processId, string playerNickname)
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + processId);
            ManagementObject processObject = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (processObject == null)
            {
                Console.WriteLine("Не удалось получить объект процесса Java.");
                return null;
            }

            string[] strings = processObject.GetPropertyValue("CommandLine") as string[];
            if (strings == null)
            {
                Console.WriteLine("Не удалось получить список строк из памяти.");
                return null;
            }

            string[] searchStrings = new string[] { "funtime", "cheat", playerNickname };

            return strings.Where(s => searchStrings.Any(searchString => s.Contains(searchString))).ToArray();
        }

        static void SendDiscordLog(string playerNickname, string[] foundStrings)
        {
            string logMessage = $"**Проверка {++checkCount}**\nНикнейм: {playerNickname}\nНайденные строки:\n" +
                               string.Join("\n", foundStrings);

            using (var client = new HttpClient())
            {
                var content = new StringContent(logMessage, System.Text.Encoding.UTF8, "text/plain");
                client.PostAsync(DiscordWebhookUrl, content).ConfigureAwait(false);
            }
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
