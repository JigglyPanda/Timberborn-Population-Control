using System;
using HarmonyLib;
using Timberborn.GameDistricts;
using Timberborn.GameDistrictsMigration;

namespace PopulationControl
{
    // Every migration (manual buttons AND automatic) ends up in one of these MigrateTo methods,
    // so patching them is enough to control who leaves.

    [HarmonyPatch(typeof(AdultsDistributorTemplate), nameof(AdultsDistributorTemplate.MigrateTo))]
    internal static class AdultsMigratePatch
    {
        private static bool Prefix(AdultsDistributorTemplate __instance, DistrictCenter target, int amount) =>
            MigrationSelector.HandleMigrateTo(__instance, PopulationGroup.Adults, target, amount);
    }

    [HarmonyPatch(typeof(ChildrenDistributorTemplate), nameof(ChildrenDistributorTemplate.MigrateTo))]
    internal static class ChildrenMigratePatch
    {
        private static bool Prefix(ChildrenDistributorTemplate __instance, DistrictCenter target, int amount) =>
            MigrationSelector.HandleMigrateTo(__instance, PopulationGroup.Children, target, amount);
    }

    [HarmonyPatch(typeof(BotsDistributorTemplate), nameof(BotsDistributorTemplate.MigrateTo))]
    internal static class BotsMigratePatch
    {
        private static bool Prefix(BotsDistributorTemplate __instance, DistrictCenter target, int amount) =>
            MigrationSelector.HandleMigrateTo(__instance, PopulationGroup.Bots, target, amount);
    }

    /// <summary>
    /// The game's automatic migration always goes through MigrationCoordinator.Migrate (private).
    /// Manual buttons call PopulationDistributor.MigrateTo directly. This marks the automatic path.
    /// </summary>
    [HarmonyPatch(typeof(MigrationCoordinator), "Migrate")]
    internal static class AutomaticMigrationMarker
    {
        private static void Prefix() => MigrationSelector.AutomaticDepth++;

        private static Exception Finalizer(Exception __exception)
        {
            MigrationSelector.AutomaticDepth--;
            return __exception;
        }
    }
}
