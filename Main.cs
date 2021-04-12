using System;
using System.Reflection;
using System.Collections;
using UIExpansionKit.API;
using Harmony;
using MelonLoader;
using UnityEngine;
using UnityEditor;
using VRC;
using VRCSDK2;
using VRC.SDKInternal;

[assembly:MelonInfo(typeof(BetterAvatarPreview.BetterAvatarPreview), "BetterAvatarPreview", "0.1", "nonce-twice", "https://github.com/nonce-twice/BetterAvatarPreview")]
[assembly:MelonGame("VRChat", "VRChat")]

namespace BetterAvatarPreview
{

    public class BetterAvatarPreview : MelonMod
    {

        // Debug for limiting stuff
        private static int CURRENT_ITERATIONS = 0;
        private static int MAX_ITERATIONS = 2;

        public const string Pref_CategoryName = "BetterAvatarPreview";
        public bool Pref_DebugOutput = false;

        private bool initialized = false;
        private bool avatarMenuOpen = false;
        private bool desktopZoomOut = false;

        private ICustomShowableLayoutedMenu customMenu;
        private string currentAvatarId = "";


        private VRCVrCameraSteam ourSteamCamera;
        private Transform ourCameraRig;
        private Transform ourCameraTransform;

        private bool betterAvatarPreviewOn = false;
        private const string avatarMenuMainModelPath = "UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel";
        private Transform userInterfaceTransform = null;
        private GameObject avatarMenuMainModel = null;

        private AvatarPreview CurrentAvatarPreview = new AvatarPreview();
        private VRC.UI.PageAvatar pageAvatar;
        private UIExpansionKit.Components.EnableDisableListener pageAvatarListener;

        private float PositionOffsetX = -1.0f;
        private float PositionOffsetY = -1.0f;
        private Vector3 resetLocalPosition;
        private Quaternion resetLocalRotation;
        private Vector3 resetDesktopCameraPosition = new Vector3();


        public override void OnApplicationStart()
        {
//            Harmony.Patch(AccessTools.Method(typeof(VRC.UI.PageAvatar), "OnEnable"), 
//                postfix: new HarmonyMethod(typeof(BetterAvatarPreview).GetMethod("OnPageAvatarOpen", BindingFlags.Static | BindingFlags.Public)));


            MelonPreferences.CreateCategory(Pref_CategoryName);
            MelonPreferences.CreateEntry(Pref_CategoryName, nameof(Pref_DebugOutput),   false,  "Show Debug Output");

            customMenu = UIExpansionKit.API.ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            customMenu.AddSimpleButton("Move model to floor", OnPageAvatarOpen);
            customMenu.AddSimpleButton("Reset", ResetBetterAvatarPreview);
            customMenu.AddSimpleButton("Move avatar further", MoveFurther);
            customMenu.AddSimpleButton("Move avatar closer", MoveCloser);
            customMenu.AddSimpleButton("Toggle Rotation", ToggleAvatarRotation);
            customMenu.AddSimpleButton("DebugAvatarInfo", ShowDebugAvatarInfo);
            customMenu.AddSimpleButton("Close", CloseMenu);

            var avatarMenu = UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(ExpandedMenu.AvatarMenu);
            avatarMenu.AddSimpleButton("AvatarPreview+", OnPageAvatarOpen);
            avatarMenu.AddSimpleButton("BetterAvatarPreviewMenu", OpenMenu);

            CurrentAvatarPreview = new AvatarPreview();
        }

        public override void VRChat_OnUiManagerInit()
        {
            // Taken from UIExpansionKit by Knah
            pageAvatar = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").GetComponent<VRC.UI.PageAvatar>();
            pageAvatarListener = pageAvatar.gameObject.GetComponent<UIExpansionKit.Components.EnableDisableListener>();
            if(pageAvatarListener == null)
            {
                pageAvatarListener = pageAvatar.gameObject.AddComponent<UIExpansionKit.Components.EnableDisableListener>();
            }
            pageAvatarListener.OnEnabled += () =>
            {
                AvatarMenuOpen(true);
            };
            pageAvatarListener.OnDisabled += () =>
            {
                AvatarMenuOpen(false);
            };

            initialized = true;
        }

        // TODO
        private void DesktopZoomOut(bool reset)
        {

        }

        public override void OnUpdate()
        {
            if (!initialized || !avatarMenuOpen )
            {
                return;
            }
            
            // Adapted from FavCat by Knah
            // New avatar loaded
            if(pageAvatar.field_Public_SimpleAvatarPedestal_0 != null && pageAvatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0 != null &&
                !currentAvatarId.Equals(pageAvatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0.id))
            {
                currentAvatarId = pageAvatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0.id;
               // MelonLogger.Msg("Loaded avatar: " + apiAvatar.name + " by " + apiAvatar.authorName); 
                SetupCurrentAvatarPreview();
                if(betterAvatarPreviewOn)
                {
                    ResetBetterAvatarPreview();
                    OnPageAvatarOpen();
                }
            }
        }

        private void AvatarMenuOpen(bool isOpen)
        {
            MelonLogger.Msg(isOpen ? "Avatar menu opened." : "Avatar menu closed.");
            avatarMenuOpen = isOpen;
        }

