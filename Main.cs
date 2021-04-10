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

        private bool betterAvatarPreviewOn = false;
        private const string avatarMenuMainModelPath = "UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel";
        private GameObject avatarMenuMainModel = null;

        private float PositionOffsetX = 2.0f;
        private Vector3 resetPosition;
        private Vector3 resetLocalPosition;
        private Vector3 resetScale;


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

        public void ResetBetterAvatarPreview()
        {
            MelonLogger.Msg("Resetting avatar preview");
            if(!betterAvatarPreviewOn)
            {
                MelonLogger.Warning("betterAvatarPRevionOn was false! returning...");
                return;
            }
            if(avatarMenuMainModel != null)
            {
                MelonLogger.Warning("mainModel not null, reset positions");
                avatarMenuMainModel.transform.localPosition = resetLocalPosition;
                avatarMenuMainModel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            betterAvatarPreviewOn = false;
        }

        public void OnPageAvatarOpen()
        {
            avatarMenuMainModel = GameObject.Find(avatarMenuMainModelPath);
            if(avatarMenuMainModel == null)
            {
                MelonLogger.Warning("MainModel was not found");
                return;
            }
            MelonLogger.Msg("MainModel found!");

            if(betterAvatarPreviewOn) // reset position and scales
            {
                MelonLogger.Msg("Resetting...");
                ResetBetterAvatarPreview();
                return;
            }

            // Store old local position
            resetLocalPosition = avatarMenuMainModel.transform.localPosition;

            // Scale to 1:1, within reason....
            var ls = avatarMenuMainModel.transform.lossyScale;
            Vector3 newLocalScale = new Vector3(1.0f / ls.x,  1.0f / ls.y,  1.0f / ls.z) ;
            avatarMenuMainModel.transform.localScale = newLocalScale;
            avatarMenuMainModel.transform.localPosition = resetLocalPosition + new Vector3(PositionOffsetX, 0.0f, 0.0f);

            // Mark dirty
            betterAvatarPreviewOn = true;
            MelonLogger.Msg("BetterAvatarPreview on!");
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