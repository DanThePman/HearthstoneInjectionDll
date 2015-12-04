using PegasusShared;
using System;
using System.Collections.Generic;

public static class DbfUtils
{
    private static void AddDbfField(DbfRecord record, string columnName, object val)
    {
        DbfField field = CreateDbfField(columnName, val);
        if (field != null)
        {
            record.AddField(field);
        }
    }

    private static void AddLocStrings(DbfRecord record, List<LocalizedString> protoStrings)
    {
        for (int i = 0; i < protoStrings.Count; i++)
        {
            List<DbfLocValue> val = new List<DbfLocValue>();
            LocalizedString str = protoStrings[i];
            string key = str.Key;
            for (int j = 0; j < str.Values.Count; j++)
            {
                LocalizedStringValue value2 = str.Values[j];
                Locale locale = (Locale) value2.Locale;
                DbfLocValue item = new DbfLocValue();
                item.SetLocale(locale);
                item.SetValue(TextUtils.DecodeWhitespaces(value2.Value));
                val.Add(item);
            }
            DbfField field = new DbfField();
            field.SetColumnName(key);
            field.SetValue(val);
            record.AddField(field);
        }
    }

    public static DbfRecord ConvertFromProtobuf(ScenarioDbRecord protoScenario)
    {
        if (protoScenario == null)
        {
            return null;
        }
        DbfRecord record = new DbfRecord();
        AddDbfField(record, "ID", protoScenario.Id);
        AddDbfField(record, "NOTE_DESC", protoScenario.NoteDesc);
        AddDbfField(record, "PLAYERS", protoScenario.NumPlayers);
        AddDbfField(record, "PLAYER1_HERO_CARD_ID", (int) protoScenario.Player1HeroCardId);
        AddDbfField(record, "PLAYER2_HERO_CARD_ID", (int) protoScenario.Player2HeroCardId);
        AddDbfField(record, "IS_EXPERT", protoScenario.IsExpert);
        AddDbfField(record, "IS_COOP", !protoScenario.HasIsCoop ? ((object) 0) : ((object) protoScenario.IsCoop));
        AddDbfField(record, "ADVENTURE_ID", protoScenario.AdventureId);
        AddDbfField(record, "MODE_ID", !protoScenario.HasAdventureModeId ? null : ((object) protoScenario.AdventureModeId));
        AddDbfField(record, "WING_ID", protoScenario.WingId);
        AddDbfField(record, "SORT_ORDER", protoScenario.SortOrder);
        AddDbfField(record, "CLIENT_PLAYER2_HERO_CARD_ID", !protoScenario.HasClientPlayer2HeroCardId ? null : ((object) ((int) protoScenario.ClientPlayer2HeroCardId)));
        AddDbfField(record, "TB_TEXTURE", protoScenario.TavernBrawlTexture);
        AddDbfField(record, "TB_TEXTURE_PHONE", protoScenario.TavernBrawlTexturePhone);
        AddDbfField(record, "TB_TEXTURE_PHONE_OFFSET_Y", !protoScenario.HasTavernBrawlTexturePhoneOffset ? null : ((object) protoScenario.TavernBrawlTexturePhoneOffset.Y));
        AddLocStrings(record, protoScenario.Strings);
        return record;
    }

    private static DbfField CreateDbfField(string columnName, object val)
    {
        if (val == null)
        {
            return null;
        }
        DbfField field = new DbfField();
        field.SetColumnName(columnName);
        field.SetValue(val);
        return field;
    }

    public static string MakeFieldKey(DbfField field)
    {
        if (field == null)
        {
            return null;
        }
        return MakeFieldKey(field.GetColumnName());
    }

    public static string MakeFieldKey(string columnName)
    {
        if (columnName == null)
        {
            return null;
        }
        return columnName.ToUpperInvariant();
    }
}

