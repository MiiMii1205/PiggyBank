using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PEAKLib.Core;
using PEAKLib.Items;
using Photon.Pun;
using PiggyBank.Behaviours;
using PiggyBank.Behaviours.GUI;
using PiggyBank.Data;
using PiggyBank.Patchers;
using pworld.Scripts.Extensions;
using TMPro;
using Unity.Collections;
using Unity.Services.Lobbies.Http;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core.Serizalization;
using Object = UnityEngine.Object;

namespace PiggyBank;

[BepInAutoPlugin]
[BepInDependency(ItemsPlugin.Id)]
[BepInDependency(CorePlugin.Id)]
public partial class Plugin : BaseUnityPlugin
{
    private static bool _bankedItemValid = true;
    private static int _bankedItemCarryWeight;

    private static bool _isDirty = true;

    private static Item? _bankedItem;
    private static ItemInstanceData? _bankedItemData;

    public static GameObject PiggyScreenPrefab = null!;

    private const string PIGGY_BAMK_FILE_NAME = ".peakpiggybank";

    private static ConfigEntry<PiggyBankScopes> _itemScope = null!;
    internal static ManualLogSource Log { get; private set; } = null!;


    private static TMP_FontAsset? _darumaFontAsset;

    public static TMP_FontAsset DarumaDropOne
    {
        get
        {
            if (_darumaFontAsset == null)
            {
                var assets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                _darumaFontAsset = assets.FirstOrDefault(fontAsset =>
                    fontAsset.faceInfo.familyName == "Daruma Drop One"
                );

                Log.LogInfo("Daruma Drop One font found!");
            }

            return _darumaFontAsset!;
        }
    }

    private static Material? _darumaShadowMaterial;

    public static Material DarumaDropOneShadowMaterial
    {
        get
        {
            if (_darumaShadowMaterial == null)
            {
                _darumaShadowMaterial = ThrowHelper.ThrowIfArgumentNull(Object.Instantiate(DarumaDropOne.material));

                _darumaShadowMaterial.EnableKeyword("UNDERLAY_ON");
                _darumaShadowMaterial.SetFloat("_UnderlayDilate", 1f);
                _darumaShadowMaterial.SetFloat("_UnderlayOffsetY", -0.7f);
                _darumaShadowMaterial.SetFloat("_UnderlaySoftness", 1f);
                _darumaShadowMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.1960784f));
                
                Log.LogInfo("Shadow material for Critial Hit indicator was successfully generated!");
            }

