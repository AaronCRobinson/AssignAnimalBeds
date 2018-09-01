using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Harmony;
using RimWorld;

namespace AssignAnimalBeds
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            HarmonyInstance h = HarmonyInstance.Create("WhyIsThat.AssignAnimalBeds");

            h.Patch(AccessTools.Method(typeof(PawnComponentsUtility), nameof(PawnComponentsUtility.CreateInitialComponents)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Postfix_CreateInitialComponents)));
        }

        public static void Postfix_CreateInitialComponents(Pawn pawn)
        {
            if (pawn.RaceProps.Animal && pawn.ownership == null)
                pawn.ownership = new Pawn_Ownership(pawn);
        }

    }
}
