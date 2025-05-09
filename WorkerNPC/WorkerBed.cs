using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using JetBrains.Annotations;

namespace WorkerNPC
{
    public static class WorkerBed
    {
        public static string displayName = "Worker Bed";
        public static string description = "A bed for workers to rest in. Spawns a worker NPC when placed.";
        public static string pieceTable = PieceTables.Hammer;
        public static string prefabName = "bed";
        public static string customName = "worker_bed";
        public static string buildCategory = "Workers";

        public static void RegisterWorkerBed()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            void CreateBed()
            {
                GameObject bedObject = GetBaseBed();
                bedObject.AddComponent<WorkerBedBehavior>();

                PieceConfig bed = new PieceConfig
                {
                    Name = displayName,
                    Description = description,
                    PieceTable = pieceTable,
                    Category = buildCategory
                };

                PieceManager.Instance.AddPiece(new CustomPiece(bedObject, false, bed));
                Jotunn.Logger.LogInfo($"Registered {displayName} under {buildCategory} category.");
                PrefabManager.OnVanillaPrefabsAvailable -= CreateBed;
            }

            PrefabManager.OnVanillaPrefabsAvailable += CreateBed;
        }

        private static GameObject GetBaseBed()
        {
            GameObject clonedBed = PrefabManager.Instance.CreateClonedPrefab(customName, prefabName);

            if (clonedBed == null)
            {
                Jotunn.Logger.LogError("Base bed prefab not found!");
                return null;
            }
            
            else
            {
                Bed bedComponent = clonedBed.GetComponent<Bed>();
                if (bedComponent != null)
                {
                    Object.Destroy(bedComponent);
                }
            }

            return clonedBed;
        }
    }

    public class WorkerBedBehavior : MonoBehaviour
    {
        private GameObject workerNPC;
        public static string prefabName = "Dverger";
        public static string customName = "worker_npc";

        // Inventory
        public static string inventoryKey = "worker_npc_inventory";
        public static int maxInventorySize = 50;

        private void Start()
        {
            Piece piece = GetComponent<Piece>();
            if (piece == null || !piece.IsPlacedByPlayer())
            {
                return;
            }

            ZNetView bedView = GetComponent<ZNetView>();
            if (bedView == null || !bedView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing—cannot track Worker NPC state.");
                return;
            }

            bool npcFound = false;
            foreach (Transform child in transform)
            {
                if (child.name.Contains(customName))
                {
                    npcFound = true;
                    break;
                }
            }

            if (npcFound)
            {
                Jotunn.Logger.LogInfo("Worker NPC already exists—skipping spawn.");
                return;
            }

            SpawnWorkerNPC();
        }

        private void OnDestroy()
        {
            RemoveWorkerNPC();
        }

        private void SpawnWorkerNPC()
        {
            GameObject dvergrPrefab = PrefabManager.Instance.GetPrefab(customName);
            if (dvergrPrefab == null)
            {
                dvergrPrefab = PrefabManager.Instance.CreateClonedPrefab(customName, prefabName);
            }

            if (dvergrPrefab == null)
            {
                Jotunn.Logger.LogError("Dvergr NPC prefab not found!");
                return;
            }

            workerNPC = Instantiate(dvergrPrefab, transform.position + Vector3.up, Quaternion.identity);
            workerNPC.transform.parent = transform;
        }

        private void RemoveWorkerNPC()
        {
            if (workerNPC != null)
            {
                ZNetScene.instance.Destroy(workerNPC);
                Jotunn.Logger.LogInfo("Worker NPC removed.");
            }
        }

        private void AddItemToInventory(string itemName, int amount)
        {
            ZNetView bedView = GetComponent<ZNetView>();
            if (bedView == null || !bedView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing—cannot modify Worker NPC inventory.");
                return;
            }

            string inventoryData = bedView.GetZDO().GetString(inventoryKey, $"{itemName}:0");
            int currentAmount = int.Parse(inventoryData.Split(':')[1]);

            int newAmount = currentAmount + amount;
            bedView.GetZDO().Set("worker_npc_inventory", $"{itemName}:{newAmount}");

            Jotunn.Logger.LogInfo($"Added {amount} {itemName} to Worker NPC's inventory (Total: {newAmount}).");
        }

        private bool UseItemFromInventory(string itemName, int amount)
        {
            ZNetView bedView = GetComponent<ZNetView>();
            if (bedView == null || !bedView.IsValid())
            {
                Jotunn.Logger.LogError("ZNetView missing—cannot modify Worker NPC inventory.");
                return false;
            }

            string inventoryData = bedView.GetZDO().GetString(inventoryKey, $"{itemName}:0");
            int currentAmount = int.Parse(inventoryData.Split(':')[1]);

            if (currentAmount < amount)
            {
                Jotunn.Logger.LogWarning($"Worker NPC does not have enough {itemName} (Has: {currentAmount}, Needs: {amount}).");
                return false;
            }

            int newAmount = currentAmount - amount;
            bedView.GetZDO().Set("worker_npc_inventory", $"{itemName}:{newAmount}");

            Jotunn.Logger.LogInfo($"Used {amount} {itemName} from Worker NPC's inventory (Remaining: {newAmount}).");
            return true;
        }
    }
}
