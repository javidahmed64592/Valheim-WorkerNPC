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
        public static string buildCategory = "Workers";

        public static void RegisterWorkerBed()
        {
            Jotunn.Logger.LogInfo($"Attempting to register {displayName}...");

            PrefabManager.OnVanillaPrefabsAvailable += () =>
            {
                GameObject bedObject = GetBaseBed();
                if (bedObject == null)
                {
                    Jotunn.Logger.LogError("Base bed prefab not found!");
                    return;
                }

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
            return PrefabManager.Instance.GetPrefab(prefabName);
        }
    }

    public class WorkerBedBehavior : MonoBehaviour
    {
        private GameObject workerNPC;
        public static string prefabName = "Dverger";

        private void Start()
        {
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

            workerNPC = Instantiate(dvergrPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
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
