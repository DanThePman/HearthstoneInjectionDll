using System;

public class MusicStingerSpell : CardSoundSpell
{
    public MusicStingerData m_MusicStingerData = new MusicStingerData();

    private bool CanPlay()
    {
        if (GameState.Get() == null)
        {
            return true;
        }
        Player controller = base.GetSourceCard().GetController();
        return ((controller == null) || controller.IsLocalUser());
    }
}

