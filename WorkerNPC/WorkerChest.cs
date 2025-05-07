using Jotunn.Managers;
using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;
using static Piece;

namespace WorkerNPC
{
    public static class WorkerChest
    {
        public static void RegisterWorkerChest()
        {
            Main.logger.LogInfo("Attempting to register Worker Chest...");

            PrefabManager.OnPrefabsRegistered += () =>
            {
                GameObject baseChest = PrefabManager.Instance.GetPrefab("piece_chest_wood");

                if (baseChest == null)
                {
                    Main.logger.LogError("Base chest prefab not found!");
                    return;
                }

                CustomPiece workerChest = new CustomPiece(baseChest, true, new PieceConfig
                {
                    Name = "Worker Chest",
                    PieceTable = "_HammerPieceTable",
                    Category = PieceCategory.Misc.ToString()
                });

                PieceManager.Instance.AddPiece(workerChest);
                Main.logger.LogInfo("Registered Worker Chest under Misc category.");
            };
        }
    }
}
