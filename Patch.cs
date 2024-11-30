﻿using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace Context_Menu_Lime_Mods
{
    public class Patch : ResoniteMod
    {
        public override string Author => "CalamityLime";
        public override string Name => "Context Menu Mods";
        public override string Version => "1.2.0";
        public override string Link => "https://github.com/LimeProgramming/ContextMenuMods/";

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MASTER_ENABLED = new ModConfigurationKey<bool>(
            "Enabled", "", () => true
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CTX_SEPERATION = new ModConfigurationKey<float>(
            "CTX menu item seperation", "", () => 6f
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CTX_RADIUS_RATIO = new ModConfigurationKey<float>(
            "CTX menu distance from center", "", () => 0.5f
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CTX_ARCLAYOUT_ARC = new ModConfigurationKey<float>(
            "CTX menu arc amount", "", () => 360f
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CTX_ARCLAYOUT_OFFSET = new ModConfigurationKey<float>(
            "CTX menu rotation offset", "", () => 0f
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<ArcLayout.Direction> CTX_ITEM_DIRECTION = new ModConfigurationKey<ArcLayout.Direction>(
            "CTX menu layout direction", "", () => ArcLayout.Direction.Clockwise
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CTX_ICON_ENABLED = new ModConfigurationKey<bool>(
            "CTX menu center icon enabled", "", () => true
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CTX_INNER_CIRCLE_ENABLED = new ModConfigurationKey<bool>(
            "CTX menu inner circle enabled", "", () => true
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CTX_FORCE_HIDDEN = new ModConfigurationKey<bool>(
            "Hide Context Menu", "Force context menu to be hidden", () => false
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CTX_ROUNDED_CORNER_RADIUS = new ModConfigurationKey<float>(
            "CTX Rounded Corner Radius", "", () => 16f
            );

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CTX_Disable_Scale_Reset = new ModConfigurationKey<bool>(
            "Disable Reset Scale", "Forcefully Disables Reset Scale button by tricking Resonite into thinking you're at full scale already.", () => true
            );

        //[AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_FILL_COLOR_ALPHA = new ModConfigurationKey<float>("Left Fill Color Alpha", "", () => 1f);


        private static ModConfiguration Config;

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);

            Harmony harmony = new Harmony("dev.calamitylime.contextmenumods");

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ContextMenu))]
        class ContextMenuPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ContextMenu), "OpenMenu")]
            public static void Prefix(ref ContextMenuOptions options)
            {
                if (Config.GetValue(CTX_FORCE_HIDDEN))
                {
                    options.hidden = true; //force the hidden option to be true.
                }
            }
          

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ContextMenu), "OpenMenu")]
            static void Postfix(ContextMenu __instance, Sync<float> ___Separation, Sync<float> ___RadiusRatio, SyncRef<ArcLayout> ____arcLayout, SyncRef<OutlinedArc> ____innerCircle, SyncRef<Image> ____iconImage, SyncRef ____currentSummoner)
            {
                if (Config.GetValue(MASTER_ENABLED))
                {
                    __instance.RunInUpdates(3, () =>
                    {
                        if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                        ArcLayout.Direction direction = Config.GetValue(CTX_ITEM_DIRECTION);
                        bool innerCircleEnabled = Config.GetValue(CTX_INNER_CIRCLE_ENABLED);
                        bool iconEnabled = Config.GetValue(CTX_ICON_ENABLED);
                        
                        ____arcLayout.Target.ItemDirection.Value = direction;
                        ____innerCircle.Target.Enabled = innerCircleEnabled;
                        ____iconImage.Target.Enabled = iconEnabled;


                        // ===== Seperation
                        float seperation = Config.GetValue(CTX_SEPERATION);

                        if (seperation != 6f) {
                            ___Separation.Value = seperation;
                        }

                        // ===== CTX_RADIUS_RATIO
                        float radiusRatio = Config.GetValue(CTX_RADIUS_RATIO);

                        if (radiusRatio != 0.5f) {
                            ___RadiusRatio.Value = radiusRatio;
                        }

                        // ===== CTX_ARCLAYOUT_ARC
                        float arc = Config.GetValue(CTX_ARCLAYOUT_ARC);
                        
                        if (radiusRatio != 360f) {
                            ____arcLayout.Target.Arc.Value = arc;
                        }

                        // ===== CTX_ARCLAYOUT_OFFSET
                        float offset = Config.GetValue(CTX_ARCLAYOUT_OFFSET);

                        if (offset != 0f) {
                            ____arcLayout.Target.Offset.Value = offset;
                        }

                    });
                }
            }
        }

        //IsAtScale is only used for the Reset Scale check.
        [HarmonyPatch(typeof(UserRoot), "IsAtScale")]
        private class UserRootIsAtScalePatch
        {
            public static bool Prefix(UserRoot __instance, ref bool __result)
            {
                if (Config.GetValue(CTX_Disable_Scale_Reset))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ContextMenuItem))]
        class ContextMenuItemPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void InitializePostfix(ContextMenuItem __instance, OutlinedArc arc)
            {
                if (!Config.GetValue(MASTER_ENABLED) || __instance == null || __instance.Slot == null || arc == null) return;

                User activeUser = __instance.Slot.ActiveUserRoot?.ActiveUser;
                if (activeUser == null || activeUser != __instance.LocalUser) return;

                ContextMenu menu = __instance.Slot?.GetComponentInParents<ContextMenu>();

                if (menu == null || menu.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                arc.RoundedCornerRadius.Value = Config.GetValue(CTX_ROUNDED_CORNER_RADIUS);
            }
        }
    }
}