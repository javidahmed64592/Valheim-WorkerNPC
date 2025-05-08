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

            return clonedBed;
        }
    }

    public class WorkerBedBehavior : MonoBehaviour
    {
        private GameObject workerNPC;
        public static string prefabName = "Dverger";
        public static string customName = "worker_npc";
        public static string npcActiveKey = $"{customName}_active";

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

            bool npcExists = bedView.GetZDO().GetBool(npcActiveKey, false);

            if (npcExists)
            {
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
                else
                {
                    Jotunn.Logger.LogWarning("Worker NPC marked as active but missing—re-spawning.");
                }
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

            ZNetView bedView = GetComponent<ZNetView>();
            if (bedView != null && bedView.IsValid())
            {
                bedView.GetZDO().Set(npcActiveKey, true);
                Jotunn.Logger.LogInfo("Worker NPC spawned and marked as active.");
            }
        }

        private void RemoveWorkerNPC()
        {
            if (workerNPC != null)
            {
                ZNetScene.instance.Destroy(workerNPC);
                Jotunn.Logger.LogInfo("Worker NPC removed.");
            }
            else
            {
                Jotunn.Logger.LogWarning("Worker NPC was missing but marked as active—resetting state.");
            }

            ZNetView bedView = GetComponent<ZNetView>();
            if (bedView != null && bedView.IsValid())
            {
                bedView.GetZDO().Set(npcActiveKey, false);
            }
        }
    }
}
