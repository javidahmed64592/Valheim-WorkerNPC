using UnityEngine;

namespace WorkerNPC
{
    internal class BuildingRepairWorker : WorkerNPCBase
    {
        internal override GameObject GetBaseNPC()
        {
            displayName = BuildingRepairWorkerConfig.displayName;
            customName = BuildingRepairWorkerConfig.customName;

            GameObject clonedNPC = base.GetBaseNPC();
            clonedNPC.name = customName;
            clonedNPC.AddComponent<BuildingRepairWorkerBehaviour>();
            return clonedNPC;
        }
    }

    internal class BuildingRepairWorkerBehaviour : NPCBehaviour
    {
        float searchRadius = BuildingRepairWorkerConfig.searchRadius;

        internal new void Start()
        {
            base.Start();
            jobInterval = BuildingRepairWorkerConfig.jobInterval;
        }

        internal override void DoJob()
        {
            base.DoJob();

            WearNTear[] buildings = FindNearbyResource<WearNTear>(searchRadius);

            foreach (WearNTear building in buildings)
            {
                if (building.GetHealthPercentage() < 1f)
                {
                    MoveTo(building.gameObject);
                    building.Repair();
                }
            }
        }
    }
}
