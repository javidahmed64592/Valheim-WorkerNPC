using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

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

            PrefabManager.OnVanillaPrefabsAvailable += () =>
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
            };
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

        private void Start()
        {
            Piece piece = GetComponent<Piece>();
            if (piece == null || !piece.IsPlacedByPlayer())
            {
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
            GameObject dvergrPrefab = PrefabManager.Instance.GetPrefab(prefabName);

            if (dvergrPrefab == null)
            {
                Jotunn.Logger.LogError("Dvergr NPC prefab not found!");
                return;
            }

            workerNPC = Instantiate(dvergrPrefab, transform.position + Vector3.up, Quaternion.identity);
            Jotunn.Logger.LogInfo("Worker NPC spawned successfully.");
        }

        private void RemoveWorkerNPC()
        {
            if (workerNPC != null)
            {
                Destroy(workerNPC);
                Jotunn.Logger.LogInfo("Worker NPC removed.");
            }
        }
    }
}
