using Jotunn.Configs;

namespace WorkerNPC
{
    internal class BuildMenuConfig
    {
        public static string pieceTable = PieceTables.Hammer;
        public static string buildCategory = "Workers";
    }

    internal class PrefabNamesConfig
    {
        public static string bedPrefabName = "bed";
        public static string chestPrefabName = "piece_chest_wood";
        public static string npcPrefabName = "Dverger";
    }

    internal class BedConfig
    {
        public static string description = "A bed for workers to rest in. Spawns a worker NPC when placed.";
    }

    internal class SupplyChestConfig
    {
        public static string displayName = "Supply Chest";
        public static string description = "Workers withdraw items from this chest.";
        public static string customName = "worker_supply_chest";
    }

    internal class DepositChestConfig
    {
        public static string displayName = "Deposit Chest";
        public static string description = "Workers deposit items into this chest. Must be assigned to an NPC.";
        public static string customName = "worker_deposit_chest";
    }

    internal class FuelWorkerConfig
    {
        public static string displayName = "Fuel Worker NPC";
        public static string customName = "fuel_worker";
        public static float jobInterval = 10f;
        public static float searchRadius = 10f;
        public static int maxInventorySize = 50;
    }

    internal class BuildingRepairWorkerConfig
    {
        public static string displayName = "Building Repair Worker NPC";
        public static string customName = "building_repair_worker";
        public static float jobInterval = 10f;
        public static float searchRadius = 10f;
    }
}
