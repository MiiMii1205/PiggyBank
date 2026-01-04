using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiggyBank.Behaviours.GUI;

public class PiggyBankZone : UIWheelSlice, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
    public RawImage image;

    private int cookedAmount;
    private bool hasItem;
    private bool isDepositZone;

    private ItemSlot itemSlot;
    private PiggyBankReference piggyBank;
    private PiggyBankScreen piggyBankScreen;
    public byte piggyBankSlot { get; private set; }

    private bool canInteract
    {
        get
        {
            if (isPiggyBankPickup)
            {
                return true;
            }

            if (isDepositZone)
            {
                return true;
            }

            if (!hasItem)
            {
                if (Character.localCharacter.data.currentItem != null)
                {
                    return Character.localCharacter.data.currentItem.UIData.canBackpack &&
                           Character.localCharacter.data.currentItem is not PiggyBankController;
                }

                return false;
            }

            return true;
        }
    }

    public bool isPiggyBankPickup => piggyBankSlot == byte.MaxValue;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Dehover();
    }


    private void UpdateInteractable()
    {
        button.interactable = canInteract;
    }

    public void InitItemSlot(PiggyBankReference pg, PiggyBankScreen pbs)
    {
        SharedInit(pg, pbs);
        piggyBankSlot = 0;

        Item item = null;
        itemSlot ??= new ItemSlot(piggyBankSlot);

        if (!Plugin.IsBankFree)
        {
            var itemData = Plugin.BankedItemData;

            itemSlot.SetItem(itemData.prefab, itemData.data);
            item = itemSlot.prefab;
        }

        SetItemIcon(item, itemSlot.data);
        UpdateInteractable();
    }

    private void SharedInit(PiggyBankReference pgRef, PiggyBankScreen screen)
    {
        piggyBank = pgRef;
        piggyBankScreen = screen;

        if (true)
        {
            if (piggyBankSlot == byte.MaxValue)
            {
                base.gameObject.SetActive(value: true);
            }

            SetItemIcon(null, null);
        }
    }

    private void SetItemIcon(Item iconHolder, ItemInstanceData itemInstanceData)
    {
        if (iconHolder == null)
        {
            if (image != null)
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

    private void UpdateCookedAmount(Item? item, ItemInstanceData? itemInstanceData)
    {
        IntItemData value;

        if (item == null || itemInstanceData == null)
        {
            cookedAmount = 0;
            if (image != null)
            {
                image.color = Color.white;
            }
        }
        else if (itemInstanceData.TryGetDataEntry(DataEntryKey.CookedAmount, out value) &&
                 cookedAmount != value.Value)
        {
            if (image != null)
            {
                image.color = Color.white;
                image.color = ItemCooking.GetCookColor(value.Value);
            }

            cookedAmount = value.Value;
        }
    }

    public void Hover()
    {
        if (canInteract)
        {
            ZoneData zoneData = default(ZoneData);
            zoneData.isDepositZone = isDepositZone;
            zoneData.isPiggyBankPickup = isPiggyBankPickup;
            zoneData.piggyBankReference = piggyBank;
            zoneData.slotID = piggyBankSlot;
            ZoneData zoneData2 = zoneData;
            piggyBankScreen.Hover(zoneData2);
        }
    }

    public void Dehover()
    {
        ZoneData zoneData = default(ZoneData);
        zoneData.isDepositZone = isDepositZone;
        zoneData.isPiggyBankPickup = piggyBankSlot == byte.MaxValue;
        zoneData.piggyBankReference = piggyBank;
        zoneData.slotID = piggyBankSlot;
        ZoneData zoneData2 = zoneData;
        piggyBankScreen.Dehover(zoneData2);
    }

    public void InitPickupPiggyBank(PiggyBankReference pg, PiggyBankScreen pbs)
    {
        piggyBankSlot = byte.MaxValue;
        SharedInit(pg, pbs);
        UpdateInteractable();
    }

    public struct ZoneData : IEquatable<ZoneData>
    {
        public bool isDepositZone;
        public bool isPiggyBankPickup;

        public PiggyBankReference piggyBankReference;

        public byte slotID;

        public bool Equals(ZoneData other)
        {
            if (isPiggyBankPickup == other.isPiggyBankPickup)
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
            return HashCode.Combine(isPiggyBankPickup, piggyBankReference, slotID);
        }
    }
}