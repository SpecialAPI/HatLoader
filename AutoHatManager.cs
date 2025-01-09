using Alexandria.cAPI;
using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HatLoader
{
    [HarmonyPatch]
    public static class AutoHatManager
    {
        public static readonly HashSet<string> RandomHatList = [];

        public const string RandomHatListFileName = "RandomHatList.txt";
        public static string RandomHatListFilePath;

        public const string RandomHatCommandGroup = "randomhats";

        public static void Init()
        {
            RandomHatListFilePath = Path.Combine(Paths.ConfigPath, RandomHatListFileName);
            LoadRandomHatList();

            ETGModConsole.Commands.AddGroup(RandomHatCommandGroup);
            var group = ETGModConsole.Commands.GetGroup(RandomHatCommandGroup);

            group.AddUnit("addcurrent", x =>
            {
                if (!GameManager.HasInstance)
                    return; // WTF?

                if(GameManager.Instance.PrimaryPlayer == null)
                {
                    ETGModConsole.Log("No active player.").Foreground = Color.red;
                    return;
                }

                if(GameManager.Instance.PrimaryPlayer.GetComponent<HatController>() is not HatController control || control.CurrentHat is not Hat h || h == null)
                {
                    ETGModConsole.Log("The active player isn't wearing a hat.").Foreground = Color.red;
                    return;
                }

                var hatName = h.hatName;
                var hatabaseName = hatName.GetDatabaseFriendlyHatName();

                if (!RandomHatList.Add(hatabaseName))
                {
                    ETGModConsole.Log($"Current hat \"{hatabaseName}\" is already in the random hat list.").Foreground = Color.yellow;
                    return;
                }

                ETGModConsole.Log($"Current hat \"{hatabaseName}\" successfully added to the random hat list.").Foreground = Color.green;
                WriteRandomHatList();
            });

            group.AddUnit("setcurrent", x =>
            {
                if (!GameManager.HasInstance)
                    return; // WTF?

                if (GameManager.Instance.PrimaryPlayer == null)
                {
                    ETGModConsole.Log("No active player.").Foreground = Color.red;
                    return;
                }

                if (GameManager.Instance.PrimaryPlayer.GetComponent<HatController>() is not HatController control || control.CurrentHat is not Hat h || h == null)
                {
                    ETGModConsole.Log("The active player isn't wearing a hat.").Foreground = Color.red;
                    return;
                }

                var hatName = h.hatName;
                var hatabaseName = hatName.GetDatabaseFriendlyHatName();

                RandomHatList.Clear();
                RandomHatList.Add(hatabaseName);

                ETGModConsole.Log($"Random hat list set to current hat \"{hatabaseName}\"").Foreground = Color.green;
                WriteRandomHatList();
            });

            group.AddUnit("removecurrent", x =>
            {
                if (!GameManager.HasInstance)
                    return; // WTF?

                if (GameManager.Instance.PrimaryPlayer == null)
                {
                    ETGModConsole.Log("No active player.").Foreground = Color.red;
                    return;
                }

                if (GameManager.Instance.PrimaryPlayer.GetComponent<HatController>() is not HatController control || control.CurrentHat is not Hat h || h == null)
                {
                    ETGModConsole.Log("The active player isn't wearing a hat.").Foreground = Color.red;
                    return;
                }

                var hatName = h.hatName;
                var hatabaseName = hatName.GetDatabaseFriendlyHatName();

                if (!RandomHatList.Remove(hatabaseName))
                {
                    ETGModConsole.Log($"Current hat \"{hatabaseName}\" is not in the random hat list.").Foreground = Color.yellow;
                    return;
                }

                ETGModConsole.Log($"Current hat \"{hatabaseName}\" successfully removed from the random hat list.").Foreground = Color.green;
                WriteRandomHatList();
            });

            group.AddUnit("clear", x =>
            {
                if (RandomHatList.Count <= 0)
                {
                    ETGModConsole.Log($"Random hat list is already empty.").Foreground = Color.yellow;
                    return;
                }

                RandomHatList.Clear();
                ETGModConsole.Log($"Successfully cleared the random hat list.").Foreground = Color.green;
                WriteRandomHatList();
            });

            group.AddUnit("log", x =>
            {
                if (RandomHatList.Count <= 0)
                {
                    ETGModConsole.Log("Random hat list is empty.").Foreground = Color.yellow;
                    return;
                }

                ETGModConsole.Log("Current random hat list:");

                foreach (var r in RandomHatList)
                {
                    var txt = r;

                    if (!Hatabase.Hats.ContainsKey(txt.GetDatabaseFriendlyHatName()))
                        txt += $"<color=$FF0000>(not currently loaded)</color>";

                    ETGModConsole.Log(r);
                }
            });

            ETGModConsole.CommandDescriptions[$"{RandomHatCommandGroup} addcurrent"] = "Adds the player's current hat to the random hats list.";
            ETGModConsole.CommandDescriptions[$"{RandomHatCommandGroup} setcurrent"] = "Sets the random hats list to only contain the player's current hat.";
            ETGModConsole.CommandDescriptions[$"{RandomHatCommandGroup} removecurrent"] = "Removes the player's current hat from the random hats list.";
            ETGModConsole.CommandDescriptions[$"{RandomHatCommandGroup} clear"] = "Removes all hats from the random hats list.";
            ETGModConsole.CommandDescriptions[$"{RandomHatCommandGroup} log"] = "Logs the current hats list to the console.";
        }

        public static void LoadRandomHatList()
        {
            RandomHatList.Clear();

            if (!File.Exists(RandomHatListFilePath))
                return;

            foreach(var l in File.ReadAllLines(RandomHatListFilePath))
                RandomHatList.Add(l);
        }

        public static void WriteRandomHatList()
        {
            var dirName = Path.GetDirectoryName(RandomHatListFilePath);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            File.WriteAllLines(RandomHatListFilePath, [.. RandomHatList]);
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Start))]
        [HarmonyPostfix]
        public static void EquipRandomHat_Postfix(PlayerController __instance)
        {
            if (__instance == null || RandomHatList.Count <= 0)
                return;


            var hats = new List<string>(RandomHatList);
            while(hats.Count > 0)
            {
                var idx = UnityEngine.Random.Range(0, hats.Count);

                var hatName = hats[idx].GetDatabaseFriendlyHatName();
                hats.RemoveAt(idx);

                if (!Hatabase.Hats.ContainsKey(hatName))
                    return;

                Hatabase.StoredHats[__instance.name] = hatName; // 😋🍝
                __instance.gameObject.GetOrAddComponent<HatController>();
                break;
            }
        }
    }
}
