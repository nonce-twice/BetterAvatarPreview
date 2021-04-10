using System;
using System.Reflection;
using System.Collections;
using UIExpansionKit.API;
using Harmony;
using MelonLoader;
using UnityEngine;
using VRC;

[assembly:MelonInfo(typeof(BetterAvatarPreview.BetterAvatarPreview), "BetterAvatarPreview", "0.1", "nonce-twice", "https://github.com/nonce-twice/BetterAvatarPreview")]
[assembly:MelonGame("VRChat", "VRChat")]

namespace BetterAvatarPreview
{
    public class BetterAvatarPreview : MelonMod
    {
        public const string Pref_CategoryName = "BetterAvatarPreview";
        public bool Pref_DisableOutlines = false;
        public bool Pref_DebugOutput = false;


        // VRCPlayer[Local] ... /AnimationController/HeadAndHandIK/RightEffector/PickupTether(Clone)
        private GameObject leftHandTether = null;

        public override void OnApplicationStart()
        {
            Harmony.Patch(AccessTools.Method(typeof(VRC.UI.PageAvatar), "OnEnable"), 
                postfix: new HarmonyMethod(typeof(BetterAvatarPreview).GetMethod("OnPageAvatarOpen", BindingFlags.Static | BindingFlags.Public)));

            MelonPreferences.CreateCategory(Pref_CategoryName);
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DisableOutlines),   false,  "Blug");

            var avatarMenu = UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(ExpandedMenu.AvatarMenu);
            avatarMenu.AddSimpleButton("BetterAvatarPreview", OnPageAvatarOpen);
        }

        // Skip over initial loading of (buildIndex, sceneName): [(0, "app"), (1, "ui")]
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            switch (buildIndex) {
                case 0: 
                    break; 
                case 1: 
                    break;  
                default:
                    ApplyAllSettings();
                    break;
            }
        }

        public override void OnPreferencesSaved()
        {
            ApplyAllSettings();
        }

        private void ApplyAllSettings()
        {
            UpdatePreferences();
        }

        public void OnPageAvatarOpen()
        {
            MelonLogger.Msg("Avatar menu is open!");
        }

        private void UpdatePreferences()
        {
            Pref_DisableOutlines   = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DisableOutlines));
            Pref_DebugOutput = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DebugOutput));
        }

        private void SetTetherReferences()
        {
//            try 
//            {
//                VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0; // is not null
//                leftHandTether  = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/LeftEffector/PickupTether(Clone)/Tether/Quad").gameObject;
//            }
//            catch(Exception e)
//            {
//                MelonLogger.Error(e.ToString());
//            }
//            finally
//            {
//            }
        }


        private void LogDebugMsg(string msg)
        {
            if (!Pref_DebugOutput)
            {
                return; 
            }
            MelonLogger.Msg(msg);
        }
        
    }
}