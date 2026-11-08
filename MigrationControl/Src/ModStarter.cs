using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace MigrationControl
{
    public class ModStarter : IModStarter
    {
        public const string HarmonyId = "Panda.MigrationControl";

        public void StartMod(IModEnvironment modEnvironment)
        {
            new Harmony(HarmonyId).PatchAll(typeof(ModStarter).Assembly);
            Debug.Log("[MigrationControl] Patches applied.");
        }
    }
}
