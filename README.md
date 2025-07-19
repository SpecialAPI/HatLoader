# Hat Loader
Allows you to easily make new hats using special `-hat.spapi` files. No code required!

# How it's done
Inside of the `plugins` folder (doesn't have to be directly inside, the files can be in subfolders inside `plugins`), make files ending in `-hat.spapi` for each hat you want to make. The files should look like this:
```
# Name
{name of your hat}

# NorthSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# SouthSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# WestSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# EastSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# NorthwestSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# NortheastSprites
{paths to your hat sprites, relative to the -hat.spapi file}

# HatRoomSprite
{sprite that will be displayed in the hat room} (optional, hat room sprite defaults to the first normal given hat sprite)

# XOffset
{your hat's horizontal offset, in pixels} (optional, defaults to 0)

# YOffset
{your hat's vertical offset, in pixels} (optional, defaults to 0)

# FPS
{fps for your hat's animations, if it's animated} (optional, defaults to 4. does nothing if the hat is not animated)

# AttachedToHead
{True if you want the hat to be achored on the character's head, False if you want it to be anchored on the character's eyes} (optional, defaults to True)

# InFrontWhenFacingBack
{True if you want the hat to be in front of the character while facing back, False otherwise} (optional, defaults to True)

# InFrontWhenFacingFront
{True if you want the hat to be in front of the character whlie facing front, False otherwise} (optional, defaults to True)

# FlipOnRoll
{True if you want the hat to flip when the player rolls, similarly to hats in Exit the Gungeon, False otherwise} (optional, defaults to True)

# VanishOnRoll
{True if you want the hat to disappear when the player rolls, False otherwise} (optional, defaults to False. takes priority over FlipOnRoll)

# FlipStartSound
{the sound you want to play when this hat's roll flip starts} (optional, defaults to no sound)

# FlipEndSound
{the sound you want to play when this hat's roll flip ends} (optional, defaults to no sound)

# FlipSpeed
{a speed multiplier for how fast this hat spins during its flip animation} (optional, defaults to 1. setting this to 0 will result in the hat simply bouncing above the player instead of spinning)

# FlipHeight
{a multiplier for how high this hat bounces during its flip animation} (optional, defaults to 1)

# FlipHorizontalWithPlayer
{True if this sprite should be automatically flipped with the player's sprite, False otherwise} (optional, if a value is not given it will be automatically decided based on the hat's directionality)
```

Properties can go in any order. Optional hat properties do not have to be included in the hat files, in which case they will instead get set to their default values. Not all `Sprites` properties have to be given in the file, the hat's directionality will be decided based on which sprite directions are given using this table:

| Given sprite directions | Directionality | `FlipHorizontalWithPlayer` default |
|-|-|-|
| `SouthSprites` | None (single direction) | `True` |
| `WestSprites`, `EastSprites` | Two way (horizontal) | `False` |
| `NorthSprites`, `SouthSprites` | Two way (vertical) | `True` |
| `NorthSprites`, `WestSprites`, `SouthSprites`, `EastSprites` | Four way | `False` |
| All sprite directions given | Six way | `False` |

Multiple sprite paths can be given for a sprite direction, in which case that direction will be animated. Sprite paths are relative to the hat file's folder, so a sprite path of `hat_idle_001` for a hat file with the path `plugins/YourHat/yourhat-hat.spapi` will lead to `plugins/YourHat/hat_idle_001.png`, a sprite path of `SomeSubfolder/hat_idle_001` will lead to `plugins/YourHat/SomeSubfolder/hat_idle_001.png`, etc. Sprite paths don't need to have extensions.

# Shaders
Hat Loader allows you to apply shaders to your hats. To do that, you need to add the `ShaderName` property to your hat file, followed by the name of the shader you want to use. For example:
```
# ShaderName
Brave/Internal/RainbowChestShader
```
You can also change the shader properties using the `ShaderFloatProperties`, `ShaderIntProperties`, `ShaderColorProperties`, `ShaderVectorProperties` and `ShaderTextureProperties`.  
They must be followed by pairs consisting of the shader property name as the first line and the property value for the second line.  
`ShaderColorProperties` parses the property value from a set of three numbers (for the R, G and B values) ranging from 0 to 1. You can also optionally give a fourth number for the alpha value.
`ShaderVectorProperties` parses the property value from a set of up to 4 numbers (for the X, Y, Z and W values). Coordinate values that aren't given will default to 0.
`ShaderTextureProperties` will use the given value to load a texture from the hat's folder. The loaded texture will be used as the value. Example:
```
# ShaderName
Brave/PlayerShaderEevee

# ShaderTextureProperties
_PortalTex
nebula_reducednoise
```
This will set the shader's `_PortalTex` property to the texture in the file `nebula_reducednoise.png` located in the hat's folder.

# Soundbank Loading
Hat Loader will automatically load soundbank files ending in `-hatsb.bnk`. You can use this to add custom sounds to your hats with the `FlipStartSound` and `FlipEndSound` properties.  
To easily make custom soundbanks you can use Captain Pretzel's `gen-gungeon-audio-bank` script, found [here](https://github.com/pcrain/gungeon-modding-tools).

# Asset Bundle Shader Loading
Hat Loader will automatically load asset bundles ending in `*-hatbundle`. Since custom shaders only work if the asset bundle they're in was built for the player's platform, Hat Loader will load asset bundles from different folders depending on the current platform:
 * Windows - `HatBundlesWindows`
 * MacOS - `HatBundlesMacOS`
 * Linux - `HatBundlesLinux`
 
To make your custom shaders work for all players, you need to build your asset bundles for all 3 of those platforms (`BuildTarget.StandaloneWindows`, `BuildTarget.StandaloneOSX` and `BuildTarget.StandaloneLinux` respectively) and place them in those folders.

# Random Hats List
Hat Loader adds the random hats list, allowing you to pick a list of hats to randomly give the player when starting a new run. These commands can be used to interact with the random hats list:
 * `randomhats addcurrent` - Adds the player's current hat to the random hats list.
 * `randomhats setcurrent` - Sets the random hats list to only contain the player's current hat.
 * `randomhats removecurrent` - Removes the player's current hat from the random hats list.
 * `randomhats clear` - Removes all hats from the random hats list.
 * `randomhats log` - Logs the current hats list to the console.