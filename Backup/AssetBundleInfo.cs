using System;
using UnityEngine;

public class AssetBundleInfo
{
    public static readonly Map<AssetFamily, AssetFamilyBundleInfo> FamilyInfo;
    public const int NUM_ACTOR_BUNDLES = 2;
    public const int NUM_BUNDLES_DEFAULT = 1;
    public const int NUM_CARD_BUNDLES = 3;
    public const int NUM_CARDBACKS_BUNDLES = 1;
    public const int NUM_CARDTEXTURES_BUNDLES = 3;
    public const int NUM_DOWNLOADABLE_SOUND_LOCALE_BUNDLES = 3;
    public const int NUM_DOWNLOADABLE_SPELL_LOCALE_BUNDLES = 9;
    public const int NUM_GAMEOBJECTS_BUNDLES = 2;
    public const int NUM_MOVIE_BUNDLES = 1;
    public const int NUM_PREMIUMMATERIALS_BUNDLES = 2;
    public const int NUM_SOUND_BUNDLES = 2;
    public const int NUM_SPELL_BUNDLES = 3;
    public const int NumSharedDependencyBundles = 4;
    public const string SharedBundleName = "shared";
    public static readonly bool UseSharedDependencyBundle = true;

    static AssetBundleInfo()
    {
        Map<AssetFamily, AssetFamilyBundleInfo> map = new Map<AssetFamily, AssetFamilyBundleInfo>();
        AssetFamilyBundleInfo info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "actors",
            NumberOfBundles = 2
        };
        map.Add(AssetFamily.Actor, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "boards"
        };
        map.Add(AssetFamily.Board, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "cardbacks",
            NumberOfBundles = 1,
            Updatable = true
        };
        map.Add(AssetFamily.CardBack, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "cards",
            NumberOfBundles = 3
        };
        map.Add(AssetFamily.CardPrefab, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(Material),
            BundleName = "premiummaterials",
            NumberOfBundles = 2
        };
        map.Add(AssetFamily.CardPremium, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(Texture),
            BundleName = "cardtextures",
            NumberOfBundles = 3
        };
        map.Add(AssetFamily.CardTexture, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(TextAsset),
            BundleName = "cardxml",
            Updatable = true
        };
        map.Add(AssetFamily.CardXML, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "fonts"
        };
        map.Add(AssetFamily.FontDef, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "gameobjects",
            NumberOfBundles = 2
        };
        map.Add(AssetFamily.GameObject, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(MovieTexture),
            BundleName = "movies",
            NumberOfBundles = 1
        };
        map.Add(AssetFamily.Movie, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "uiscreens"
        };
        map.Add(AssetFamily.Screen, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "sounds",
            NumberOfBundles = 2,
            NumberOfDownloadableLocaleBundles = 3
        };
        map.Add(AssetFamily.Sound, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(GameObject),
            BundleName = "spells",
            NumberOfBundles = 3,
            NumberOfDownloadableLocaleBundles = 9
        };
        map.Add(AssetFamily.Spell, info);
        info = new AssetFamilyBundleInfo {
            TypeOf = typeof(Texture),
            BundleName = "textures"
        };
        map.Add(AssetFamily.Texture, info);
        FamilyInfo = map;
    }

    public static string BundlePathPlatformModifier()
    {
        return "win/";
    }
}

