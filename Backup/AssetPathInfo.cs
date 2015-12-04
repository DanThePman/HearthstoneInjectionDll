using System;

public class AssetPathInfo
{
    public static readonly Map<AssetFamily, AssetFamilyPathInfo> FamilyInfo;

    static AssetPathInfo()
    {
        Map<AssetFamily, AssetFamilyPathInfo> map = new Map<AssetFamily, AssetFamilyPathInfo>();
        AssetFamilyPathInfo info = new AssetFamilyPathInfo {
            format = "Data/Actors/{0}.unity3d",
            sourceDir = "Assets/Game/Actors"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.Actor, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Boards/{0}.unity3d",
            sourceDir = "Assets/Game/Boards"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.Board, info);
        info = new AssetFamilyPathInfo {
            format = "Data/CardBacks/{0}.unity3d",
            sourceDir = "Assets/Game/CardBacks"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.CardBack, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Cards/{0}_prefab.unity3d",
            sourceDir = "Assets/Game/Cards"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.CardPrefab, info);
        info = new AssetFamilyPathInfo {
            format = "{0}",
            sourceDir = string.Empty
        };
        info.exts = new string[] { "mat" };
        map.Add(AssetFamily.CardPremium, info);
        info = new AssetFamilyPathInfo {
            format = "{0}",
            sourceDir = string.Empty
        };
        info.exts = new string[] { "psd", "png", "tif" };
        map.Add(AssetFamily.CardTexture, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Cards/{0}_xml.unity3d",
            sourceDir = "Assets/Game/Cards"
        };
        info.exts = new string[] { "xml" };
        map.Add(AssetFamily.CardXML, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Fonts/{0}.unity3d",
            sourceDir = "Assets/Game/Fonts"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.FontDef, info);
        info = new AssetFamilyPathInfo {
            format = "Data/GameObjects/{0}.unity3d",
            sourceDir = "Assets/Game/GameObjects"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.GameObject, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Movies/{0}.unity3d",
            sourceDir = "Assets/Game/Movies"
        };
        info.exts = new string[] { "ogv" };
        map.Add(AssetFamily.Movie, info);
        info = new AssetFamilyPathInfo {
            format = "Data/UIScreens/{0}.unity3d",
            sourceDir = "Assets/Game/UIScreens"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.Screen, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Sounds/{0}.unity3d",
            sourceDir = "Assets/Game/Sounds"
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.Sound, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Spells/{0}.unity3d",
            sourceDir = string.Empty
        };
        info.exts = new string[] { "prefab" };
        map.Add(AssetFamily.Spell, info);
        info = new AssetFamilyPathInfo {
            format = "Data/Textures/{0}.unity3d",
            sourceDir = "Assets/Game/Textures"
        };
        info.exts = new string[] { "psd" };
        map.Add(AssetFamily.Texture, info);
        FamilyInfo = map;
    }
}

