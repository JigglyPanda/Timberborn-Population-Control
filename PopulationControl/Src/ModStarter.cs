using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace PopulationControl
{
    public class ModStarter : IModStarter
    {
        public const string HarmonyId = "Panda.PopulationControl";

        public void StartMod(IModEnvironment modEnvironment)
        {
            new Harmony(HarmonyId).PatchAll(typeof(ModStarter).Assembly);
            Debug.Log("[PopulationControl] Patches applied.");
        }
    }
}
