using System;

public static class CustomEditTypeUtils
{
    public static string GetExtension(EditType type)
    {
        switch (type)
        {
            case EditType.MATERIAL:
                return "mat";

            case EditType.TEXTURE:
            case EditType.CARD_TEXTURE:
                return "psd";
        }
        return "prefab";
    }
}

