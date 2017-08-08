using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;
using Harmony;

namespace AssignAnimalBeds
{
    /*[StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
        static string varName = "pawn";
        static Type anonType = typeof(RestUtility).GetNestedTypes(BindingFlags.NonPublic).First(t => t.HasAttribute<CompilerGeneratedAttribute>() && t.GetField(varName, bf) != null);
        static MethodInfo anonMethod = anonType.GetMethods(bf).First(); // assuming first for now...

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.assignanimalbeds");
            //Log.Message("anonMethod: " + anonMethod.Name);
            harmony.Patch(anonMethod, null, null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Transpiler))));
        }

        private static MethodInfo MI_getMedical = typeof(Building_Bed).GetProperty(nameof(Building_Bed.Medical)).GetGetMethod();
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int i;
            for (i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];
                if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i].operand == MI_getMedical)
                {
                    yield return instructionList[++i];
                    while (instructionList[++i].opcode != OpCodes.Brfalse) { } // eat up instructions
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0); // force the break (which is really a continue in the statement)
                    yield return instructionList[i];
                }
            }
        }
    }*/
}