            return _darumaShadowMaterial!;
        }
    }
    
    public static (Item? prefab, ItemInstanceData? data) BankedItemData
    {
        get
        {
            RefreshIfDirty();
            return (_bankedItem, _bankedItemData);
        }
    }

    public static int BankedItemCarryWeight => _bankedItemCarryWeight;

    public static bool IsBankFree { get; private set; }

    public static string PiggyBankPath =>
        _itemScope.Value switch
        {
            PiggyBankScopes.GLOBAL => Paths.GameRootPath,
            PiggyBankScopes.PER_PROFILE => Paths.BepInExRootPath,
            _ => throw new ArgumentOutOfRangeException(nameof(_itemScope))
        };

    
    public static bool IsBankItemValid
    {
        get
        {
            RefreshIfDirty();
            return IsBankFree || _bankedItemValid;
        }
    }

    private static void RefreshIfDirty()
    {
        if (_isDirty)
        {
            if (IsBankFree)
            {
                _bankedItem = null;
                _bankedItemData = null;
                _bankedItemCarryWeight = 0;
            }
            else
            {
                _bankedItem = FetchBankedItemPrefab();
                _bankedItemData = FetchBankedItemData();
                _bankedItemCarryWeight = _bankedItem.carryWeight;
            }

            _isDirty = false;
        }
    }

    private void Awake()
    {
        Log = Logger;

        _itemScope = Config.Bind(
            "General", "Piggy Bank scope", PiggyBankScopes.GLOBAL, """
                                                                   The scope of stored items.
                                                                   You can use this config to have different stored items per profile or keep it globally available.
                                                                   """);

        // We just ensure that the data we have in memory is valid
        _itemScope.SettingChanged += (_, _) => { _isDirty = true; };

        this.LoadBundleWithName("piggybank.peakbundle", bundle =>
        {
            var piggyScreen = bundle.LoadAsset<GameObject>("PiggyScreen.prefab");

            var pigScr = piggyScreen.AddComponent<PiggyBankScreen>();
            
            pigScr.maxCursorDistance = 190;
            pigScr.chosenItemText = piggyScreen.transform.Find(@"SelectedItemName").GetComponent<TextMeshProUGUI>();
            pigScr.currentlyHeldItem = piggyScreen.transform.Find(@"HeldItem").GetComponent<RawImage>();
            pigScr.invalidItemIndicator = piggyScreen.transform.Find(@"InvalidItemIndicator").GetComponent<Image>();

            ReplaceShaders([
                "Scouts/UI",
                "TextMeshPro/Distance Field"
            ], piggyScreen);
            
            var pigz = piggyScreen.transform.Find(@"PiggyZone").gameObject.GetOrAddComponent<PiggyBankZone>();

            pigz.image = pigz.transform.Find(@"Segment").GetComponentInChildren<RawImage>();
            pigz.button = pigz.GetComponent<Button>();

            var picz = piggyScreen.transform.Find(@"PickUpZone").gameObject.GetOrAddComponent<PiggyBankZone>();

            picz.button = picz.GetComponent<Button>();

            pigScr.piggyZone = pigz;
            pigScr.pickupZone = picz;

            PiggyScreenPrefab = pigScr.gameObject;

            var piggyR = bundle.LoadAsset<GameObject>("Piggybank.prefab");
            var piggyBreakVFX = bundle.LoadAsset<GameObject>("VFX_PiggyShards.prefab");

            var piggyC = piggyR.GetOrAddComponent<PiggyBankController>();
            var piggyBrk = piggyR.GetOrAddComponent<PiggyBankBreakable>();
            var piggyImpk = piggyR.GetOrAddComponent<PiggyBankImpactSFX>();
            
            var piggyItemImpact = piggyR.GetOrAddComponent<ItemImpactSFX>();

            var piggyCook = piggyR.GetOrAddComponent<ItemCooking>();

            piggyBrk.breakOnCollision = true;
            piggyBrk.minBreakVelocity = 15f;

            piggyBrk.ragdollCharacterOnBreak = true;
            piggyBrk.pushForce = 2;
            piggyBrk.wholeBodyPushForce = 1;

            piggyBrk.instantiateNonItemOnBreak =
            [
                piggyBreakVFX
            ];

            piggyImpk.impact = piggyItemImpact.impact;
            piggyImpk.disallowInHands = piggyItemImpact.disallowInHands;
            piggyImpk.vel = piggyItemImpact.vel;
            piggyImpk.velMult = piggyItemImpact.velMult;

            piggyCook.additionalCookingBehaviors = piggyCook.additionalCookingBehaviors.AddToArray(
                new CookingBehaviourPiggyBankRelease()
                {
                    cookedAmountToTrigger = 1
                });

            piggyImpk.impact_empty =
            [
                bundle.LoadAsset<SFX_Instance>("SFXI Piggy Bank Hold Empty.asset")
            ];

            piggyImpk.impact_full =
            [
                bundle.LoadAsset<SFX_Instance>("SFXI Piggy Bank Hold Full.asset")
            ];

            Destroy(piggyItemImpact);

            piggyC.defaultPos = new Vector3(0f, -0.3f, 1.25f);
            piggyC.defaultForward = new Vector3(0f, 0f, 1f);
            piggyC.mainRenderer = piggyR.transform.Find(@"Piggybank/Piggybank").GetComponent<Renderer>();
            piggyC.addtlRenderers = [piggyR.transform.Find(@"Piggybank/Piggybank_eyes").GetComponent<Renderer>()];

            piggyC.offsetLuggageSpawn = true;
            piggyC.offsetLuggageRotation = new Vector3(281.467f, 0f, -180f);
            piggyC.offsetLuggagePosition = new Vector3(0f, -0.47f, 0.444f * 1.5f);

            piggyC.openRadialMenuTime = 0f;

            // Carry Weight will change based on the banked item
            piggyC.carryWeight = 1;

            piggyC.mass = 50;
            // piggyC.UIData.icon = Texture2D.whiteTexture;

            piggyC.UIData = new Item.ItemUIData
            {
                itemName = "Piggy Bank",
                icon = bundle.LoadAsset<Texture2D>("piggybank.png"),
                hasAltIcon = false,
                hasColorBlindIcon = false,
                altIcon = null,
                hasMainInteract = false,
                mainInteractPrompt = "INSPECT",
                hasSecondInteract = false,
                secondaryInteractPrompt = null,
                hasScrollingInteract = false,
                scrollInteractPrompt = null,
                canDrop = true,
                canPocket = true,
                canBackpack = true,
                canThrow = true,
                isShootable = false,
                hideFuel = true,
                iconPositionOffset = default,
                iconRotationOffset = default,
                iconScaleOffset = 0
            };

            piggyC.showUseProgress = true;
            
            ReplaceShaders([
                "GD/Face Cards"
            ], piggyR);
            
            ReplaceShaders([
                "W/Peak_Standard",
                "SmokeParticle"
            ], piggyBreakVFX);
            
            bundle.Mod.RegisterContent();

            Log.LogInfo("Piggy bank is loaded!");
        });

        IsBankFree = !File.Exists(Path.Join(PiggyBankPath, ".peakpiggybank"));

        AddLocalizedTextCsv();

        new Harmony(Id).PatchAll(typeof(PiggyBankPatcher));

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void ReplaceShaders(List<string> shaderNames, GameObject go)
    {
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            ReplaceAllShaderInRenderer(shaderNames, renderer);
        }
    }

    private void ReplaceAllShaderInRenderer(List<string> shaderNames, Renderer ren)
    {
        foreach (var shaderName in shaderNames)
        {
            
            var shader = Shader.Find(shaderName);

            if (shader == null)
            {
                Log.LogWarning(
                    $": Shader {shaderName} was not found."
                );
                continue;
            }
            
            foreach (var mat in ren.sharedMaterials)
            {

                ReplaceShader(shader, mat);
            }

            foreach (var mat in ren.materials)
            {
                ReplaceShader(shader, mat);
            }
            
        }
        
    }

    private static void ReplaceShader(Shader shader, Material mat)
    {
        if (mat.shader.name != shader.name)
        {
            return;
        }

        mat.shader = shader;
    }

    private void OnDestroy()
    {
        Harmony.UnpatchID(Id);

        Log.LogInfo($"Plugin {Name} unloaded!");
    }

    private void AddLocalizedTextCsv()
    {
        using var reader = new StreamReader(Path.Join(Path.GetDirectoryName(Info.Location),
            "PiggyBankLocalizedText.csv"));

        var currentLine = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (line == null)
            {
                break;
            }

            currentLine++;

            List<string> valList = new List<string>(CSVReader.SplitCsvLine(line));

            var locName = valList.Deque();

            var endline = valList.Pop();

            if (endline != "ENDLINE")
            {
                Log.LogError($"Invalid localization at line {currentLine}");
            }

            if (locName != "CURRENT_LANGUAGE")
            {
                LocalizedText.mainTable[locName.ToUpper()] = valList;
                Log.LogDebug($"Added localization of {locName.ToUpper()}");
            }
        }

        Log.LogDebug($"Added {currentLine - 1} localizations");
    }

    private static Item FindItemPrefab(string itemName, ushort itemId)
    {
        Item? itemPrefab = null;
        var possibleItems = ItemDatabase.Instance.itemLookup.Values
            .Where(t => t.UIData.itemName == itemName).ToList();

        itemPrefab = possibleItems.Count() switch
        {
            1 => possibleItems.First(),
            // The las fallback. If we can't find the item using its ID then we'll give up
            > 1 => possibleItems.First((t => t.itemID == itemId)),
            _ => itemPrefab
        };

        if (itemPrefab == null)
        {
            Log.LogWarning($"No item named {itemName} were found. Defaulting to a Red Crispberry...");
            _bankedItemValid = false;
            // If we cant find the item, then it means that it came from a mod that isn't available anymore... Defaulting to a red crispberry...
            if (!ItemDatabase.TryGetItem(4, out itemPrefab))
            {
                throw new DeserializationException(
                    $"No valid item were found while fetching the piggybank");
            }
        }
        else
        {
            _bankedItemValid = true;
        }

        return itemPrefab;
    }

    private static Item FetchBankedItemPrefab()
    {
        BinaryDeserializer deserializer =
            new BinaryDeserializer(File.ReadAllBytes(Path.Join(PiggyBankPath, PIGGY_BAMK_FILE_NAME)), Allocator.Temp);

        var itemName = deserializer.ReadString(Encoding.Unicode);
        var itemId = deserializer.ReadUShort();

        var itemPrefab = FindItemPrefab(itemName, itemId);

        _bankedItemCarryWeight = itemPrefab.carryWeight;

        return itemPrefab;
    }

    private static ItemInstanceData FetchBankedItemData()
    {
        BinaryDeserializer deserializer =
            new BinaryDeserializer(File.ReadAllBytes(Path.Join(PiggyBankPath, PIGGY_BAMK_FILE_NAME)), Allocator.Temp);

        deserializer.ReadString(Encoding.Unicode);
        deserializer.ReadUShort();

        var data = new ItemInstanceData();

        data.Deserialize(deserializer);

        return data;
    }

    public static bool WithdrawFromBank(out Item? item, bool giveToLocalPlayer = true, Vector3 spawnPosition = default)
    {
        var piggyBankPath = Path.Join(PiggyBankPath, PIGGY_BAMK_FILE_NAME);

        using var deserializer =
            new BinaryDeserializer(File.ReadAllBytes(piggyBankPath), Allocator.Temp);

        try
        {
            var itemName = deserializer.ReadString(Encoding.Unicode);
            var itemID = deserializer.ReadUShort();

            var itemPrefab = FindItemPrefab(itemName, itemID);

            var itemGo = PhotonNetwork.Instantiate($"0_Items/{itemPrefab.name}", spawnPosition, Quaternion.identity);

            var itemScript = itemGo.GetComponent<Item>();

            var dat = new ItemInstanceData();

            if (_bankedItemValid)
            {
                dat.Deserialize(deserializer);
                
                // Because we'll instantiate a brand new item, the item data's instance ID needs to be cleared
                dat.data[DataEntryKey.InstanceID] = DataEntryValue.GetNewFromValue(1);
            }
            
            itemScript.SetItemInstanceDataRPC(dat);

            if (giveToLocalPlayer)
            {
                itemGo.GetComponent<Item>()
                    .RequestPickup(Character.localCharacter.GetComponent<PhotonView>());
            }

            Log.LogInfo(
                $"Retreived item {itemScript.GetName()} from {piggyBankPath}.");

            _isDirty = false;

            item = itemScript;

            IsBankFree = true;

            return true;
        }
        catch (Exception e)
        {
            Log.LogError(e);
            item = null;
            return false;
        }
    }

    public static void ClearBank()
    {
        File.Delete(Path.Join(PiggyBankPath, PIGGY_BAMK_FILE_NAME));
    }

    private static void RefreshBankedItemData(Item storedItemInstante, ItemInstanceData data)
    {
        var itemPrefab = FindItemPrefab(storedItemInstante.UIData.itemName, storedItemInstante.itemID);

        _bankedItem = itemPrefab;
        _bankedItemData = data;

        _bankedItemCarryWeight = itemPrefab.carryWeight;

        _isDirty = false;
    }

    public static bool DepositItemToBank(Item item, ItemInstanceData data)
    {
        using var serializer = new BinarySerializer();

        var piggyBankPath = Path.Join(PiggyBankPath, PIGGY_BAMK_FILE_NAME);

        try
        {
            serializer.WriteString(item.UIData.itemName, Encoding.Unicode);
            serializer.WriteUshort(item.itemID);

            data.Serialize(serializer);

            File.WriteAllBytes(piggyBankPath, serializer.buffer.ToArray());

            Log.LogInfo($"Serialized item {item.GetName()} @ {piggyBankPath}.");

            _isDirty = true;

            IsBankFree = false;

            // Refresh stored values in case the player changes their mind
            RefreshBankedItemData(item, data);

            return true;
        }
        catch (Exception e)
        {
            Log.LogError(e);
            return false;
        }
    }
}