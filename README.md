# PiggyBank #

A persistent item bank that lets you save **one item between runs**.

---

## 📦 What does this mod do? ##

**PiggyBank** adds a legendary piggy bank item to **PEAK** that allows you to store **one item persistently**.
Once deposited, the item stays available across future runs until it's withdrawn.

## 🐷 The Piggy Bank ##

<p><img src="https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/piggybank.png?raw=true" height="256" width="256"/></p>

This is a fragile porcelain piggy bank item that you can encounter during your runs.

<dl>
<dt>Weights</dt>
<dd>1 + whatever is inside</dd>
<dt>Rarity</dt>
<dd>Legendary</dd>
<dt>Spawns at</dt>
<dd>Any luggage</dd>
<dt>Tags</dt>
<dd>None</dd>
</dl>

## 🔁 How it works ##

Whenever you find a piggy bank, you can do one of two things:

- [_Deposit_](#-deposit-an-item) an item
- [_Withdraw_](#-withdraw-an-item) a stored item

![Piggy Bank UI](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/itemDeposit1.png?raw=true)

You can also pick up the piggy bank by using the hand icon at the bottom of the screen.

Each player has their own personal piggy bank storage. Stored items are **not shared** between players.

## ➕ Deposit an Item ##

If you don't already have a stored item, you can deposit the item you're currently holding.

1. Open the piggy bank while holding your item
2. Place it inside
3. Done! It's now saved to your persistent bank

![Piggy Bank Deposit](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/itemDeposit2.png?raw=true)

As long as it's not withdrawn, it will then stay even after the run has ended.

> [!NOTE]
> Only items that can be placed in a backpack can be deposited. Piggy banks themselves cannot be deposited.

## ➖ Withdraw an Item ##

To withdraw your stored item:

1. Open **any** piggy banks like a backpack
2. Take your stored item out

![Piggy Bank Withdraw](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/itemWithdraw.png?raw=true)

> [!TIP]
> When you have a stored item, you can see its name by hovering the piggy bank. No need to open it just to check.
> ![Stored item name](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/storedItemName.png?raw=true)
> <br/>
> It will also show in your inventory and/or your backpack when you hover above it
> <br/>
> ![Stored item name inventory](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/inventoryStoredItemName.png?raw=true)
> ![Stored item name backpack](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/backpackStoredItemName.png?raw=true)

Alternatively, you can **break** a piggy bank by throwing it at something hard or dropping it from high above. This will
release your stored item at a distance.

<p>
<mark>
⚠️ <b>Warning</b>: Remember that physics exists. Ceramic shards hurt, and your actions may have consequences.
</mark>
</p>

## ❓ FAQ ##

## Can I deposit an item from another mod? ##

Yes! Any item that can be placed in a backpack can be deposited (except **piggy banks**, of course!)

## HELP! My item turned into a Red Crispberry! What's happening?!? ##

This happens when PiggyBank cannot identify your stored item.

It usually happens when:

- Your stored item **comes from a mod**;
- That mod is **disabled** and/or **missing**.

Note that your stored item is **not lost yet**. PiggyBank just doesn't know what kind of item it is.

If PiggyBank doesn't recognize your stored item, a **warning icon (⚠️)** will appear next to the piggy bank slot when
you open it.

![invalid item marker](https://github.com/MiiMii1205/PiggyBank/blob/master/imgs/invalidItem.png?raw=true)

As long as you **don't withdraw** it, your stored item should be back whenever you re-enable/install that mod.

<p>
<mark>
❌ <b>Important</b>: If you withdraw your item while the mod is missing, <b>the original item will be permanently lost</b>.
</mark>
</p>

... but if you don't really care, you can definitely take it out. Enjoy the free Crispberry!

## I play with many different friend groups with different mod setups. Is there a way to deal with that? ##

**Yes!**
You can configure PiggyBank to have different stored items per mod profile.

To do that, set **`Piggy Bank Scope`** to `PER_PROFILE`.

> [!TIP]
> You can change your setting in your mod manager or by using a mod
> like [ModConfig](https://thunderstore.io/c/peak/p/PEAKModding/ModConfig/).
> If you want to use your mod manager, you will need to boot the game with the mod enabled first to create the config
> file.

This is especially useful when playing with different groups or mod lists.

## Is it safe to leave a piggy bank behind? ##

**Yes!**
The piggy bank will **not** withdraw your stored item unless you deliberately:

- **Throw** it
- **Drop** it from a high place.

So if you trip on it or push it without picking it up, it **won't withdraw any item at all**.

## Can I store multiple items in the bank? ## 

**No**, only **one item** can be stored at a time per players.

Even if you get multiple piggy banks, they'll **ALL** be synced and contain your stored item.

You can share a piggy bank with others, but everyone still only gets **one item slot**.

## Can we cook it? ##

Ehm, no? Nothing is stopping you. Just don’t say you weren’t warned.

## Where's the stored item data file? ##

The stored item is saved as a binary file named `.peakpiggybank`.
Its location depends on your **`Piggy Bank Scope`** setting:

- `GLOBAL` (default): the file is located in the same folder as `peak.exe`, so usually in your steam
  library.
- `PER_PROFILE`: the file is located inside the `BepInEx` directory of your current profile. You can use the
  Thunderstore Mod Manager to open your profile folder.

To edit it, you might need a HEX editor... It's not officially supported, so you'll be on your own.

## I found an item that doesn't get stored correctly. Can you fix it? ##

If you find any bugs or incompatible mods,
please [open an issue in the repo](https://github.com/MiiMii1205/PiggyBank/issues/new) with as much detail as possible.
We'll respond as soon as possible.