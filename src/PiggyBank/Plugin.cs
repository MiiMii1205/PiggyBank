using BepInEx;
using BepInEx.Logging;
using PEAKLib.Core;
using PEAKLib.Items;

namespace PiggyBank;

[BepInAutoPlugin]
[BepInDependency(ItemsPlugin.Id)]
[BepInDependency(CorePlugin.Id)]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        this.LoadBundleWithName("piggybank.peakbundle", bundle =>
        {
            // TODO: Add piggy bank controller
            
            bundle.Mod.RegisterContent();
            
            Log.LogInfo("Piggy bank is loaded!");
        });
        
        Log.LogInfo($"Plugin {Name} is loaded!");

    }
}