        public void ShowDebugAvatarInfo()
        {
//            if(avatarMenuMainModel == null)
//            {
//                MelonLogger.Warning("Main Model is null, returning.");
//                return;
//            }
//            AvatarVersion avatarSDKVersion = CheckAvatarSDKVersion();
//            bool isSDK3 = (avatarSDKVersion == AvatarVersion.AV3);
//            MelonLogger.Msg("Avatar Version: " + (isSDK3 ? "SDK3" : "SDK2"));
        }
        private void LookAtModel(bool reset)
        {

        }

        //TODO
        private void SetupCurrentAvatarPreview()
        {
            var mainModel = GetMainModel();
            var currentApiAvatar = pageAvatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0;
            var avatarPrefab = pageAvatar.field_Private_GameObject_0;
            CurrentAvatarPreview.AvatarPrefab = pageAvatar.field_Private_GameObject_0;
            CurrentAvatarPreview.AvatarName = currentApiAvatar.name;
            CurrentAvatarPreview.AuthorName = currentApiAvatar.authorName;
            CurrentAvatarPreview.AvatarId = currentApiAvatar.id;
            CurrentAvatarPreview.AvatarVersion = CheckAvatarSDKVersion(avatarPrefab);
            MelonLogger.Msg("Loaded new avatar!\n " + CurrentAvatarPreview.ToString());
        }

        public void OpenMenu()
        {
            customMenu.Show();
        }
        public void CloseMenu()
        {
            customMenu.Hide();
        }

        // Move these to a Utilities class
        private void MoveModel(Vector3 offset)
        {
            GetMainModel().transform.localPosition += offset;
        }
        private void MoveModelToFloor(Vector3 scaleOffset, Vector3 playerPosition, Vector3 mainModelPosition)

        {
        }

        public void MoveFurther()
        {
            if (!betterAvatarPreviewOn) return;
            MoveModel(new Vector3(-0.25f, 0.0f, 0.0f));
        }

        public void MoveCloser()
        {
            if (!betterAvatarPreviewOn) return;
            MoveModel(new Vector3(0.25f, 0.0f, 0.0f));
        }
        public void ToggleAvatarRotation()
        {
            var rotator = GetMainModel().GetComponent<UnityStandardAssets.Utility.AutoMoveAndRotate>();
            if(rotator == null)
            {
                MelonLogger.Warning("Could not get AutoMoveAndRotate component, not toggling rotation.");
                return;
            }
            rotator.enabled = !rotator.isActiveAndEnabled;
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

        private GameObject GetMainModel()
        {
            if(avatarMenuMainModel == null)
            {
                avatarMenuMainModel = GameObject.Find(avatarMenuMainModelPath);
                if(avatarMenuMainModel == null)
                {
                    MelonLogger.Error("MainModel not found! BetterAvatarPreview won't work...");
                }
            }
            return avatarMenuMainModel;
        }

        private bool MainModelActive()
        {
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                return mainModel.activeInHierarchy;
            }
            return false;
        }


        // Toggles the thing
        public void OnPageAvatarOpen()
        {
            Transform playerTransform = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform;

            var mainModel = GetMainModel();

//            if (betterAvatarPreviewOn) // reset position and scales
//            {
//                MelonLogger.Msg("Resetting...");
//                ResetBetterAvatarPreview();
//                return;
//            }

            // Store old local position
            resetLocalPosition = mainModel.transform.localPosition;
            resetLocalRotation = mainModel.transform.localRotation;

            // Scale to 1:1, within reason...
            Vector3 newLocalScale = GetScaleOffset(mainModel);
            mainModel.transform.localScale = newLocalScale;
            mainModel.transform.rotation = Quaternion.identity;

            // must be multiplied by local scale offset to match floor
            float floorOffset = newLocalScale.y * (mainModel.transform.position.y - playerTransform.position.y); // should be positive 
            mainModel.transform.localPosition = new Vector3(0.0f, -floorOffset, 0.0f);

            // Move model a bit further out
            Vector3 playerRight = playerTransform.right;
            float xOffset = newLocalScale.x * PositionOffsetX;
            MoveModel(new Vector3(xOffset, 0.0f, 0.0f));

            // Mark dirty
            betterAvatarPreviewOn = true;
            MelonLogger.Msg("BetterAvatarPreview on!");
        }

        // Returns 1 / lossyScale which is the amt. to scale to get real size
        private Vector3 GetScaleOffset(GameObject mainModel)
        {
            var ls = GetMainModel().transform.lossyScale;
            return new Vector3(1.0f/ls.x, 1.0f/ls.y, 1.0f/ls.z);
        }

        private void UpdatePreferences()
        {
            Pref_DebugOutput = MelonPreferences.GetEntryValue<bool>(Pref_CategoryName, nameof(Pref_DebugOutput));
        }

        private void OnAvatarUpdate()
        {

        }

        private AvatarVersion CheckAvatarSDKVersion(GameObject avatarPrefab)
        {
            if(avatarPrefab == null)
            {
                return AvatarVersion.None;
            }
            // Move logic to avatar preview class
            Component[] sdk2 = avatarPrefab.GetComponentsInChildren<VRCSDK2.VRC_AvatarDescriptor>();
            if (sdk2.Length > 0) 
                return AvatarVersion.AV2;
            // SDK3 check redundant (CURRENTLY) but implemented for testing
            Component[] sdk3 = avatarPrefab.GetComponentsInChildren<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if(sdk3.Length > 0)
                return AvatarVersion.AV3; 
            return AvatarVersion.None;
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