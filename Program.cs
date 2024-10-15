using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Net.Http;

namespace CheatHunterBTR
{
    class Program
    {
        // Токен Webhook Discord
        private static string DiscordWebhookUrl = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv";

        // Счетчик проверок
        private static int checkCount = 0;

        static void Main(string[] args)
        {
            // Проверка прав доступа
            if (!IsAdministrator())
            {
                Console.WriteLine("Для работы программы необходимы права администратора.");
                Console.WriteLine("Запустите программу от имени администратора.");
                return;
            }

            // Получение ника игрока от пользователя
            Console.WriteLine("Введите никнейм игрока:");
            string playerNickname = Console.ReadLine();

            // Поиск процесса javaw.exe
            Process javaProcess = FindJavaProcess();
            if (javaProcess == null)
            {
                Console.WriteLine("Процесс javaw.exe не найден. Minecraft не запущен?");
                return;
            }

            // Поиск строк в памяти процесса
            string[] foundStrings = SearchStringsInProcess(javaProcess.Id, playerNickname);

            // Отправка лога в Discord
            if (foundStrings != null && foundStrings.Length > 0)
            {
                SendDiscordLog(playerNickname, foundStrings);
            }

            // Сообщение о завершении проверки
            Console.WriteLine("Проверка завершена. Нажмите ENTER, чтобы закрыть.");
            Console.ReadKey(); // Ожидание нажатия ENTER
        }

        static Process FindJavaProcess()
        {
            // Получить список всех запущенных процессов
            Process[] processes = Process.GetProcessesByName("javaw.exe");

            // Выбрать процесс Java, если он найден
            return processes.FirstOrDefault();
        }

        static string[] SearchStringsInProcess(int processId, string playerNickname)
        {
            // Получить объект управления процессом
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + processId);
            ManagementObject processObject = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (processObject == null)
            {
                Console.WriteLine("Не удалось получить объект процесса Java.");
                return null;
            }

            // Получить список строк из памяти процесса
            string[] strings = processObject.GetPropertyValue("CommandLine") as string[];
            if (strings == null)
            {
                Console.WriteLine("Не удалось получить список строк из памяти.");
                return null;
            }

            // Список строк для поиска
            string[] searchStrings = new string[] { "funtime", "cheat", playerNickname };

            // Найти строки, содержащие искомые значения
            return strings.Where(s => searchStrings.Any(searchString => s.Contains(searchString))).ToArray();
        }

        static void SendDiscordLog(string playerNickname, string[] foundStrings)
        {
            // Формирование данных для отправки в Discord
            var logData = new
            {
                content = $"**Проверка {++checkCount}**\nНикнейм: {playerNickname}\nНайденные строки:\n" +
                          string.Join("\n", foundStrings)
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(logMessage, System.Text.Encoding.UTF8, "application/json");
                client.PostAsync(DiscordWebhookUrl, content).ConfigureAwait(false);
            }
        }

        // Функция для проверки прав администратора
        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
