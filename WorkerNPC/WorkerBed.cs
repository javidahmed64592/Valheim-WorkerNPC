﻿using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace WorkerNPC
{
    internal static class WorkerBed
    {
        static string displayName = "Worker Bed";
        static string description = "A bed for workers to rest in. Spawns a worker NPC when placed.";
        static string prefabName = "bed";
        static string customName = "worker_bed";
        static string pieceTable = GlobalConfig.pieceTable;
        static string buildCategory = GlobalConfig.buildCategory;

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
        string prefabName = "Dverger";
        string customName = "worker_npc";

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
            if (dvergrPrefab == null)
            {
                dvergrPrefab = PrefabManager.Instance.CreateClonedPrefab(customName, prefabName);
            }

            if (dvergrPrefab == null)
            {
                Jotunn.Logger.LogError("Dvergr NPC prefab not found!");
                return;
            }

            if (dvergrPrefab.GetComponent<NPCBehaviour>() == null)
            {
                dvergrPrefab.AddComponent<NPCBehaviour>();
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
    }
}
