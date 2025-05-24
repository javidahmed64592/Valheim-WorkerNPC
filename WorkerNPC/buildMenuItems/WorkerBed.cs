using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    internal static class WorkerBed
    {
        static string description = BedConfig.description;
        static string prefabName = BedConfig.prefabName;
        static string customName = BedConfig.customName;
        static string pieceTable = BuildMenuConfig.pieceTable;
        static string buildCategory = BuildMenuConfig.buildCategory;

        public static void RegisterWorkerBed()
        {
            void CreateBed()
            {
                GameObject bedObject = GetBaseBed();
                FuelWorkerBed fuelWorkerBed = bedObject.AddComponent<FuelWorkerBed>();

                PieceConfig bed = new PieceConfig
                {
                    Name = fuelWorkerBed.displayName,
                    Description = description,
                    PieceTable = pieceTable,
                    Category = buildCategory
                };

                PieceManager.Instance.AddPiece(new CustomPiece(bedObject, false, bed));
                Jotunn.Logger.LogInfo($"Registered {fuelWorkerBed.displayName} under {buildCategory} category.");
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

    internal class WorkerBedBehaviour : MonoBehaviour
    {
        internal virtual string displayName { get { return WorkerNPCConfig.displayName; } }
        internal virtual string npcName { get { return WorkerNPCConfig.customName; } }
        GameObject workerNPC;

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
                if (child.name.Contains(npcName))
                {
                    npcFound = true;
                    break;
                }
            }

            if (!npcFound)
            {
                SpawnWorkerNPC();
            }
        }

        private void OnDestroy()
        {
            RemoveWorkerNPC();
        }

        private void SpawnWorkerNPC()
        {
            GameObject dvergrPrefab = PrefabManager.Instance.GetPrefab(npcName);
            workerNPC = Instantiate(dvergrPrefab, transform.position + Vector3.up, Quaternion.identity);
            workerNPC.transform.parent = transform;
        }

        private void RemoveWorkerNPC()
        {
            if (workerNPC != null)
            {
                ZNetScene.instance.Destroy(workerNPC);
            }
        }
    }

    internal class FuelWorkerBed : WorkerBedBehaviour
    {
        internal override string displayName { get { return FuelWorkerConfig.displayName; } }
        internal override string npcName { get { return FuelWorkerConfig.customName; } }
    }

    internal class BuildingRepairWorkerBed : WorkerBedBehaviour
    {
        internal override string displayName { get { return BuildingRepairWorkerConfig.displayName; } }
        internal override string npcName { get { return BuildingRepairWorkerConfig.customName; } }
    }
}
