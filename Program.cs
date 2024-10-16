using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Management;

namespace CheckStrings
{
    class Program
    {
        static int checkNumber = 0;

        // Массив шаблонов (регулярных выражений) - *важно* исправьте их!
        private static readonly string[] patterns =
        {
            @"funtime",
            @"\vmdoqhEwxdPU",
        };

        static void Main(string[] args)
        {

            // Ввод ника
            Console.Write("Введите ник: ");
            string nickname = Console.ReadLine();

            // Увеличение номера проверки
            checkNumber++;
            Console.WriteLine($"Номер проверки: #{checkNumber}");

            // Поиск javaw.exe
            Process javawProcess = FindJavawProcess();

            if (javawProcess != null)
            {
                Console.WriteLine($"Найден процесс javaw: {javawProcess.ProcessName} (PID: {javawProcess.Id})");
                Console.WriteLine("Ищем...");

                SendDiscordMessage(nickname, checkNumber, patterns);
            }
            else
            {
                Console.WriteLine("Процесс javaw не найден.");
                // Просим ввести PID вручную или получить его из System Informer
                int pid = GetPidFromUserOrSystemInformer();
                if (pid != 0)
                {
                    // Если PID получен успешно, пытаемся получить процесс
                    javawProcess = Process.GetProcessById(pid);
                    if (javawProcess != null)
                    {
                        Console.WriteLine($"Найден процесс javaw по PID: {pid}");
                        Console.WriteLine("Ищем...");

                        // Отправляем сообщение в Discord без проверки файла
                        SendDiscordMessage(nickname, checkNumber, patterns);
                    }
                    else
                    {
                        Console.WriteLine($"Не удалось найти процесс javaw по PID: {pid}");
                    }
                }
                else
                {
                    Console.WriteLine("Не удалось найти javaw по PID.");
                }
            }

            Console.ReadKey();
        }

        // Метод для поиска процесса javaw.exe
        static Process FindJavawProcess()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == "javaw")
                {
                    return process;
                }
            }
            return null; // Процесс не найден
        }

        // Метод для получения PID от пользователя или из System Informer
        static int GetPidFromUserOrSystemInformer()
        {
            Console.Write("Введите PID javaw.exe или нажмите Enter для поиска в System Informer: ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                // Введен PID
                if (int.TryParse(input, out int pid))
                {
                    return pid;
                }
                else
                {
                    Console.WriteLine("Некорректный формат PID.");
                    return 0;
                }
            }
            else
            {
                // Поиск в System Informer
                return GetPidFromSystemInformer();
            }
        }

        // Метод для получения PID из System Informer
        static int GetPidFromSystemInformer()
        {
            try
            {
                // Запрос к WMI для получения информации о процессе javaw.exe
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process WHERE Name = 'javaw.exe'");
                foreach (ManagementObject mo in searcher.Get())
                {
                    return (int)mo["ProcessId"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении PID из System Informer: {ex.Message}");
            }
            return 0; // PID не найден
        }

        // Метод для отправки сообщения в Discord Webhook
        static async void SendDiscordMessage(string nickname, int checkNumber, string[] patterns)
        {
            // Webhook URL
            string webhookUrl = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv";

            foreach (string pattern in patterns)
            {
                // Проверяем, соответствуют ли шаблоны  никам в  patterns
                bool isMatch = Regex.IsMatch(nickname, pattern);
                if (isMatch)
                {
                    using (var client = new HttpClient())
                    {
                        var content = new StringContent($"{nickname} ; #{checkNumber} ; {pattern}");
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = await client.PostAsync(webhookUrl, content);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Сообщение успешно отправлено в Discord.");
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка при отправке сообщения в Discord: {response.StatusCode}");
                        }
                    }
                }
            }
        }
    }
}
