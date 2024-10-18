using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CheatHunterBTR
{
    private const string WEBHOOK_URL = "https://discord.com/api/webhooks/1296836559471120459/91DxRiMCMauf9oqWpJEQ7IqjoTrerPdAx3PE2xcg0zbqzPY9FJXowOacz0iqRx8ye1Dj";
    private const string STRINGS_FILE_PATH = "strings.txt";
    private static string[] stringsToCheck;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("CheatHunterBTR запущен.");

        try
        {
            stringsToCheck = File.ReadAllLines(STRINGS_FILE_PATH).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            if (stringsToCheck == null || stringsToCheck.Length == 0)
            {
                Console.WriteLine($"Ошибка: файл '{STRINGS_FILE_PATH}' пуст или не найден. Программа завершается.");
                return;
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Ошибка: файл '{STRINGS_FILE_PATH}' не найден. Программа завершается.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}. Программа завершается.");
            return;
        }

        string nick;
        do
        {
            Console.Write("Введите свой никнейм: ");
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
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "javaw.exe";
                    process.StartInfo.Arguments = "путь_к_вашему_приложению.jar";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Ошибка от javaw: {error}");
                        await SendDiscordMessage($"Ошибка проверки для {nick}: {error}");
                        continue;
                    }

                    var foundStrings = FindStrings(output, stringsToCheck);

                    string logMessage = $"{nick}: {string.Join(", ", foundStrings)}";
                    await SendDiscordMessage($"```{logMessage}```");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка во время проверки: {ex.Message}");
            }
        } while (true);
    }


    private static List<string> FindStrings(string text, string[] searchStrings)
    {
        var found = new List<string>();
        foreach (string str in searchStrings)
        {
            if (Regex.IsMatch(text, $"(?i){Regex.Escape(str)}", RegexOptions.IgnoreCase))
            {
                found.Add(str);
            }
        }
        return found;
    }

    private static async Task SendDiscordMessage(string message)
    {
    }
}
