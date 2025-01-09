using Alexandria.cAPI;
using Alexandria.ItemAPI;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HatLoader
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.hatloader";
        public const string NAME = "Hat Loader";
        public const string VERSION = "1.1.0";

        public void Awake()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager gm)
        {
            Loader.LoadHats();
            AutoHatManager.Init();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }
    }
}
