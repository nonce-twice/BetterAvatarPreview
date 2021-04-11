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

        private ICustomShowableLayoutedMenu customMenu;

        private VRCVrCameraSteam ourSteamCamera;
        private Transform ourCameraRig;
        private Transform ourCameraTransform;

        private bool betterAvatarPreviewOn = false;
        private const string avatarMenuMainModelPath = "UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel";
        private Transform userInterfaceTransform = null;
        private GameObject avatarMenuMainModel = null;

        private float PositionOffsetX = 2.0f;
        private float PositionOffsetY = -1.0f;
        private Vector3 resetPosition;
        private Vector3 resetLocalPosition;
        private Quaternion resetLocalRotation;
        private Vector3 resetScale;


        public override void OnApplicationStart()
        {
            Harmony.Patch(AccessTools.Method(typeof(VRC.UI.PageAvatar), "OnEnable"), 
                postfix: new HarmonyMethod(typeof(BetterAvatarPreview).GetMethod("OnPageAvatarOpen", BindingFlags.Static | BindingFlags.Public)));


            MelonPreferences.CreateCategory(Pref_CategoryName);
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DisableOutlines),   false,  "Blug");

            customMenu = UIExpansionKit.API.ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            customMenu.AddSimpleButton("Do the thing", OnPageAvatarOpen);
            customMenu.AddSimpleButton("Close", CloseMenu);

            var avatarMenu = UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(ExpandedMenu.AvatarMenu);
            avatarMenu.AddSimpleButton("BetterAvatarPreview", OpenMenu);
        }

        public override void VRChat_OnUiManagerInit()
        {
//            // Taken directly from Knah's ViewpointTweaker Mod
//            foreach (var vrcTracking in VRCTrackingManager.field_Private_Static_VRCTrackingManager_0.field_Private_List_1_VRCTracking_0)
//            {
//                var trackingSteam = vrcTracking.TryCast<VRCTrackingSteam>();
//                if (trackingSteam == null) continue;
//
//                ourSteamCamera = trackingSteam.GetComponentInChildren<VRCVrCameraSteam>();
//                ourCameraRig = trackingSteam.transform.Find("SteamCamera/[CameraRig]");
//                ourCameraTransform = trackingSteam.transform.Find("SteamCamera/[CameraRig]/Neck/Camera (head)/Camera (eye)");
//            }
//
        }

        public void OpenMenu()
        {
            customMenu.Show();
        }
        public void CloseMenu()
        {
            customMenu.Hide();
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
                avatarMenuMainModel.transform.localRotation = resetLocalRotation;
                avatarMenuMainModel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            betterAvatarPreviewOn = false;
        }

        public void OnPageAvatarOpen()
        {
            Transform playerTransform = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform;
            userInterfaceTransform = GameObject.Find("UserInterface").transform;
            avatarMenuMainModel = GameObject.Find(avatarMenuMainModelPath);
            if(userInterfaceTransform == null)
            {
                MelonLogger.Warning("UserInterface traansfrom not found");
                return;
            }
            if(avatarMenuMainModel == null)
            {
                MelonLogger.Warning("MainModel was not found");
                return;
            }
            MelonLogger.Msg("MainModel and Transform found!");

            if(betterAvatarPreviewOn) // reset position and scales
            {
                MelonLogger.Msg("Resetting...");
                ResetBetterAvatarPreview();
                return;
            }

            // Store old local position
            resetLocalPosition = avatarMenuMainModel.transform.localPosition;
            resetLocalRotation = avatarMenuMainModel.transform.localRotation;

            // Scale to 1:1, within reason...
            var ls = avatarMenuMainModel.transform.lossyScale;
            Vector3 newLocalScale = new Vector3(1.0f / ls.x,  1.0f / ls.y,  1.0f / ls.z);
            avatarMenuMainModel.transform.localScale = newLocalScale;
            avatarMenuMainModel.transform.rotation = Quaternion.identity;

            // must be multiplied by local scale offset to match floor
            float floorOffset = newLocalScale.y * (avatarMenuMainModel.transform.position.y - playerTransform.position.y); // should be positive 
            avatarMenuMainModel.transform.localPosition = new Vector3(0.0f, -floorOffset , 0.0f);


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