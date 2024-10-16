using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Net.Http;
using System.Collections.Generic;

namespace CheatHunterBTR
{
    class Program
    {
        // Токен Webhook Discord
        private static readonly string DiscordWebhookUrl = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv";

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

            // Поиск PID процесса javaw.exe
            int javaProcessId = FindJavaProcessId();
            if (javaProcessId == -1)
            {
                Console.WriteLine("Процесс javaw.exe не найден. Введите PID процесса javaw.exe вручную:");
                string pidInput = Console.ReadLine();
                if (int.TryParse(pidInput, out javaProcessId))
                {
                    Console.WriteLine("PID получен.");
                }
                else
                {
                    Console.WriteLine("Неверный PID.");
                    return;
                }
            }

            // Поиск строк в памяти процесса
            string[] foundStrings = SearchStringsInProcess(javaProcessId, playerNickname);

            // Отправка лога в Discord
            if (foundStrings != null && foundStrings.Length > 0)
            {
                SendDiscordLog(playerNickname, foundStrings);
            }

            // Сообщение о завершении проверки
            Console.WriteLine("Проверка завершена. Нажмите ENTER, чтобы закрыть.");
            Console.ReadKey(); // Ожидание нажатия ENTER
        }

        static int FindJavaProcessId()
        {
            // Использовать tasklist для поиска PID процесса Java (с использованием внешней команды)
            try
            {
                // Запускаем команду tasklist и фильтруем по "javaw.exe"
                string command = "tasklist | findstr /i \"javaw.exe\"";
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string pidString = process.StandardOutput.ReadLine();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(pidString))
                {
                    // Выделение PID из строки, например, "javaw.exe      1234 Console"
                    string[] parts = pidString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int pid))
                    {
                        return pid;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении PID: {ex.Message}");
            }

            return -1;
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

            // Загрузить список строк для поиска 
            HashSet<string> searchStrings = LoadSearchStrings();

            // Найти строки, содержащие искомые значения
            return strings.Where(s => searchStrings.Contains(s)).ToArray();
        }

        static HashSet<string> LoadSearchStrings()
        {
            // Загрузить список строк для поиска 
            HashSet<string> searchStrings = new HashSet<string>()
            {
                "FunTime",
                "hLit/unimi/dsi/fastutil/objects/Object2DoubleMap<Lnet/minecraft/tags/ITag<Lnet/minecraft/fluid/Fluid;>;>;"
            };

            return searchStrings;
        }

        static void SendDiscordLog(string playerNickname, string[] foundStrings)
        {
            // Формирование сообщения для Discord
            string logMessage = $"**Проверка {++checkCount}**\nНикнейм: {playerNickname}\nНайденные строки:\n" +
                               string.Join("\n", foundStrings);

            // Отправка сообщения в Discord
            using (var client = new HttpClient())
            {
                var content = new StringContent(logMessage, System.Text.Encoding.UTF8, "text/plain");
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
