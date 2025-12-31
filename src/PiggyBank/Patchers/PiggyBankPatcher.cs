using HarmonyLib;
using PiggyBank.Behaviours;
using UnityEngine;
using UnityEngine.UI.Extensions;

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

            var gui = Object.Instantiate(Plugin.piggyScreenPrefab, man.transform);
            gui.SetActive(false);
        }
    }
}