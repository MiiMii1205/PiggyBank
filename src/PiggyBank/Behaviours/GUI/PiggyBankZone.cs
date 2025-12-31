using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiggyBank.Behaviours.GUI;

public class PiggyBankZone : UIWheelSlice, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
    public RawImage image;
    private PiggyBankScreen piggyBankScreen;
    private PiggyBankReference piggyBank;

    private ItemSlot itemSlot;

    private int cookedAmount;
    private bool hasItem;
    private bool isPiggyBankZone;
    public byte piggyBankSlot { get; private set; }

    private bool canInteract
    {
        get
        {
            if (isPiggyBankExit)
            {
                return true;
            }          
            
            if (isPiggyBankPickup && piggyBank.TryGetPiggyBankItem(out var pg) && pg.itemState != ItemState.Held)
            {
                return true;
            }

            if (isPiggyBankZone)
            {
                return true;
            }

            if (!hasItem)
            {
                if (Character.localCharacter.data.currentItem != null)
                {
                    return Character.localCharacter.data.currentItem.UIData.canBackpack;
                }

                return false;
            }

            return true;
        }
    }

    public bool isPiggyBankExit => piggyBankSlot == byte.MaxValue;
    public bool isPiggyBankPickup => piggyBankSlot == (byte.MaxValue - 1);

    public struct ZoneData : IEquatable<ZoneData>
    {
        public int slotID;
        public bool isPiggyBankZone;
        public PiggyBankReference piggyBankReference;

        public bool isPiggyBankExit;
        public bool isPiggyBankPickup;

        public bool Equals(ZoneData other)
        {
            if (isPiggyBankExit == other.isPiggyBankExit)
            {
                return slotID == other.slotID;
            }
            else if (isPiggyBankPickup == other.isPiggyBankPickup)
            {
                return slotID == other.slotID;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is ZoneData other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(isPiggyBankExit, isPiggyBankPickup, piggyBankReference, slotID);
        }
    }


    private void UpdateInteractable()
    {
        button.interactable = canInteract;
    }

    public void InitItemSlot(PiggyBankScreen piggyBankScreen)
    {
        piggyBankSlot = 0;

        Item item = null;

        if (!Plugin.IsBankFree())
        {
            var itemData = Plugin.BankedItemData;
            itemSlot = new ItemSlot(0);
            itemSlot.SetItem(itemData.prefab, itemData.data);
            item = itemSlot.prefab;
        }

        SetItemIcon(item, itemSlot.data);
        UpdateInteractable();
    }


    private void SetItemIcon(Item iconHolder, ItemInstanceData itemInstanceData)
    {
        if (iconHolder == null)
        {
            if(image != null)
            {
                image.enabled = false;
            }
            hasItem = false;
        }
        else
        {
            if (image != null)
            {
                image.enabled = true;
                image.texture = iconHolder.UIData.GetIcon();
            }
            
            hasItem = true;
        }

        UpdateCookedAmount(iconHolder, itemInstanceData);
    }

    private void UpdateCookedAmount(Item item, ItemInstanceData itemInstanceData)
    {
        IntItemData value;

        if (item == null || itemInstanceData == null)
        {
            cookedAmount = 0;
            image.color = Color.white;
        }
        else if (itemInstanceData.TryGetDataEntry<IntItemData>(DataEntryKey.CookedAmount, out value) &&
                 cookedAmount != value.Value)
        {
            image.color = Color.white;
            image.color = ItemCooking.GetCookColor(value.Value);
            cookedAmount = value.Value;
        }
    }

    public void InitExitPiggyBank(PiggyBankReference piggyBank, PiggyBankScreen piggyBankScreen)
    {
        piggyBankSlot = byte.MaxValue;
        UpdateInteractable();
    }

    public void Hover()
    {
        if (canInteract)
        {
            ZoneData sliceData = default(ZoneData);
            sliceData.isPiggyBankExit = isPiggyBankExit;
            sliceData.isPiggyBankZone = isPiggyBankZone;
            sliceData.isPiggyBankPickup = isPiggyBankPickup;
            sliceData.piggyBankReference = piggyBank;
            sliceData.slotID = piggyBankSlot;
            ZoneData sliceData2 = sliceData;
            piggyBankScreen.Hover(sliceData2);
        }
    }

    public void Dehover()
    {
        ZoneData sliceData = default(ZoneData);
        sliceData.isPiggyBankExit = piggyBankSlot == byte.MaxValue;
        sliceData.isPiggyBankZone = isPiggyBankZone;
        sliceData.isPiggyBankPickup = piggyBankSlot == (byte.MaxValue - 1);
        sliceData.piggyBankReference = piggyBank;
        sliceData.slotID = piggyBankSlot;
        ZoneData sliceData2 = sliceData;
        piggyBankScreen.Dehover(sliceData2);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Dehover();
    }

    public void InitPickupPiggyBank(PiggyBankReference piggyBank, PiggyBankScreen piggyBankScreen)
    {
        piggyBankSlot = byte.MaxValue - 1;
        UpdateInteractable();
    }
}