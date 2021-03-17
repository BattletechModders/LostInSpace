using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Harmony;
using IRBTModUtils.Logging;
using BattleTech;
using HBS.Collections;
using LostInSpace.Framework;
using UnityEngine;

namespace LostInSpace
{
    public class LostInSpaceInit
    {
        internal static DeferringLogger modLog;
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
                modLog = new DeferringLogger(modDir, "LostInSpace", "LostInSpace", modSettings.debugLog, modSettings.traceLog);
                modLog.Debug?.Write($"Loaded settings from {modDir}/settings.json");
                
            }
            catch (Exception ex)
            {
                modSettings = new Settings();
                modLog = new DeferringLogger(modDir, "LostInSpace", "LostInSpace", true, true);
                modLog.Error?.Write(ex);
            }

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Util.HolderInstance.Initialize();
            modLog.Info?.Write($"Initializing PracticeMakesPerfect - Version {typeof(Settings).Assembly.GetName().Version}");
        }
    }

    public class Settings
    {
        public bool debugLog = true;
        public bool traceLog = true;

        public Dictionary<string, List<string>> hiddenSystems = new Dictionary<string, List<string>>();
    }
}
