using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.IO.Compression;

namespace CheatHunterBTR
{
    class Program
    {
        static void Main(string[] args)
        {
            // Список директорий, где искать javaw.exe
            string[] directories = {
                @"C:\Program Files\Java\",
                @"C:\Program Files (x86)\Java\",
                @"C:\Minecraft\",
                @"C:\Users\Public\Documents\Minecraft\",

                // Получаем путь к AppData текущего пользователя
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"legacy\"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Lunar\"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $".tlauncher\"), // Добавили .tlauncher

                // Динамический путь к .minecraft
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $".minecraft\\"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $".minecraft\\versions\\"), // Добавили versions
                // ... (другие пути) ...
            };

            // Список версий Minecraft
            string[] minecraftVersions = {
                "1.16.5", "1.17", "1.17.1", "1.18", "1.18.1", "1.19", "1.19.1", "1.19.2", "1.19.3",
                "1.20.1", "1.20.2", "1.20.3", "1.20.4", "1.20.5", "1.20.6", "1.21"
            };

            // Список модификаций
            string[] modifications = {
                "Fabric", "Forge", "ForgeOptifine", "Optifine", "Vanilla",
                "Lunar Client", "LabyMod-3", "LabyMod-4", "Feather Client"
            };

            // Ищем javaw.exe
            string javawPath = FindJavaw(directories, minecraftVersions, modifications);

            // Проверяем, найден ли javaw.exe
            if (javawPath != null)
            {
                Console.WriteLine($"Найден javaw.exe: {javawPath}");
            }
            else
            {
                Console.WriteLine("javaw.exe не найден.");
            }

            // Ожидаем нажатия Enter
            Console.WriteLine("Проверка завершена. Чтобы закрыть чекер, нажмите Enter.");
            Console.ReadLine();
        }

        // Функция поиска javaw.exe
        static string FindJavaw(string[] directories, string[] minecraftVersions, string[] modifications)
        {
            foreach (string directory in directories)
            {
                // Проверяем, существует ли директория
                if (Directory.Exists(directory))
                {
                    // Рекурсивный обход директории
                    foreach (string subdirectory in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
                    {
                        // Проверяем, содержит ли директория Minecraft версии
                        foreach (string version in minecraftVersions)
                        {
                            if (subdirectory.Contains(version))
                            {
                                // Проверяем, содержит ли директория модификации
                                foreach (string modification in modifications)
                                {
                                    if (subdirectory.Contains(modification))
                                    {
                                        // Проверяем, существует ли javaw.exe в директории
                                        string javawPath = Path.Combine(subdirectory, "javaw.exe");
                                        if (File.Exists(javawPath))
                                        {
                                            return javawPath;
                                        }

                                        // Дополнительная проверка .jar файлов в version
                                        if (subdirectory.Contains("versions") && Directory.Exists(subdirectory))
                                        {
                                            foreach (string jarFile in Directory.EnumerateFiles(subdirectory, "*.jar"))
                                            {
                                                using (ZipArchive archive = ZipFile.OpenRead(jarFile))
                                                {
                                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                                    {
                                                        if (entry.Name.EndsWith("javaw.exe", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            return Path.Combine(subdirectory, jarFile, entry.FullName);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // javaw.exe не найден
            return null;
        }
    }
}
