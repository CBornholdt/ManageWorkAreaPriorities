using System;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WorkAreaPriorityManager
{
	[StaticConstructorOnStartup]
	static public class Harmony_Patches
	{
		static Harmony_Patches ()
		{
			HarmonyInstance instance = HarmonyInstance.Create("cbornholdt.rimworld.workareaprioritymanager");
			//HarmonyInstance.DEBUG = true;
			instance.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(RimWorld.JobGiver_Work))]
	[HarmonyPatch("TryIssueJobPackage")]
	static public class JobGiver_Work_Patches
	{
		static IEnumerable<CodeInstruction> Transpiler (IEnumerable<CodeInstruction> instructions)
		{
			var helper = AccessTools.Method (typeof(JobGiver_Work_Patches), "Helper");

			List<CodeInstruction> codes = instructions.ToList ();
			for (int i = 0; i < codes.Count; i++) {
				yield return codes [i];

				if(codes [i].opcode == OpCodes.Isinst && codes [i].operand == typeof(WorkGiver_Scanner)) {
					yield return new CodeInstruction (OpCodes.Ldarg_1);	//WorkGiver_Scanner, Pawn on stack
					yield return new CodeInstruction (OpCodes.Call, helper); //Consume 2, leave WorkGiver_Scanner
				}
			}
		}

		static WorkGiver_Scanner Helper(WorkGiver_Scanner scanner, Pawn pawn)
		{
			if (scanner == null || 
				(pawn.Map.GetComponent<AreaPriorityManager> ().Prioritizations [scanner.def]?.disabled ?? true) == true)
				return scanner;
			return new WorkGiver_Scanner_AreaPriorityWrapper (scanner, pawn.Map);
		}
	}

	[HarmonyPatch(typeof(RimWorld.Dialog_ManageAreas))]
	[HarmonyPatch("DoWindowContents")]
	[HarmonyAfter("cbornholdt.rimworld.compositeareamanager")]
	static class Dialog_ManageAreas_DoWindowContents
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//	MethodInfo listingEnd = AccessTools.Method (typeof(Verse.Listing), "End");
			MethodInfo setColumnWidth = AccessTools.Method (typeof(Listing), "set_ColumnWidth");
			MethodInfo buttonHelper = AccessTools.Method (typeof(Dialog_ManageAreas_DoWindowContents), "ButtonHelper");
			MethodInfo listingEnd = AccessTools.Method (typeof(Listing), "End");

			List<CodeInstruction> codes = instructions.ToList ();

			for (int i = 0; i < codes.Count; i++) {
				if(codes[i].opcode == OpCodes.Callvirt && codes[i].operand == listingEnd) {
					yield return new CodeInstruction(OpCodes.Dup);	//Copy arguemnt to listingEnd, Listing_Standard on stack
					yield return new CodeInstruction (OpCodes.Ldarg_0);	//Listing_Standard on stack, Dialog_ManageAreas on statck
					yield return new CodeInstruction(OpCodes.Call, buttonHelper); //Consume 2
				}	
				yield return codes [i];
			}
		}

		static void ButtonHelper(Listing_Standard listing, Dialog_ManageAreas dialog) {
			FieldInfo mapField = AccessTools.Field (typeof(Dialog_ManageAreas), "map");
			Map map = (Map)mapField.GetValue (dialog);
			if (listing.ButtonText ("ManageWorkAreaPriorities".Translate (), null)) {
				map.GetComponent<AreaPriorityManager> ().LaunchDialog_ManageWorkAreaPriorities ();
				dialog.Close (false);
			}
		}
	}

	[HarmonyPatch(typeof(Verse.AreaUtility))]
    [HarmonyPatch("MakeAllowedAreaListFloatMenu")]
    static class AreaUtility_MakeAllowedAreaListFloatMenu
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getWindowStack = AccessTools.Method(typeof(Verse.Find), "get_WindowStack");
            MethodInfo listAddHelper = AccessTools.Method(typeof(AreaUtility_MakeAllowedAreaListFloatMenu), "ListAddHelper");

            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand == getWindowStack) {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);   //Map on stack
                    yield return new CodeInstruction(OpCodes.Ldloc_1);  //Leave List<FloatMenuOption>, Map on stack
                    yield return new CodeInstruction(OpCodes.Call, listAddHelper);  //Consume 1
                }
                yield return codes[i];
            }
        }

        static void ListAddHelper(Map map, List<FloatMenuOption> list)
        {
            list.Add(new FloatMenuOption("ManageWorkAreaPriorities".Translate(),
                () => map.GetComponent<AreaPriorityManager>().LaunchDialog_ManageWorkAreaPriorities(),
                MenuOptionPriority.Low, null, null, 0, null, null));
        }
    }

	[HarmonyPatch(typeof(RimWorld.MainTabWindow_Work))]
	[HarmonyPatch("DoManualPrioritiesCheckbox")]
	static class MainTabWindow_Work__DoManualPrioritiesCheckbox_Patches
	{
		static void Postfix(MainTabWindow_Work __instance)
		{
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect = new Rect(5f, 40f, 140f, 30f);
			if (Widgets.ButtonText(rect, "WorkAreaPriorities".Translate()))
				Find.VisibleMap?.GetComponent<AreaPriorityManager>().LaunchDialog_ManageWorkAreaPriorities();
		}
	}
}
               