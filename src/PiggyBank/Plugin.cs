using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Items;
using PiggyBank.Behaviours;
using PiggyBank.Behaviours.GUI;
using pworld.Scripts.Extensions;
using TMPro;
using Unity.Collections;
using Unity.Services.Lobbies.Http;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core.Serizalization;

namespace PiggyBank;

[BepInAutoPlugin]
[BepInDependency(ItemsPlugin.Id)]
[BepInDependency(CorePlugin.Id)]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private static bool isDirty = true;

    private static Item bankedItem;
    private static ItemInstanceData bankedItemData;

    public static GameObject piggyScreenPrefab;
    
    public static (Item prefab, ItemInstanceData data) BankedItemData
    {
        get
        {
            if (isDirty)
            {
                bankedItem = FetchBankedItemPrefab();
                bankedItemData = FetchBankedItemData();
                isDirty = false;
            }

            return (bankedItem, bankedItemData);
        }
    }

    private void Awake()
    {
        Log = Logger;

        this.LoadBundleWithName("piggybank.peakbundle", bundle =>
        {
            var piggyScreen = bundle.LoadAsset<GameObject>("PiggyScreen.prefab");

            var pigScr = piggyScreen.AddComponent<PiggyBankScreen>();

            pigScr.maxCursorDistance = 190;
            pigScr.chosenItemText = piggyScreen.transform.Find(@"SelectedItemName").GetComponent<TextMeshProUGUI>();
            pigScr.currentlyHeldItem = piggyScreen.transform.Find(@"HeldItem").GetComponent<RawImage>();

            var pigz = piggyScreen.transform.Find(@"PiggyZone").gameObject.GetOrAddComponent<PiggyBankZone>();
            
            pigz.image = pigz.transform.Find(@"Segment/Image").GetComponent<RawImage>();
            pigz.button = pigz.GetComponent<Button>();
            
            var extz = piggyScreen.transform.Find(@"ExitZone").gameObject.GetOrAddComponent<PiggyBankZone>();
            
            extz.button = extz.GetComponent<Button>();
            var picz = piggyScreen.transform.Find(@"PickUpZone").gameObject.GetOrAddComponent<PiggyBankZone>();
            
            picz.button = picz.GetComponent<Button>();

            pigScr.piggyZone = pigz;
            pigScr.exitZone = extz;
            pigScr.pickupZone = picz;

            piggyScreenPrefab = pigScr.gameObject;
            
            var piggyR = bundle.LoadAsset<GameObject>("Piggybank.prefab");

            var piggyC = piggyR.GetOrAddComponent<PiggyBankController>();

            piggyC.defaultPos = new Vector3(0f, -0.3f, 1.25f);
            piggyC.defaultForward = new Vector3(0f, 0f, 1f);
            
            piggyC.openRadialMenuTime = 0f;
            
            // Carry Weight will change based on the banked item
            piggyC.carryWeight = 1;
            
            piggyC.mass = 20;
            piggyC.UIData.icon = bundle.LoadAsset<Texture2D>("PiggyBank.png");

            piggyC.UIData.hasMainInteract = true;
            piggyC.UIData.mainInteractPrompt = "INSPECT";
            
            piggyC.showUseProgress = true;
            
            piggyC.UIData.canBackpack = true;
            piggyC.UIData.canPocket = true;
            piggyC.UIData.canDrop = true;
            piggyC.UIData.canThrow = true;
            
            piggyC.UIData.itemName = "Piggy Bank";
            
            var cardsShaders = Shader.Find("GD/Face Cards");

            if (cardsShaders == null)
            {
                Log.LogWarning(
                    $": Shader GD/Face Cards was not found."
                );
            }
            else
            {

                foreach (Renderer renderer in piggyR.GetComponentsInChildren<Renderer>())
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat.shader.name != cardsShaders.name)
                        {
                            continue;
                        }
                        
                        mat.shader = cardsShaders;
                    }

                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.shader.name != cardsShaders.name)
                        {
                            continue;
                        }
                        
                        mat.shader = cardsShaders;
                    }
                }
            }

            
            bundle.Mod.RegisterContent();

            Log.LogInfo("Piggy bank is loaded!");
        });

        AddLocalizedTextCsv();

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private static Item FetchBankedItemPrefab()
    {
        BinaryDeserializer deserializer =
            new BinaryDeserializer(File.ReadAllBytes(Path.Join(Paths.GameRootPath, ".peakpiggybank")), Allocator.Temp);

        var itemName = deserializer.ReadString(Encoding.Unicode);

        var itemPrefab = ItemDatabase.Instance.itemLookup.Values.First(item => item.UIData.itemName == itemName);

        if (itemPrefab == null)
        {
            // If we cant find the item, then it means that it came from a mod that isn't available anymore. We'll default to a red crispberry 
            if (!ItemDatabase.TryGetItem(0, out itemPrefab))
            {
                throw new DeserializationException("No valid item were found while fetching the piggybank");
            }
        }

        return itemPrefab;
    }

    private static ItemInstanceData FetchBankedItemData()
    {
        BinaryDeserializer deserializer =
            new BinaryDeserializer(File.ReadAllBytes(Path.Join(Paths.GameRootPath, ".peakpiggybank")), Allocator.Temp);

        deserializer.ReadString(Encoding.Unicode);

        var data = new ItemInstanceData();

        data.Deserialize(deserializer);

        return data;
    }

    public static bool WithdrawFromBank(out Item item)
    {
       using var deserializer =
            new BinaryDeserializer(File.ReadAllBytes(Path.Join(Paths.GameRootPath, ".peakpiggybank")), Allocator.Temp);

       try
       {
           var itemName = deserializer.ReadString(Encoding.Unicode);

           var itemPrefab = ItemDatabase.Instance.itemLookup.Values.First(item => item.UIData.itemName == itemName);

           if (itemPrefab == null)
           {
               // If we cant find the item, then it means that it came from a mod that isn't available anymore. We'll default to a red crispberry 
               if (!ItemDatabase.TryGetItem(0, out itemPrefab))
               {
                   throw new DeserializationException("No valid item were found while fetching the piggybank");
               }
           }

           var itemGo = GameObject.Instantiate(itemPrefab.gameObject);

           var itemScript = itemGo.GetComponent<Item>();

           itemScript.data.Deserialize(deserializer);

           Log.LogInfo(
               $"Retreived item {itemScript.GetName()} from {Path.Join(Paths.GameRootPath, ".peakpiggybank")}.");

           isDirty = false;

           item = itemScript;
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
        File.Delete(Path.Join(Paths.GameRootPath, ".peakpiggybank"));
    }

    public static bool DepositItemToBank(Item item, ItemInstanceData data)
    {
        using var serializer = new BinarySerializer();

        try
        {
            serializer.WriteString(item.UIData.itemName, Encoding.Unicode);

            data.Serialize(serializer);

            File.WriteAllBytes(Path.Join(Paths.GameRootPath, ".peakpiggybank"), serializer.buffer.ToArray());

            Log.LogInfo($"Serialized item {item.GetName()} @ {Path.Join(Paths.GameRootPath, ".peakpiggybank")}.");

            isDirty = true;

            return true;
        }
        catch (Exception e)
        {
            Log.LogError(e);
            return false;
        }
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


    public static bool IsBankFree()
    {
        return File.Exists(Path.Join(Paths.GameRootPath, ".peakpiggybank"));
    }
}