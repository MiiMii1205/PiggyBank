using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zorro.Core;

namespace PiggyBank.Behaviours.GUI;

public class PiggyBankScreen: UIWheel
{
    public PiggyBankZone piggyZone;
    public PiggyBankZone pickupZone;

    public TextMeshProUGUI chosenItemText;

    public Optionable<PiggyBankZone.ZoneData> chosenZone;

    public PiggyBankReference piggyBank;

    public RawImage currentlyHeldItem;
    public Image invalidItemIndicator;
    private int currentlyHeldItemCookedAmount;

    private void Awake()
    {
        chosenItemText.font = Plugin.DarumaDropOne;
        chosenItemText.lineSpacing = -50f;
        chosenItemText.fontSharedMaterial =
            chosenItemText.fontMaterial = chosenItemText.material = Plugin.DarumaDropOneShadowMaterial;
    }

    public void InitWheel(PiggyBankReference pg)
    {
        piggyBank = pg;
        chosenZone = Optionable<PiggyBankZone.ZoneData>.None;
        chosenItemText.text = "";
        
        piggyZone.InitItemSlot(pg,this);
        
        base.gameObject.SetActive(value: true);
        
        pickupZone.InitPickupPiggyBank(pg, this);
        
        if (Character.localCharacter.data.currentItem != null &&
            Character.localCharacter.data.currentItem is not PiggyBankController &&
            Character.localCharacter.data.currentItem.UIData.canBackpack)
        {
            currentlyHeldItem.texture = Character.localCharacter.data.currentItem.UIData.GetIcon();
            UpdateCookedAmount(Character.localCharacter.data.currentItem);
            currentlyHeldItem.enabled = true;
        }
        else
        {
            UpdateCookedAmount(null);
            currentlyHeldItem.enabled = false;
        }

        invalidItemIndicator.enabled = !Plugin.IsBankItemValid;
        
    }

    private void UpdateCookedAmount(Item item)
    {
        IntItemData value;
        if (item == null || item.data == null)
        {
            currentlyHeldItemCookedAmount = 0;
            currentlyHeldItem.color = Color.white;
        } else if (item.data.TryGetDataEntry(DataEntryKey.CookedAmount, out value) &&
                   currentlyHeldItemCookedAmount != value.Value)
        {
            currentlyHeldItem.color = Color.white;
            currentlyHeldItem.color = ItemCooking.GetCookColor(value.Value);
            currentlyHeldItemCookedAmount = value.Value;
            
        }
    }

    public override void Update()
    {
        if (!Character.localCharacter.input.interactIsPressed)
        {
            Choose();
            PiggyBankUIManager.instance.ClosePîggyBankScreen();
            return;
        }

        if (piggyBank.locationTransform != null &&
            Vector3.Distance(piggyBank.locationTransform.position, Character.localCharacter.Center) > 6f)
        {
            PiggyBankUIManager.instance.ClosePîggyBankScreen();
            return;
            
        }

        if (chosenZone.IsSome && !chosenZone.Value.isPiggyBankPickup && !piggyZone.image.enabled)
        {
            currentlyHeldItem.transform.position  = Vector3.Lerp(currentlyHeldItem.transform.position,
                piggyZone.transform.GetChild(0).GetChild(0).position, Time.deltaTime * 20f);
        }
        else
        {
            currentlyHeldItem.transform.localPosition = Vector3.Lerp(currentlyHeldItem.transform.localPosition,
                Vector3.zero, Time.deltaTime * 20f);

        }
        
        base.Update();
    }

