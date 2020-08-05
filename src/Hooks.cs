using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;
using MelonLoader;

namespace AudicaModding
{
    internal static class Hooks
    {
        public static void ApplyHooks(HarmonyInstance instance)
        {
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage")]
        private static class PatchOptionsMenuShowPage
        {
            private static void Postfix(OptionsMenu __instance, ref OptionsMenu.Page page)
            {
                AudicaMod.ShowPage(__instance, page);
            }
        }

        [HarmonyPatch(typeof(SpectatorCam), "Update")]
        private static class PatchSpectatorCamUpdate
        {
            private static bool Prefix(SpectatorCam __instance)
            {
                if (AudicaMod.camOK && AudicaMod.spectatorCamSet && AudicaMod.config.activated)
                {
                    AudicaMod.SpectatorCamUpdate();
                    return true;
                }
                else
                {
                    return false;
                }
                   
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState")]
        private static class PatchMenuSetState
        {
            private static void Prefix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.MainPage || state == MenuState.State.SettingsPage)
                {
                    MelonCoroutines.Start(AudicaMod.PerformChecks());
                }
            }
        }

        [HarmonyPatch(typeof(SpectatorCam), "OnEnable")]
        private static class PatchSpectatorCamOnEnable
        {
            private static void Postfix(SpectatorCam __instance)
            {
                if (AudicaMod.spectatorCamSet) return;

                AudicaMod.SetSpectatorCam(__instance, true);
            }
        }
    }
}
