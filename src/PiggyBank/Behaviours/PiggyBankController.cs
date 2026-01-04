using Photon.Pun;
using UnityEngine.Rendering;
using Zorro.Core;
using Zorro.Core.CLI;

namespace PiggyBank.Behaviours;

public class PiggyBankController: Item
{
    public float openRadialMenuTime = 0.25f;
    public bool holdOnFinish => false;

    public override void AddPhysics()
    {
        base.AddPhysics();
        rig.sleepThreshold = 0f;
    }

    public override void Interact(Character interactor)
    {
        if (this.itemState == ItemState.InBackpack)
        {
            base.Interact(interactor);
        }
        else
        {
            PiggyBankUIManager.instance.OpenPîggyBankScreen(PiggyBankReference.GetFromBackpackItem(this));
        }

    }

    public void ReleaseInteract(Character interactor)
    {
    }

    public void PickUpPiggyBank(Character interactor)
    {
        base.Interact(interactor);
    }
    
    private void DisableVisuals()
    {
        mainRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    private void EnableVisuals()
    {
        mainRenderer.shadowCastingMode = ShadowCastingMode.On;
    }
    
    private static bool HasSpace()
    {
        return Plugin.IsBankFree;
    }

    public override string GetInteractionText()
    {
        return LocalizedText.GetText("INSPECT");
    }
    
    [ConsoleCommand]
    public static void PrintBankedItems()
    {
        var data = Plugin.BankedItemData;
        Plugin.Log.LogInfo($"Slot: {data.prefab.gameObject.name}, data entries: {data.data.data.Count}");
    }

    public bool IsConstantlyInteractable(Character interactor)
    {
        return false;
    }

    public float GetInteractTime(Character interactor)
    {
        return openRadialMenuTime;
    }

    public void Interact_CastFinished(Character interactor)
    {
    }

    public void CancelCast(Character interactor)
    {
    }
    
    public void Deposit(Character interactor)
    {
        if (!interactor.data.currentItem || !HasSpace())
        {
            return;
        }

        var items = interactor.refs.items;
        
        if (items.currentSelectedSlot.IsNone)
        {
            Plugin.Log.LogError("Need item slot selected to stash item in piggy bank!");
            return;
        }

        var itemSlot = interactor.player.GetItemSlot(items.currentSelectedSlot.Value);
        
        if (itemSlot.IsEmpty())
        {
            Plugin.Log.LogError($"Item slot {itemSlot.itemSlotID} is empty!");
            return;
        }

        // Not needed since we don't need to replicate anything...
        
        // view.RPC("RPCAddItemToBackpack", RpcTarget.All, interactor.player.GetComponent<PhotonView>(),
        //     items.currentSelectedSlot.Value, backpackSlotID);
        
        if (Plugin.DepositItemToBank(itemSlot.prefab, itemSlot.data))
        {
            interactor.player.EmptySlot(items.currentSelectedSlot);

            if (items.currentSelectedSlot is {IsSome: true, Value: 250})
            {
                interactor.photonView.RPC("DestroyHeldItemRpc", RpcTarget.All);
            }
            else
            {
                items.EquipSlot(Optionable<byte>.None);
            }
            
        }
    
    }

}