using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System.Reflection;

namespace WorkerNPC
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Main : BaseUnityPlugin
    {
        const string pluginGUID = "com.javidahmed64592.valheim.worker-npc";
        const string pluginName = "WorkerNPC";
        const string pluginVersion = "0.1.0";

        private readonly Harmony HarmonyInstance = new Harmony(pluginGUID);

        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Jotunn.Logger.LogInfo("Loading WorkerNPC mod...");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        public void Start()
        {
            // Add NPC
            FuelWorker resinWorker = new FuelWorker();
            resinWorker.RegisterWorkerNPC();

            BuildingRepairWorker repairWorker = new BuildingRepairWorker();
            repairWorker.RegisterWorkerNPC();

            // Add bed to build menu in Misc tab
            WorkerBed.RegisterWorkerBed();

            // Add chests to build menu in Misc tab
            WorkerSupplyChest.RegisterWorkerChest();
            WorkerDepositChest.RegisterWorkerChest();
        }
    }
}
