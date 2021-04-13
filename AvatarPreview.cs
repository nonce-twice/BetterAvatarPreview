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
        private string avatarName = "";
        private string authorName = "";
        private string avatarId = "";
        private AvatarVersion avatarVersion = AvatarVersion.None;
        private Vector3 resetPosition;
        private Vector3 resetScale;
        private Vector3 lossyScale;
        private bool valid;

        public GameObject AvatarPrefab
        { get; set; }

        public string AvatarName
        {
            get { return avatarName; }
            set { avatarName = value; }
        }
        public string AuthorName
        {
            get { return authorName ; }
            set { authorName = value; }
        }

        public string AvatarId
        {
            get { return avatarId; }
            set { avatarId = value; }
        }

        public Transform ResetTransform
        { get; set; }

        public AvatarVersion AvatarVersion
        {
            get { return avatarVersion; }
            set { avatarVersion = value; }
        }
        public Vector3 ResetPosition 
        {
            get { return resetPosition; }
            set { resetPosition = value; }
        }

        public Vector3 ResetScale 
        {
            get { return resetScale; }
            set { resetScale = value; }
        }
        public Vector3 LossyScale 
        {
            get { return lossyScale; }
            set { lossyScale = value; }
        }
        public bool Valid 
        {
            get { return valid; }
            set { valid = value; }
        }
        public override string ToString()
        {
            return "Name: " + AvatarName + "\n"
                + "Author Name: " + AuthorName + "\n"
                + "Version: " + AvatarVersion.ToString() + "\n"
                + "Id: " + AvatarId;
        }

    }

}
