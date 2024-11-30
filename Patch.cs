using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace Context_Menu_Funnies
{
    public class Patch : ResoniteMod
    {
        public override string Author => "CalamityLime";
        public override string Name => "Context Menu Mods";
        public override string Version => "1.2.0";
        public override string Link => "https://github.com/LimeProgramming/ContextMenuMods/";

        [AutoRegisterConfigKey] private static ModConfigurationKey<bool> MASTER_ENABLED = new ModConfigurationKey<bool>("Enabled", "", () => true);
        [AutoRegisterConfigKey] private static ModConfigurationKey<dummy> SPACER_A = new ModConfigurationKey<dummy>("", "");
        // Left

        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_SEPERATION = new ModConfigurationKey<float>("CTX menu item seperation", "", () => 6f);
        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_RADIUS_RATIO = new ModConfigurationKey<float>("CTX menu distance from center", "", () => 0.5f);
        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_ARCLAYOUT_ARC = new ModConfigurationKey<float>("CTX menu arc amount", "", () => 360f);
        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_ARCLAYOUT_OFFSET = new ModConfigurationKey<float>("CTX menu rotation offset", "", () => 0f);
        [AutoRegisterConfigKey] private static ModConfigurationKey<ArcLayout.Direction> CTX_ITEM_DIRECTION = new ModConfigurationKey<ArcLayout.Direction>("CTX menu layout direction", "", () => ArcLayout.Direction.Clockwise);
        [AutoRegisterConfigKey] private static ModConfigurationKey<bool> CTX_ICON_ENABLED = new ModConfigurationKey<bool>("CTX menu center icon enabled", "", () => true);
        [AutoRegisterConfigKey] private static ModConfigurationKey<bool> CTX_INNER_CIRCLE_ENABLED = new ModConfigurationKey<bool>("CTX menu inner circle enabled", "", () => true);
        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_ROUNDED_CORNER_RADIUS = new ModConfigurationKey<float>("CTX Rounded Corner Radius", "", () => 16f);
        [AutoRegisterConfigKey] private static ModConfigurationKey<float> CTX_FILL_COLOR_ALPHA = new ModConfigurationKey<float>("CTX Fill Color Alpha", "", () => 1f);


        private static ModConfiguration config;

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony("dev.lecloutpanda.contextmenufunnies");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(ContextMenu))]
        class ContextMenuPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ContextMenu), "OpenMenu")]
            static void Postfix(ContextMenu __instance, Sync<float> ___Separation, Sync<float> ___RadiusRatio, SyncRef<ArcLayout> ____arcLayout, SyncRef<OutlinedArc> ____innerCircle, SyncRef<Image> ____iconImage, SyncRef ____currentSummoner)
            {
                if (config.GetValue(MASTER_ENABLED))
                {
                    __instance.RunInUpdates(3, () =>
                    {
                        if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                        //Chirality side = __instance.Pointer.Target.GetComponent<InteractionLaser>().Side;

                        ___Separation.Value =                       config.GetValue(CTX_SEPERATION);
                        ___RadiusRatio.Value =                      config.GetValue(CTX_RADIUS_RATIO);
                        ____arcLayout.Target.Arc.Value =            config.GetValue(CTX_ARCLAYOUT_ARC);
                        ____arcLayout.Target.ItemDirection.Value =  config.GetValue(CTX_ITEM_DIRECTION);
                        ____arcLayout.Target.Offset.Value =         config.GetValue(CTX_ARCLAYOUT_OFFSET);
                        ____innerCircle.Target.Enabled =            config.GetValue(CTX_INNER_CIRCLE_ENABLED);
                        ____iconImage.Target.Enabled =              config.GetValue(CTX_ICON_ENABLED);
                    });
                }
            }
        }

        [HarmonyPatch(typeof(ContextMenuItem))]
        class ContextMenuItemPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void InitializePostfix(ContextMenuItem __instance, OutlinedArc arc)
            {
                if (!config.GetValue(MASTER_ENABLED) || __instance == null || __instance.Slot == null || arc == null) return;

                User activeUser = __instance.Slot.ActiveUserRoot?.ActiveUser;
                if (activeUser == null || activeUser != __instance.LocalUser) return;

                ContextMenu menu = __instance.Slot?.GetComponentInParents<ContextMenu>();
                //Chirality? side = menu?.Pointer?.Target?.GetComponent<InteractionLaser>()?.Side;
                //if (menu == null || side == null || menu.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                if (menu == null || menu.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                arc.RoundedCornerRadius.Value = config.GetValue(CTX_ROUNDED_CORNER_RADIUS);
            }

            [HarmonyPostfix]
            [HarmonyPatch("UpdateColor", new Type[] { })]
            public static void UpdateColorPostfix(ContextMenuItem __instance, SyncRef<Button> ____button)
            {
                if (!config.GetValue(MASTER_ENABLED) || __instance == null || __instance.Slot == null || ____button == null) return;

                User activeUser = __instance.Slot.ActiveUserRoot?.ActiveUser;
                if (activeUser == null || activeUser != __instance.LocalUser) return;

                ContextMenu menu = __instance.Slot?.GetComponentInParents<ContextMenu>();
                //Chirality? side = menu?.Pointer?.Target?.GetComponent<InteractionLaser>()?.Side;
                //if (menu == null || side == null || menu.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                if (menu == null || menu.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser) return;

                var colorDrive = ____button?.Target?.ColorDrivers[0];
                if (colorDrive == null) return;

                var alpha = config.GetValue(CTX_FILL_COLOR_ALPHA);

                var oldNormalColor = colorDrive.NormalColor.Value;
                var oldHighlightColor = colorDrive.HighlightColor.Value;
                var oldPressColor = colorDrive.PressColor.Value;
                colorDrive.NormalColor.Value = new colorX(oldNormalColor.r, oldNormalColor.g, oldNormalColor.b, alpha);
                colorDrive.HighlightColor.Value = new colorX(oldHighlightColor.r, oldHighlightColor.g, oldHighlightColor.b, alpha);
                colorDrive.PressColor.Value = new colorX(oldPressColor.r, oldPressColor.g, oldPressColor.b, alpha);
            }
        }
    }
}

