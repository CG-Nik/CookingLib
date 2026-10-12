using System.Collections.Generic;
using Alta.Liquid;
using HarmonyLib;
using UnityEngine;

namespace CookingLib
{
    internal static class InfusionPatch
    {
        internal static void Apply(HarmonyLib.Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(LiquidContainerTrigger), nameof(LiquidContainerTrigger.OnTriggerEnter)),
                postfix: new HarmonyMethod(typeof(InfusionPatch), nameof(Postfix)));
        }

        static void Postfix(Collider other, ILiquidContainer ___thisContainer, Dictionary<Rigidbody, int> ___handledRigidbodies)
        {
            if (Infusions.All.Count == 0 || other == null)
                return;

            Rigidbody body = other.attachedRigidbody;
            if (body == null || !___handledRigidbodies.TryGetValue(body, out int entries) || entries != 1)
                return;

            Pickup pickup = body.GetComponent<Pickup>();
            ILiquidContainerInternal container = ___thisContainer as ILiquidContainerInternal;
            if (pickup == null || pickup.Item == null || container == null)
                return;

            LibInfusion infusion = Infusions.Find(pickup.Item, ___thisContainer.ContainedLiquid);
            if (infusion == null)
                return;

            container.SetContent(infusion.Output.CreateContent());
            if (infusion.Spec.ConsumeItem)
                pickup.Entity.Spawner.Destroy(pickup.Entity.PrefabRoot);
        }
    }
}
