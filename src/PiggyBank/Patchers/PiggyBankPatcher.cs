using System;
using HarmonyLib;
using PiggyBank.Behaviours;
using PiggyBank.Behaviours.GUI;
using UnityEngine.UI.Extensions;
using Object = UnityEngine.Object;

namespace PiggyBank.Patchers;

public static class PiggyBankPatcher
{
    [HarmonyPatch(typeof(GUIManager), "Awake")]
    [HarmonyPostfix]
    public static void GUIAwakePostfix(GUIManager __instance)
    {
        var man = __instance.gameObject.GetOrAddComponent<PiggyBankUIManager>();

        if (man == null)
        {
            Plugin.Log.LogError($"Can't add PiggyBankUIManager to {__instance.character.characterName}'s GUI");
        }
        else
        {
            var gui = Object.Instantiate(Plugin.PiggyScreenPrefab, __instance.hudCanvas.transform);
            man.piggyBankScreen = gui.GetComponent<PiggyBankScreen>();
            gui.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(GUIManager), "wheelActive", MethodType.Getter)]
    [HarmonyPostfix]
    public static void GUIWheelActivePost(GUIManager __instance, ref bool __result)
    {
        // We do this for mouse and input capture
        __result = PiggyBankUIManager.instance.piggyBankActive || __result;
    }

    [HarmonyPatch(typeof(Item), "CarryWeight", MethodType.Getter)]
    [HarmonyPostfix]
    public static void CarryWeightPost(Item __instance, ref int __result)
    {
        if (__instance is PiggyBankController && !Plugin.IsBankFree)
        {
            // Just count the stored item's weight in the piggy bank too
            __result = __instance.carryWeight + Plugin.BankedItemCarryWeight + Ascents.itemWeightModifier;
        }
    }

    [HarmonyPatch(typeof(Item), "GetItemName")]
    [HarmonyPostfix]
    public static void ItemNamePost(Item __instance, ref string __result)
    {
        if (__instance is PiggyBankController && !Plugin.IsBankFree)
        {
            // Get the stored item name

            var bid = Plugin.BankedItemData;
            var prefab = bid.prefab ?? throw new NullReferenceException(nameof(bid.prefab));

            __result = $"{__result} ({prefab.GetItemName(bid.data)})";
        }
    }
}