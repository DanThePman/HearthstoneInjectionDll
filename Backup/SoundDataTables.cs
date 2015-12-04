using System;

public class SoundDataTables
{
    public static readonly Map<SoundCategory, Option> s_categoryEnabledOptionMap;
    public static readonly Map<SoundCategory, Option> s_categoryVolumeOptionMap;

    static SoundDataTables()
    {
        Map<SoundCategory, Option> map = new Map<SoundCategory, Option>();
        map.Add(SoundCategory.MUSIC, Option.MUSIC);
        map.Add(SoundCategory.SPECIAL_MUSIC, Option.MUSIC);
        map.Add(SoundCategory.HERO_MUSIC, Option.MUSIC);
        s_categoryEnabledOptionMap = map;
        map = new Map<SoundCategory, Option>();
        map.Add(SoundCategory.MUSIC, Option.MUSIC_VOLUME);
        map.Add(SoundCategory.SPECIAL_MUSIC, Option.MUSIC_VOLUME);
        map.Add(SoundCategory.HERO_MUSIC, Option.MUSIC_VOLUME);
        s_categoryVolumeOptionMap = map;
    }
}

