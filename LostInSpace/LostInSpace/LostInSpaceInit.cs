using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using HBS.Logging;

namespace LostInSpace
{
    public class LostInSpaceInit
    {
        internal static ILog modLog;
        internal static string modDir;
        public static Settings modSettings;

        public const string HarmonyPackage = "us.tbone.LostInSpace";

        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;

            try
            {
                using (StreamReader reader = new StreamReader($"{modDir}/settings.json"))
                {
                    string jsData = reader.ReadToEnd();
                    modSettings = JsonConvert.DeserializeObject<Settings>(jsData);
                }

                modLog = Logger.GetLogger("LostInSpace");
                modLog.Log($"Loaded settings from {modDir}/settings.json");

            }
            catch (Exception ex)
            {
                modSettings = new Settings();
                modLog = Logger.GetLogger("LostInSpace");
                modLog.LogException(ex);
            }

            modLog.Log($"Initializing LostInSpace - Version {typeof(Settings).Assembly.GetName().Version}");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }
    }

    public class Settings
    {
        public Dictionary<string, List<string>> hiddenSystems = new Dictionary<string, List<string>>();
    }
}
