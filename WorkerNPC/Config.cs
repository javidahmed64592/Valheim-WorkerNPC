using Jotunn.Configs;

namespace WorkerNPC
{
    internal class BuildMenuConfig
    {
        public static string pieceTable = PieceTables.Hammer;
        public static string buildCategory = "Workers";
    }

    internal class BedConfig
    {
        public static string displayName = "Worker Bed";
        public static string description = "A bed for workers to rest in. Spawns a worker NPC when placed.";
        public static string prefabName = "bed";
        public static string customName = "worker_bed";
    }

    internal class SupplyChestConfig
    {
        public static string displayName = "Supply Chest";
        public static string description = "Workers withdraw items from this chest.";
        public static string prefabName = "piece_chest_wood";
        public static string customName = "worker_supply_chest";
    }

    internal class DepositChestConfig
    {
        public static string displayName = "Deposit Chest";
        public static string description = "Workers deposit items into this chest. Must be assigned to an NPC.";
        public static string prefabName = "piece_chest_wood";
        public static string customName = "worker_deposit_chest";
    }

    internal class WorkerNPCConfig
    {
        public static string displayName = "Worker NPC";
        public static string prefabName = "Dverger";
        public static string customName = "worker_npc";
    }

    internal class FuelWorkerConfig
    {
        public static string customName = "fuel_worker";
        public static int maxInventorySize = 50;
        public static float searchRadius = 10f;
        public static float searchInterval = 10f;
    }
}
