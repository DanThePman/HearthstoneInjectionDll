using System;

public static class InitialConversationLines
{
    public static readonly string[][] BRM_INITIAL_CONVO_LINES;
    public static readonly string[][] LOE_INITIAL_CONVO_LINES;

    static InitialConversationLines()
    {
        string[][] textArrayArray1 = new string[5][];
        textArrayArray1[0] = new string[] { "NormalNefarian_Quote", "VO_NEFARIAN_INTRO1_24" };
        textArrayArray1[1] = new string[] { "Ragnaros_Quote", "VO_RAGNAROS_INTRO2_64" };
        textArrayArray1[2] = new string[] { "NormalNefarian_Quote", "VO_NEFARIAN_INTRO3_25" };
        textArrayArray1[3] = new string[] { "NormalNefarian_Quote", "VO_NEFARIAN_INTRO4_26" };
        textArrayArray1[4] = new string[] { "Ragnaros_Quote", "VO_RAGNAROS_INTRO5_65" };
        BRM_INITIAL_CONVO_LINES = textArrayArray1;
        string[][] textArrayArray2 = new string[2][];
        textArrayArray2[0] = new string[] { "Cartographer_Quote", "VO_LOE_INTRO_1" };
        textArrayArray2[1] = new string[] { "Cartographer_Quote", "VO_LOE_INTRO_3" };
        LOE_INITIAL_CONVO_LINES = textArrayArray2;
    }
}

