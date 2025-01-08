using Alexandria.cAPI;
using Alexandria.ItemAPI;
using BepInEx;
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
        public const string VERSION = "1.0.0";

        public static tk2dSpriteCollectionData HatLoaderCollection;

        public void Awake()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager gm)
        {
            HatLoaderCollection = SpriteBuilder.ConstructCollection(new GameObject(), "HatLoaderHatCollection");

            foreach (var f in Directory.GetFiles(Paths.PluginPath, "*-hat.spapi", SearchOption.AllDirectories))
            {
                var l = File.ReadAllLines(f);
                var file = Path.GetFileName(f);
                var dir = Path.GetDirectoryName(f);

                var name = "";

                var xOffs = 0;
                var yOffs = 0;

                var fps = 4;

                var attachedToHead = true;

                var inFrontWhenFacingBack = true;
                var inFrontWhenFacingFront = true;

                var flipOnRoll = true;
                var vanishOnRoll = false;

                var flipStartSound = "";
                var flipEndSound = "";

                var flipSpeed = 1f;
                var flipHeight = 1f;

                var flipHorizontalWithPlayer = (bool?)null;

                var hatSpritePaths = new Dictionary<string, List<string>>();
                var hatSpriteDefs = new List<tk2dSpriteDefinition>();

                var hatRoomSprite = "";

                SpapiDataReader.HandleLines(l, new()
                {
                    { "name", x =>
                    {
                        name = x.LastOrDefault();
                        return true;
                    } },

                    { "northsprites", x =>
                    {
                        hatSpritePaths["north"] = x;
                        return true;
                    } },
                    { "southsprites", x =>
                    {
                        hatSpritePaths["south"] = x;
                        return true;
                    } },
                    { "westsprites", x =>
                    {
                        hatSpritePaths["west"] = x;
                        return true;
                    } },
                    { "eastsprites", x =>
                    {
                        hatSpritePaths["east"] = x;
                        return true;
                    } },
                    { "northwestsprites", x =>
                    {
                        hatSpritePaths["northwest"] = x;
                        return true;
                    } },
                    { "northeastsprites", x =>
                    {
                        hatSpritePaths["northeast"] = x;
                        return true;
                    } },

                    { "hatroomsprite", x =>
                    {
                        hatRoomSprite = x.LastOrDefault();
                        return true;
                    } },

                    { "xoffset", x =>
                    {
                        if(int.TryParse(x.LastOrDefault(), out xOffs))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: XOffset should be an integer.");
                        return false;
                    } },
                    { "yoffset", x =>
                    {
                        if(int.TryParse(x.LastOrDefault(), out yOffs))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: YOffset should be an integer.");
                        return false;
                    } },

                    { "fps", x =>
                    {
                        if(int.TryParse(x.LastOrDefault(), out fps))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: FPS should be an integer.");
                        return false;
                    } },

                    { "attachedtohead", x =>
                    {
                        if(bool.TryParse(x.LastOrDefault(), out attachedToHead))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: AttachedToHead should be True or False.");
                        return false;
                    } },

                    { "infrontwhenfacingback", x =>
                    {
                        if(bool.TryParse(x.LastOrDefault(), out inFrontWhenFacingBack))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: InFrontWhenFacingBack should be True or False.");
                        return false;
                    } },
                    { "infrontwhenfacingfront", x =>
                    {
                        if(bool.TryParse(x.LastOrDefault(), out inFrontWhenFacingFront))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: InFrontWhenFacingFront should be True or False.");
                        return false;
                    } },

                    { "fliponroll", x =>
                    {
                        if(bool.TryParse(x.LastOrDefault(), out flipOnRoll))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: FlipOnRoll should be True or False.");
                        return false;
                    } },
                    { "vanishonroll", x =>
                    {
                        if(bool.TryParse(x.LastOrDefault(), out vanishOnRoll))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: VanishOnRoll should be True or False.");
                        return false;
                    } },

                    { "flipstartsound", x =>
                    {
                        flipStartSound = x.LastOrDefault();
                        return true;
                    } },
                    { "flipendsound", x =>
                    {
                        flipEndSound = x.LastOrDefault();
                        return true;
                    } },

                    { "flipspeed", x =>
                    {
                        if(float.TryParse(x.LastOrDefault(), out flipSpeed))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: FlipSpeed should be a number.");
                        return false;
                    } },
                    { "flipheight", x =>
                    {
                        if(float.TryParse(x.LastOrDefault(), out flipHeight))
                            return true;

                        Debug.LogError($"Error reading hat file {file}: FlipHeight should be a number.");
                        return false;
                    } },

                    { "fliphorizontalwithplayer", x =>
                    {
                        var res = bool.TryParse(x.LastOrDefault(), out var b);
                        flipHorizontalWithPlayer = b;

                        if(res)
                            return true;

                        Debug.LogError($"Error reading hat file {file}: FlipHorizontalWithPlayer should be True or False.");
                        return false;
                    } },
                }, x => Debug.LogError($"Error reading hat file {file}: {x}"));

                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogError($"Error loading hat {file}: hat name not given.");
                    continue;
                }

                foreach (var kvp in hatSpritePaths)
                {
                    var direction = kvp.Key;
                    var paths = kvp.Value;

                    for (var i = 0; i < paths.Count; i++)
                    {
                        var def = LoadHatSpriteDefinition(dir, paths[i], $"{name.ToID()}_{direction}_{i + 1:D3}", file, out _);

                        if (def == null)
                            continue;

                        hatSpriteDefs.Add(def);
                    }
                }

                var defaultSpriteId = -1;
                var defaultSpriteDef = (tk2dSpriteDefinition)null;

                if (!string.IsNullOrEmpty(hatRoomSprite))
                    defaultSpriteDef = LoadHatSpriteDefinition(dir, hatRoomSprite, $"{name.ToID()}_default_001", file, out defaultSpriteId);

                if(hatSpriteDefs.Count <= 0)
                {
                    Debug.LogError($"Error loading hat {file}: hat doesn't have any sprites.");
                    continue;
                }

                try
                {
                    var h = HatUtility.SetupHat(name,
                        null,
                        new(xOffs, yOffs),
                        fps,
                        attachedToHead ? Hat.HatAttachLevel.HEAD_TOP : Hat.HatAttachLevel.EYE_LEVEL,
                        inFrontWhenFacingBack ? (inFrontWhenFacingFront ? Hat.HatDepthType.ALWAYS_IN_FRONT : Hat.HatDepthType.IN_FRONT_WHEN_FACING_BACK) : (inFrontWhenFacingFront ? Hat.HatDepthType.BEHIND_WHEN_FACING_BACK : Hat.HatDepthType.ALWAYS_BEHIND),
                        vanishOnRoll ? Hat.HatRollReaction.VANISH : (flipOnRoll ? Hat.HatRollReaction.FLIP : Hat.HatRollReaction.NONE),
                        string.IsNullOrEmpty(flipStartSound) ? null : flipStartSound,
                        string.IsNullOrEmpty(flipEndSound) ? null : flipEndSound,
                        flipSpeed,
                        flipHeight,
                        flipHorizontalWithPlayer);

                    h.SetupHatSprites(null, hatSpriteDefs, fps);

                    if (defaultSpriteDef != null)
                        defaultSpriteDef.colliderVertices = [.. h.sprite.CurrentSprite.colliderVertices];

                    if (defaultSpriteId >= 0)
                        h.sprite.SetSprite(HatLoaderCollection, defaultSpriteId);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Error loading hat {file}: {ex.Message}");
                }
            }
        }

        public static tk2dSpriteDefinition LoadHatSpriteDefinition(string hatDirectory, string spriteName, string definitionName, string hatFile, out int spriteId)
        {
            spriteId = -1;
            var path = spriteName.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            if (!path.EndsWith(".png"))
                path += ".png";

            var tPath = Path.Combine(hatDirectory, path);

            if (!File.Exists(tPath))
            {
                Debug.LogError($"Error loading sprites for hat {hatFile}: file {spriteName} doesn't exist.");
                return null;
            }

            var ba = File.ReadAllBytes(tPath);

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                name = definitionName
            };

            try
            {
                if (!tex.LoadImage(ba))
                {
                    Debug.LogError($"Error loading sprites for hat {hatFile}: file {path} is not a valid texture.");
                    return null;
                }
            }
            catch
            {
                Debug.LogError($"Error loading sprites for hat {hatFile}: file {path} is not a valid texture.");
                return null;
            }

            spriteId = SpriteBuilder.AddSpriteToCollection(tex, HatLoaderCollection);
            var def = HatLoaderCollection.spriteDefinitions[spriteId];

            return def;
        }
    }
}
