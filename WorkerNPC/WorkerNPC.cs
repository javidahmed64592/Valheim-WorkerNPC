using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace WorkerNPC
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        const string pluginGUID = "com.example.GUID";
        const string pluginName = "WorkerNPC";
        const string pluginVersion = "0.1.0";

        private readonly Harmony HarmonyInstance = new Harmony(pluginGUID);

        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Main.logger.LogInfo("Loading WorkerNPC mod...");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        public void Start()
        {
            // Add chest to build menu in Misc tab
            WorkerChest.RegisterWorkerChest();
        }
    }
}
