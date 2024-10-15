using System;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;

namespace CheatHunterBTR
{
    class Program
    {
        static int checkNumber = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите свой никнейм:");
            string nickname = Console.ReadLine();

            Thread searchThread = new Thread(() =>
            {
                // Проверяем, запущен ли Minecraft
                if (IsMinecraftRunning())
                {
                    Process[] javaProcesses = Process.GetProcessesByName("javaw");

                    if (javaProcesses.Length > 0)
                    {
                        foreach (Process javaProcess in javaProcesses)
                        {
                            ProcessModuleCollection modules = javaProcess.Modules;

                            foreach (ProcessModule module in modules)
                            {
                                if (module.FileName.Contains("wallhack") ||
                                    module.FileName.Contains("aimbot") ||
                                    module.FileName.Contains("esp") ||
                                    module.FileName.Contains("funtime"))
                                {
                                    Console.WriteLine("Мы нашли ПО!");
                                    checkNumber++;
                                    SendDiscordWebhook(nickname, checkNumber.ToString(), module.FileName);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Мы не нашли ПО.");
                    }
                }
                else
                {
                    Console.WriteLine("Minecraft не запущен.");
                }
            });

            searchThread.Start();
            searchThread.Join();

            Console.WriteLine("Нажмите Enter, чтобы закрыть чекер.");
            Console.ReadKey();
        }

        // Функция для проверки запуска Minecraft
        static bool IsMinecraftRunning()
        {
            // Проверяем наличие процесса "javaw" с названием "Minecraft.exe" или "MinecraftLauncher.exe"
            foreach (Process process in Process.GetProcessesByName("javaw"))
            {
                if (process.MainWindowTitle.Contains("Minecraft.exe") ||
                    process.MainWindowTitle.Contains("MinecraftLauncher.exe"))
                {
                    return true;
                }
            }

            return false;
        }

        static async void SendDiscordWebhook(string nickname, string checkNumber, string foundString)
        {
            // Заменяйте  это  на  свой  вебхук  Discord
            string webhookUrl = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv";

            // Создаем строку сообщения вручную
            string jsonMessage = $"{{\"content\": \"Нашлось ПО\\nник: {nickname}\\nномер проверки: #{checkNumber}\\nстрока: {foundString}\"}}";

            // Отправляем сообщение с помощью HttpClient
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(webhookUrl, new StringContent(jsonMessage, System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Сообщение отправлено в Discord!");
                }
                else
                {
                    Console.WriteLine($"Ошибка отправки сообщения в Discord: {response.StatusCode}");
                }
            }
        }
    }
}
