using System;

[Serializable]
public class DevicePreset : ICloneable
{
    public InputCategory input;
    public string name = "No Emulation";
    public OSCategory os = OSCategory.PC;
    public ScreenCategory screen = ScreenCategory.PC;
    public ScreenDensityCategory screenDensity = ScreenDensityCategory.High;

    public object Clone()
    {
        return base.MemberwiseClone();
    }

    public void ReadFromConfig(ConfigFile config)
    {
        this.name = config.Get("Emulation.DeviceName", this.name.ToString());
        string str = config.Get("Emulation.OSCategory", this.os.ToString());
        string str2 = config.Get("Emulation.InputCategory", this.input.ToString());
        string str3 = config.Get("Emulation.ScreenCategory", this.screen.ToString());
        string str4 = config.Get("Emulation.ScreenDensityCategory", this.screenDensity.ToString());
        Log.ConfigFile.Print("Reading Emulated Device: " + this.name + " from " + config.GetPath(), new object[0]);
        try
        {
            this.os = (OSCategory) ((int) Enum.Parse(typeof(OSCategory), str));
            this.input = (InputCategory) ((int) Enum.Parse(typeof(InputCategory), str2));
            this.screen = (ScreenCategory) ((int) Enum.Parse(typeof(ScreenCategory), str3));
            this.screenDensity = (ScreenDensityCategory) ((int) Enum.Parse(typeof(ScreenDensityCategory), str4));
        }
        catch (ArgumentException)
        {
            object[] args = new object[] { this.name, config.GetPath() };
            Blizzard.Log.Warning("Could not parse {0} in {1} as a valid device!", args);
        }
    }

    public void WriteToConfig(ConfigFile config)
    {
        Log.ConfigFile.Print("Writing Emulated Device: " + this.name + " to " + config.GetPath(), new object[0]);
        config.Set("Emulation.DeviceName", this.name.ToString());
        config.Set("Emulation.OSCategory", this.os.ToString());
        config.Set("Emulation.InputCategory", this.input.ToString());
        config.Set("Emulation.ScreenCategory", this.screen.ToString());
        config.Set("Emulation.ScreenDensityCategory", this.screenDensity.ToString());
        config.Save(null);
    }
}

