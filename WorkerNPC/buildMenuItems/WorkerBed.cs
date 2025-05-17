using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    internal static class WorkerBed
    {
        static string displayName = BedConfig.displayName;
        static string description = BedConfig.description;
        static string prefabName = BedConfig.prefabName;
        static string customName = BedConfig.customName;
        static string pieceTable = BuildMenuConfig.pieceTable;
        static string buildCategory = BuildMenuConfig.buildCategory;

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

    internal class WorkerBedBehavior : MonoBehaviour
    {
        GameObject workerNPC;
        string customName = FuelWorkerConfig.customName;

        private void Start()
        {
            Piece piece = GetComponent<Piece>();
            if (piece == null || !piece.IsPlacedByPlayer())
            {
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
                Jotunn.Logger.LogInfo("Worker NPC already exists — skipping spawn.");
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
    }
}
