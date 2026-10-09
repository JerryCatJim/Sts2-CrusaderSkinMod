namespace CrusaderSkin.Code.ModConfig;

public static class CrusaderSettings
{
    public static float CardVolumeOffset => (float)RitsuLibModConfig.GetRitsuLibSettingDouble("CardVolumeOffset");
    public static bool PlayCardSfx => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardSfx");
    public static bool PlayCardAnims => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardAnims");
    public static bool PlayCardVfx => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardVfx");
    public static bool UseLowHealthIdle => RitsuLibModConfig.GetRitsuLibSettingBool("UseLowHealthIdle");
    public static bool SwitchBannerImmediately => RitsuLibModConfig.GetRitsuLibSettingBool("SwitchBannerImmediately");
    public static bool ShowTeammateCardSelectAnim => RitsuLibModConfig.GetRitsuLibSettingBool("ShowTeammateCardSelectAnim");
    public static string CurrentSkin => RitsuLibModConfig.GetRitsuLibSettingString("CurrentSkin"); //返回的居然是options的label(value)而不是value(key)

    public static bool ToggleEnabled(string key)
    {
        return RitsuLibModConfig.GetRitsuLibSettingBool(key);
    }
}
