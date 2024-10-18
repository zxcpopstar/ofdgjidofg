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
    private static string[] stringsToCheck;

    public static async Task Main(string[] args)
    {
        stringsToCheck = ReadStringsFromFile(STRINGS_FILE);
        if (stringsToCheck == null)
        {
            Console.WriteLine($"Ошибка: файл '{STRINGS_FILE}' не найден или не может быть прочитан.");
            return;
        }

        string nick;
        do
        {
            Console.Write("Введите свой никнейм на сервере (или 'exit' для выхода): ");
            nick = Console.ReadLine();

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
                Console.WriteLine($"Проверка #{checkId} для {nick}...");

                using (Process javawProcess = new Process())
                {
                    javawProcess.StartInfo.FileName = "javaw.exe";
                    javawProcess.StartInfo.RedirectStandardOutput = true;
                    javawProcess.StartInfo.UseShellExecute = false;
                    javawProcess.StartInfo.CreateNoWindow = true;

                    try
                    {
                        javawProcess.Start();
                        var foundStrings = await CheckForStringsAsync(nick, stringsToCheck, javawProcess, TimeSpan.FromSeconds(10));
                        await SendDiscordMessage(FormatDiscordMessage(nick, checkId, foundStrings));
                        javawProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка запуска или работы javaw: {ex.Message}");
                        javawProcess.Kill();
                    }
                    finally
                    {
                        javawProcess.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        } while (true);
    }

    private static string FormatDiscordMessage(string nick, long checkId, List<string> foundStrings)
    {
        string message = $@"```
{nick}; #{checkId}; ";
        if (foundStrings.Count > 0)
        {
            message += string.Join(", ", foundStrings);
        }
        else
        {
            message += "Ничего не найдено";
        }
        message += "\n```";
        return message;
    }

    private static async Task<List<string>> CheckForStringsAsync(string nick, string[] stringsToCheck, Process javawProcess, TimeSpan timeout)
    {
        var foundStrings = new List<string>();
        var cancellationTokenSource = new CancellationTokenSource(timeout);

        try
        {
            using (var output = javawProcess.StandardOutput)
            {
                string processOutput = await output.ReadToEndAsync(cancellationTokenSource.Token);

                foreach (string str in stringsToCheck)
                {
                    if (Regex.IsMatch(processOutput, $"(?i){Regex.Escape(str)}", RegexOptions.IgnoreCase))
                    {
                        foundStrings.Add(str);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в CheckForStringsAsync: {ex.Message}");
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
            using var request = new HttpRequestMessage(HttpMethod.Post, WEBHOOK_URL);
            request.Content = new StringContent($@"{{""content"":""{message}"",""username"":""CheatHunter""}}", Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки сообщения в Discord: {ex.Message}");
        }
    }
}
