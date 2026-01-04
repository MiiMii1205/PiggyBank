using PiggyBank.Behaviours.GUI;
using UnityEngine;

namespace PiggyBank.Behaviours;

public class PiggyBankUIManager : MonoBehaviour
{
    public bool piggyBankActive => piggyBankScreen.gameObject.activeSelf;

    public PiggyBankScreen piggyBankScreen;

    public static PiggyBankUIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void ClosePîggyBankScreen()
    {
        Plugin.Log.LogDebug("Close piggy screen");
        Character.localCharacter.data.usingBackpackWheel = false;
        piggyBankScreen.gameObject.SetActive(value: false);
    }

    public void OpenPîggyBankScreen(PiggyBankReference backpackReference)
    {
        if (!GUIManager.instance.wheelActive && !GUIManager.instance.windowBlockingInput)
        {
            Character.localCharacter.data.usingBackpackWheel = true;
            piggyBankScreen.InitWheel(backpackReference);
        }
    }
}