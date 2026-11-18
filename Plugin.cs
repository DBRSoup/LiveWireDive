using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Pigeon.Movement;
using System;
using System.Reflection;
using UnityEngine;
namespace LiveWireDive;

[MycoMod(null, ModFlags.IsClientSide)]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public const string PLUGIN_GUID = "DBR.LiveWireDive";
    public const string PLUGIN_NAME = "LiveWireDive";
    public const string PLUGIN_VERSION = "1.3.0";

    private void Awake()
    {


        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded");

        Harmony harmony = new(PLUGIN_GUID);
        try
        {
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            Logger.LogError((object)("Error applying patches: " + ex.Message));
        }
    }

    private static readonly AccessTools.FieldRef<VoltaicGrenade, float> SelfGrenadeExplosionTimer = AccessTools.FieldRefAccess<VoltaicGrenade, float>("selfGrenadeExplosionTimer");
    private static readonly AccessTools.FieldRef<VoltaicGrenade, Sprite> LiveWireIcon = AccessTools.FieldRefAccess<VoltaicGrenade, Sprite>("livewireIcon");
    private static readonly AccessTools.FieldRef<VoltaicGrenade, float> SelfGrenadeTimer = AccessTools.FieldRefAccess<VoltaicGrenade, float>("selfGrenadeTimer");


    [HarmonyPatch(typeof(NoseDiveUpgrade), nameof(NoseDiveUpgrade.TriggerNoseDive))]
    public static class TriggerNoseDivePatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (Player.LocalPlayer.ThrowableGear is VoltaicGrenade grenade)
            {
                VoltaicGrenade.Data grenadeData = grenade.GrenadeData; 
                if (grenadeData.selfGrenadeDuration > 0)
                {
                    float selfGrenadeTimer = SelfGrenadeTimer(grenade);
                    if (selfGrenadeTimer >= 0)
                    {
                        SelfGrenadeTimer(grenade) = selfGrenadeTimer + grenadeData.selfGrenadeExplodeInterval - SelfGrenadeExplosionTimer(grenade);
                        SelfGrenadeExplosionTimer(grenade) = grenadeData.selfGrenadeExplodeInterval;
                        grenade.Player.UpdateStackDisplay(typeof(UpgradeProperty_VoltaicGrenade_Livewire), TextBlocks.GetString("Livewire", 0), LiveWireIcon(grenade), grenadeData.selfGrenadesApplied, grenadeData.selfGrenadeDuration - SelfGrenadeTimer(grenade));
                    }
                }
            }
        }
    }

}
