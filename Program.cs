using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CheatHunterBTR
{
    class Program
    {
        static string modFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "minecraft", "mods");
        static string webhookUrl = "YOUR_DISCORD_WEBHOOK_URL"; // Замените на свой Webhook URL
        static readonly HttpClient httpClient = new HttpClient();

        // Метод для отправки данных в Discord
        static async void SendDataToDiscord(string javaExePath)
        {
            var data = new
            {
                content = $"Найден javaw.exe: {javaExePath}"
            };

            var json = JsonConvert.SerializeObject(data); // Удаление JSON-преобразования
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // Удаление JSON-формата

            try
            {
                var response = await httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Данные успешно отправлены в Discord.");
                }
                else
                {
                    Console.WriteLine("Ошибка отправки данных в Discord.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static void Main(string[] _args)
        {
            Console.WriteLine("CheatHunterBTR запущен...");

            // ... (Твой код из Program.cs)

            // Цикл по всем процессам
            Process[] allProcesses = Process.GetProcesses();

            foreach (Process process in allProcesses)
            {
                // Проверяем, является ли процесс Javaw.exe
                if (process.ProcessName.Equals("javaw"))
                {
                    string imageName = process.MainModule.FileName;

                    // Проверка Image file name
                    if (imageName.Contains("javaw.exe") &&
                        !imageName.EndsWith("win32") &&
                        !imageName.Contains("win32"))
                    {
                        string javaExePath = Path.GetDirectoryName(imageName);

                        // Вывод пути к javaw.exe
                        Console.WriteLine($"Путь к javaw.exe: {javaExePath}");

                        // Отправить информацию о Javaw.exe в Discord
                        SendDataToDiscord(javaExePath);

                        // Дополнительная проверка на наличие запущенных Minecraft-процессов
                        if (CheckMinecraftProcesses())
                        {
                            Console.WriteLine("Minecraft запущен!");
                            // ... (Дополнительные действия)
                        }
                        break; // Выход из цикла, если путь найден
                    }
                }
            }

            // ... (Твой код из Program.cs)
        }

        // Метод проверки запущенных Minecraft-процессов
        static bool CheckMinecraftProcesses()
        {
            Process[] processes = Process.GetProcessesByName("Minecraft");

            if (processes.Length > 0)
            {
                return true; // Minecraft запущен
            }
            else
            {
                return false; // Minecraft не запущен
            }
        }
    }
}