    public void Choose()
    {
        if (!chosenZone.IsSome)
        {
            return;
        }

        Plugin.Log.LogDebug($"Chose zone {chosenZone.Value.slotID}");

        Item? item = null;
        if (chosenZone.Value.isPiggyBankPickup)
        {
            if (chosenZone.Value.piggyBankReference.TryGetPiggyBankItem(out var pg) && pg.itemState != ItemState.Held)
            {
                pg.PickUpPiggyBank(Character.localCharacter);
            }

        }  else if (chosenZone.Value.isDepositZone)
        {
            TryDeposit();
        }
        else if (!Plugin.IsBankFree && Plugin.WithdrawFromBank(out item))
        {

            item = item ?? throw new NullReferenceException(nameof(item));
            
            item.Interact(Character.localCharacter);

            Plugin.ClearBank();
        }
        else if (Character.localCharacter.data.currentItem)
        {
            TryDeposit();
        }
    }

    private void TryDeposit()
    {
        if (this.piggyBank.TryGetPiggyBankItem(out var piggyBank))
        {
            piggyBank.Deposit(Character.localCharacter);
        }
    }

    public void Hover(PiggyBankZone.ZoneData zoneData)
    {
        if (zoneData.isPiggyBankPickup)
        {
            if (zoneData.piggyBankReference.TryGetPiggyBankItem(out var pg) && pg.itemState != ItemState.Held)
            {
                chosenItemText.text = LocalizedText.GetText("CARRY").Replace("#", pg.GetItemName());
                chosenZone = Optionable<PiggyBankZone.ZoneData>.Some(zoneData);
            }
            else
            {
                chosenItemText.text = "";
                chosenZone = Optionable<PiggyBankZone.ZoneData>.None;
            }

            return;
        }

        if (zoneData.isDepositZone)
        {
            var currentItem = Character.localCharacter.data.currentItem;
            
            if (currentItem != null)
            {
                chosenItemText.text = LocalizedText.GetText("DEPOSITITEM").Replace("#", currentItem.GetItemName());
                chosenZone = Optionable<PiggyBankZone.ZoneData>.Some(zoneData);
            }
            else
            {
                chosenItemText.text = "";
                chosenZone = Optionable<PiggyBankZone.ZoneData>.None;
            }

            return;
        }
        
        bool flag = false;

        if (Plugin.IsBankFree && Character.localCharacter.data.currentItem)
        {
            if (Character.localCharacter.data.currentItem)
            {
                chosenItemText.text = LocalizedText.GetText("DEPOSITITEM").Replace("#",
                    Character.localCharacter.data.currentItem.GetItemName());
                flag = true;
            }
        }
        else
        {
            var data = Plugin.BankedItemData;

            Item? prefab = data.prefab;
            
            if (prefab != null)
            {
                chosenItemText.text = LocalizedText.GetText("WITHDRAWITEM").Replace("#", prefab.GetItemName(data.data));
                flag = true;
            }
        }

        if (flag)
        {
            chosenZone = Optionable<PiggyBankZone.ZoneData>.Some(zoneData);
        }
        
    }

    public void Dehover(PiggyBankZone.ZoneData zoneData)
    {
        if (chosenZone.IsSome && chosenZone.Value.Equals(zoneData))
        {
            chosenItemText.text = "";
            chosenZone = Optionable<PiggyBankZone.ZoneData>.None;
        }
    }

    public override void TestSelectSliceGamepad(Vector2 gamepadVector)
    {
        float num = 0f;
        PiggyBankZone piggyBankZone = null;
        
        if (!(gamepadVector.sqrMagnitude < 0.5f))
        {
            float num2 = Vector3.Angle(gamepadVector, pickupZone.GetUpVector());
            
            if (piggyBankZone == null || num2 < num)
            {
                piggyBankZone = pickupZone;
                num = num2;
            }

            num2 = Vector3.Angle(gamepadVector, piggyZone.GetUpVector());
            
            if (piggyBankZone == null || num2 < num)
            {
                piggyBankZone = piggyZone;
                num = num2;
            }
            
        }

        if (piggyBankZone != null)
        {
            EventSystem.current.SetSelectedGameObject(piggyBankZone.button.gameObject);
            piggyBankZone.Hover();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            Dehover(chosenZone.Value);
        }
    }
}
