using System.Collections.Generic;

namespace HearthmoonLib
{
    public interface IBehaviour
    {
        IEnumerable<BotPlay> OnQueryBotPlays();

        Card OnQueryCardChoice();
    }

    public interface IMulligan
    {
        IEnumerable<Card> OnQueryMulligan();
    }

    public static class Core
    {
        public static IBehaviour ExternBehaviour;
        public static IMulligan ExternMulligan;
    }

    
}
