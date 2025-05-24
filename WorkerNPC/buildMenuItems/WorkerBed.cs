using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace WorkerNPC
{
    internal static class WorkerBed
    {
        static string description = BedConfig.description;
        static string prefabName = PrefabNamesConfig.bedPrefabName;
        static string pieceTable = BuildMenuConfig.pieceTable;
        static string buildCategory = BuildMenuConfig.buildCategory;

        public static void RegisterWorkerBed()
        {
            List<Type> bedTypes = new List<Type>
            {
                typeof(FuelWorkerBed),
                typeof(BuildingRepairWorkerBed)
            };

            void CreateBeds()
            {

                foreach (Type bedType in bedTypes)
                {
                    string npcName = bedType.Name;
                    GameObject bedObject = GetBaseBed(npcName);
                    WorkerBedBehaviour bedBehavior = (WorkerBedBehaviour)bedObject.AddComponent(bedType);

                    PieceConfig bed = new PieceConfig
                    {
                        Name = bedBehavior.displayName,
                        Description = description,
                        PieceTable = pieceTable,
                        Category = buildCategory
                    };

                    PieceManager.Instance.AddPiece(new CustomPiece(bedObject, false, bed));
                    Jotunn.Logger.LogInfo($"Registered {bedBehavior.displayName} under {buildCategory} category.");
                }

                PrefabManager.OnVanillaPrefabsAvailable -= CreateBeds;
            }

            PrefabManager.OnVanillaPrefabsAvailable += CreateBeds;
        }

        private static GameObject GetBaseBed(string npcName)
        {
            GameObject clonedBed = PrefabManager.Instance.CreateClonedPrefab($"{npcName}_bed", prefabName);

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
                    UnityEngine.Object.Destroy(bedComponent);
                }
            }

            return clonedBed;
        }
    }

    internal class WorkerBedBehaviour : MonoBehaviour
    {
        internal virtual string displayName { get { return ""; } }
        internal virtual string npcName { get { return ""; } }
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
                workerNPC = null;
            }
        }

        public void OnNPCDeath()
        {
            if (workerNPC != null)
            {
                RemoveWorkerNPC();
                SpawnWorkerNPC();
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
