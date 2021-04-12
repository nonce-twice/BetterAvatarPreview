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

namespace BetterAvatarPreview
{
    public enum AvatarVersion
    {
       AV2, AV3, None
    }

    public class AvatarPreview
    {
        private GameObject avatarPrefab = null;
        private string avatarId = "";
        private AvatarVersion avatarVersion = AvatarVersion.None;

        public GameObject AvatarPrefab
        { get; set; }

        public string AvatarId
        { get; set; }

        public Transform ResetTransform
        { get; set; }

        public AvatarVersion AvatarVersion        
        { get; set; }

    }

}
