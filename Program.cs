using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class CheatHunterBTR
{
    private const string WEBHOOK_URL = "https://discord.com/api/webhooks/1295397605378887700/osasnvN87qmehkcFreGL33jgw-YmHdQTZ1b1idfDYb79C23lNT-oo7crS2S3ayG48Exv"; // Замените на ваш webhook
    private const string STRINGS_FILE = "strings/strings.txt";
    private static long checkId = 0;
    private static bool checking = false;
    private static string[] stringsToCheck;


    public static async Task Main(string[] args)
    {
        stringsToCheck = ReadStringsFromFile(STRINGS_FILE);
        if (stringsToCheck == null)
        {
            Console.WriteLine($"Ошибка: файл '{STRINGS_FILE}' не найден или не может быть прочитан.");
            return;
        }

        Console.WriteLine("CheatHunterBTR запущен.");

        while (true)
        {
            Console.Write("Введите никнейм для проверки (или 'exit' для выхода): ");
            string nick = Console.ReadLine();

            if (string.IsNullOrEmpty(nick))
            {
                Console.WriteLine("Введите никнейм.");
                continue;
            }

            if (nick.ToLower() == "exit")
            {
                Console.WriteLine("Чекер закрыт.");
                return;
            }

            try
            {
                checkId++;
                checking = true;
                Console.WriteLine($"Проверка #{checkId} для {nick}...");

                using (Process javawProcess = new Process())
                {
                    javawProcess.StartInfo.FileName = "javaw.exe";
                    javawProcess.StartInfo.RedirectStandardOutput = true;
                    javawProcess.StartInfo.UseShellExecute = false;
                    javawProcess.StartInfo.CreateNoWindow = true;
                    javawProcess.Start();

                    var foundStrings = await CheckForStringsAsync(nick, stringsToCheck, javawProcess, TimeSpan.FromSeconds(10));
                    checking = false;

                    string message = FormatDiscordMessage(nick, checkId, foundStrings);
                    await SendDiscordMessage(message);


                    javawProcess.WaitForExit();
                    javawProcess.Close();

                    Console.WriteLine("Нажмите Enter для следующей проверки, или 'exit' для выхода:");
                    string exitInput = Console.ReadLine();
                    if (exitInput.ToLower() == "exit")
                    {
                        Console.WriteLine("Чекер закрыт.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                checking = false;
            }
        }
    }

    private static async Task<List<string>> CheckForStringsAsync(string nick, string[] stringsToCheck, Process javawProcess, TimeSpan timeout)
    {
        var foundStrings = new List<string>();
        var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            using var output = javawProcess.StandardOutput;
            string processOutput = await output.ReadToEndAsync(cancellationTokenSource.Token);

            foreach (string str in stringsToCheck)
            {
                if (Regex.IsMatch(processOutput, $"(?i){Regex.Escape(str)}", RegexOptions.IgnoreCase))
                {
                    foundStrings.Add(str);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            javawProcess.Kill();
            return new List<string>();
        }
        return foundStrings;
    }


    private static string[] ReadStringsFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл '{filePath}' не найден.");
                return null;
            }

            return File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
            return null;
        }
    }


    private static async Task SendDiscordMessage(string message)
    {
        try
        {
            using HttpClient client = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, WEBHOOK_URL)
            {
                Content = new StringContent($@"{{""content"":""{message}"",""username"":""CheatHunter""}}", Encoding.UTF8, "application/json")
            };
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки сообщения в Discord: {ex.Message}");
        }
    }
}
