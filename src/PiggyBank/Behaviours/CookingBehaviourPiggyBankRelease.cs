using System;

namespace PiggyBank.Behaviours;

public class CookingBehaviourPiggyBankRelease: AdditionalCookingBehavior
{
    public override void TriggerBehaviour(int cookedAmount)
    {
        if (this.itemCooking.item.holderCharacter && this.itemCooking.item.holderCharacter.IsLocal && !Plugin.IsBankFree && Plugin.WithdrawFromBank(out var i, true ))
        {
            i = i ?? throw new NullReferenceException(nameof(i));

            i.Interact(Character.localCharacter);

            Plugin.ClearBank();
        }
    }    
}