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
                "Legit",
                "Karma",
                "Dream",
                "Entropy",
                "Drip",
                "Vape",
                "Akira",
                "Rise",
                "Cortex",
                "Ammit",
                "Doomsday",
                "Rockstar Legit",
                "vZenWare",
                "Troxill",
                "Nursultan",
                "Celestial",
                "Expensive",
                "Rockstar",
                "Wild",
                "Minced",
                "Fuze Client",
                "Arbuz Client",
                "\vmdoqhEwxdPU",
                "H]vaivPBy[dIRvLHv",
                "rVXAjswRuyTFWaV",
                "DNiqO;ae2)2^HEC",
                "mXs_os_Pig^o]R]OgS",
                "i^HTJ06MQ>K",
                "_jc\",
                "cQxjrTsCXNfroQAk",
                "vFokMOqak_MULU",
                "psKoqaK",
                "r&SF[!",
                "LZ L-.iRLT [CF6",
                "W_lk<Ifo[2mVqo",
                "OS7skZ72v]oan_0ZaV",
                "^rtS&sj<k82",
                ".Ln1A4<cH3_UD",
                "YqKZr5&I\dqLSfy(i",
                "ZXJdf)P+xfvWSj",
                "uZ3ot]43#'XctU[",
                "nb(:PbrO(2",
                "(Laps;IIZZZ)Vjav",
                "Du[Gxt[JuWTOInEHB",
                "rc^SoAMohrofl_G",
                "kZDBQbP",
                "XCjdxJW[lXGxAlYjLO",
                ",+vk%S[ir_nZL8'K!7X",
                "<-8dd",
                ":79Q",
                "com.oiseth.doomsday",
                "EXGFEZGQCEDwHBID",
                "nfuTQLVsKir",
                "LPkS5tf5Nu2yS:",
                "redefined class",
                "n:s.v",
                "URZtR@t",
                "getEventButtonState",
                "SCHEDULED_EXECUTABES",
                "hLit/unimi/dsi/fastutil/objects/Object2DoubleMap<Lnet/minecraft/tags/ITag<Lnet/minecraft/fluid/Fluid;>;>;",
                "exitsave",
                ":|:",
                "71KD",
                "HNtXxl",
                "--LLdd5522",
                "-%0"0"-%-%-%4%4%4%0%0%",
                "HhhHHh)DO>OD",
                "ZS'1|",
                "Troxil",
                "radioegor146",
                "ZDCoder",
                "(Ljava/lang/String;FFFFZ)",
                "CortexConfig",
                ".]+5+]5",
                "71KD",
                "(Ljava/lang/Object;ZI",
                "(Ljava/lang/String;)V",
                "]]]]10]]]]",
                "99]]]]]]10",
                "9910]]]]]]]]_]]]]",
                "eL-9bH+T]]] ",
                "H+7P2j9FVp1qru6qpnQj8DXs8hvsaZXYO++MyjQVBa0=",
                "]]]]10]]]] ",
                "eL-9bH+T]]]",
                "]]]]++Z",
                "]]]]]%",
                "(Ljava/lang/String;IILjava/lang/String;I)V/l}",
                ")"""w'""""""""""""""3s'"""""33s""&"",
                """"23#"!"""11<#null>"'"",
                """""""""""""""""""""""""""""""""""""""""r'""""""r'""""""""""""""""""""""""B$""""""D$""""""D$""""""D$"""""BD$""""""D""""""BD"""""""$"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""D$""""w"D$""""wBD$"""""BD$"""""DD$"""""BD$"""""BG$""<#null>BD"""""""""""""""""R"""""""U%""""""R"""""""""""""",
                "--LLdd5522",
                "-%0"0"-%-%-%4%4%4%0%0%",
                "!!5!27654'!5!27654'!5!",
                "]]?10]]]]",
                "]]]]]]]]++++++10",
                "?]]]]",
                "22/+^]]++",
                "99^]]]]",
                "T"T(T.T4T:T @TFTLTRTXT4T^TdTjT",
                "3t3t3t3t3t3t3t3t4",
                "_^]++++",
                "?10^]]]]]]]]]]]^]]]]]]%",
                "99?_]10]]]]]]]]]]",
                "_q]q]]q]+++++",
                "]]]]_]10]]]]]]]",
                ".]+5+]5pastebin",
                "]]]+4]",
                "5+]5]]q",
                "10^]^]]]!!46",
                ",Lnet/labymod/ingamechat/tabs/GuiChatSymbols;",
                "7Ljava/util/HashMap<Ljava/lang/String;Ljava/lang/Long;>;",
                "1Lit/unimi/dsi/fastutil/ints/Int2ObjectMap<Lcxb;>;",
                "{M7CP^xsLG^s|lf}zb[^X?.?_KJUG:GQ[dS>0&3Tr",
                "=com/google/common/util/concurrent/AbstractFuture$AtomicHelper",
                "4()[Lnet/labymod/ingamechat/renderer/EnumMouseAction;",
                "(Ljava/lang/Object;Ljava/lang/String;)Ljava/lang/String;Q",
                "eyJwcm9maWxlTmFtZSI6ImtyaXN0aXMi",
                "330b28432fb13a05feb15fec4e5e30cbc4d53d15",
                "+848:8>(P(R(a(d(g(gO8<068H8Q8e8q8{8O@T@W@a@1@4@=@A@I@V@",
                "H@5@78O85@?@R@c@",
                ")(Lnet/minecraft/server/MinecraftServer;)Z",
                "4(Ljava/util/function/Supplier<Ljava/lang/String;>;)V",
                "#(Lnet/minecraft/profiler/Snooper;)V",
                "/(Lnet/minecraft/server/management/PlayerList;)V",
                "(Ljava/util/List;)Lnet/minecraft/util/math/shapes/VoxelShapes$ILineConsumer;=",
                "(Lnet/minecraft/entity/Entity;)Ljava/util/function/Consumer;3",
                "3(Lnet/minecraft/entity/player/ServerPlayerEntity;)V",
                "2net/minecraft/util/registry/DynamicRegistries$Impl",
                "5Lnet/minecraft/client/gui/toasts/TutorialToast$Icons;",
                "(Lnet/minecraft/class_846;Lnet/minecraft/class_846$class_4690;Lnet/minecraft/class_750;)Ljava/lang/Runnable;}",
                "(Lnet/minecraft/class_255;Lnet/minecraft/class_247;Lnet/minecraft/class_251;ILnet/minecraft/class_251;IIII)Z}",
                "(Lnet/minecraft/class_5481;Lnet/minecraft/class_5481;)Lnet/minecraft/class_5481;}",
                "(Lnet/minecraft/class_1297;)Ljava/util/function/BiPredicate;}",
                "(Lnet/minecraft/class_2558$class_2559;)Lnet/minecraft/class_2558$class_2559;}",
                "(Lnet/minecraft/class_2568$class_5247;)Lnet/minecraft/class_2568$class_5247;}",
                "(Lnet/minecraft/class_3191;)I",
                "(Lnet/minecraft/class_4587;IIIIII)Vs",
                "0(Lnet/minecraft/class_1132;)Ljava/lang/Runnable;",
                "(Lnet/minecraft/class_327;)Ljava/util/function/Function;}",
                "it.unimi.dsi.fastutil.longs.LongArrays$ForkJoinQuickSortCompy",
                "pastebin",
                "AimAssist",
                "nelqr",
                "bh\kF",
                "D$@&"6G",
                "x9D;e",
                "W`Dh0",
                "bsWg7",
                "&-]UR",
                "@?sPOR",
                "ili-e",
                "]O-KfX",
                "N IO BI UM",
                "VNejaWA.ModuleCh",
                "dreampool",
                "forge.commons.",
                "hitbox:",
                "reach:",
                "0Getenv",
                "me.didyoumuch.Native",
                "ldldld.java",
                "wPy^eopgDr",
                "]H"JF ",
                "Az85'. ",
                "71L;",
                "V: ",
                "stubborn.website",
                "bushroot",
                "hitbox:",
                "ASM:",
                "Val: ",
                "%width%",
                "Triggerbot",
                "event",
                "baobab",
                "killaura ",
                "handler",
                "VAPE4DLL",
                "InvisibleHitbox",
                "expensive",
                "ASMEventHandler_",
                "celka",
                "allatori
                "magicthein",
                "chs/Main",
                "chs/EH",
                "chs/MM",
                "chs/Profiller",
                "chs/90",
                "Noise Client",
                "b/time",
                "clowdy",
                "neathitbox",
                "Gk8oA",        
                "Derick1337",
                "#THV",
                "okuma:",
                "sfxcmd",
                "/Class<*>;Ljava/lang/String;Ljava/lang/Object;)V",
                "lookAheadStep",
                "Zero/Time",
                "#Hit",
                "BeackGA",
                "BreakHitsOn",
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
