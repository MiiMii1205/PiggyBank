# PiggyBank #

A persistent item bank that lets you save **one item between runs**.

---

## 📦 What does this mod do? ##

**PiggyBank** adds a legendary piggy bank item to PEAK that allows each player to store **one item persistently**. 
Once deposited, the item stays available across future runs until you withdraw it.

## 🐷 The Piggy Bank ##

<img src="https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/piggybank.png?raw=true" height="256" width="256"/>

This is a **legendary** porcelain piggy bank item that you can encounter in **any luggage**.

- Pretty light when empty;
- Gets heavier when full (based on whatever is stored inside);
- Can be carried in your inventory or in a backpack.

## 🔁 How it works ##

Whenever you find a piggy bank, you can do one of two things: 

- [_Deposit_](#-deposit-an-item) an item;
- [_Withdraw_](#-withdraw-an-item) a stored item.

Each player has their own personal piggy bank storage. Stored items are **not shared** between players.

## ➕ Deposit an Item ##

If you don't already have a stored item, you can deposit the item you're currently holding.

1. Open the piggy bank while holding your item;
2. Place it inside;
3. Done! It's now saved to your persistent bank.

As long as you don't withdraw, it will stay in your bank even after the run has ended.

## ➖ Withdraw an Item ##

To withdraw your stored item:

1. Open any piggy banks like a backpack;
2. Take your stored item out.

Alternatively, you can **break** a piggy bank by throwing it at something hard. This will release your stored item remotely.

<p>
<mark>
⚠️ <b>Warning</b>: Remember that physics exists. Ceramic shards hurt, and your actions may have consequences.
</mark>
</p>

## ❓ FAQ ##

## Can I deposit an item from another mod? ##

Yes! Any item that can be backpacked can be deposited (except **piggy banks**, of course!)

## HELP! My item turned into a Red Crispberry! What's happening?!? ##

This happens when PiggyBank cannot identify your stored item.

It usually happens when:

- Your stored item comes from a mod;
- That mod is disabled and/or missing.

Note that your stored item is **not lost yet**. PiggyBank just doesn't know what kind of item it is.

If PiggyBank doesn't recognize your stored item, a **warning icon (⚠️)** will appear next to the piggy bank slot when you open it.

As long as you don't withdraw it, your stored item should be back whenever you re-enable/install that mod.

<p>
<mark>
❌ <b>Important</b>: If you withdraw your item while the mod is missing, <b>the original item will be permanently lost</b>.
</mark>
</p>

... but if you don't really care, you can definitely take it out. Enjoy the free Crispberry!

## I play with many different friend groups with different mod setups. Is there a way to deal with that? ##

Yes!
You can change the mod's config to have different stored items per mod profile.

To do that, set **"Piggy Bank Scope"** to `PER_PROFILE`.

This is especially useful when playing with different groups or mod lists.

## Is it safe to leave a piggy bank behind? ##

Yes! Even if you were the last one to interact with it, it will **not** release your stored item unless you deliberately **throw** it.

That also means that if you trip on it or push it using another item, it **shouldn't release any item at all**.

## Can I store multiple items in the bank? ## 

No, only **one item** can be stored at a time per players.

Even if you get multiple piggy banks, they'll **ALL** contain your stored item.

You can share piggy banks, but everyone only gets one item slot.

## Can we cook it? ##

Ehm, no? Nothing is stopping you. Just don’t say you weren’t warned.

## Where are the stored item data file? ##

The stored item file is named `.peakpiggybank` and is saved as a binary file.
Its location depends on the **"Piggy Bank Scope"** setting:

- For `GLOBAL`, the file is located in the same folder as `peak.exe`, so usually inside your steam library. 
- For `PER_PROFILE`, the file is located inside the "BepInEx" directory of your current profile. You can use the Thunderstore Mod Manager to open your profile folder.

To edit it, you might need a HEX editor... But you're on your own!

## I found an item that doesn't get stored correctly. Can you fix it? ##

If you find any bugs or incompatible mods, please [open an issue in the repo](https://github.com/MiiMii1205/PiggyBank/issues/new) with as much detail as possible. We'll respond as soon as possible.