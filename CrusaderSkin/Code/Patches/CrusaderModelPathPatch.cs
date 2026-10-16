using CrusaderSkin.Code.ModConfig;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
namespace CrusaderSkin.Code.Patches;

public static class TscnPathGetter
{
    public static string GetPath()
    {
        return CrusaderSettings.CurrentSkin switch
        {
            //与RitsuLibModConfig.cs里的dropdown0写的options对应
            "默认皮肤" => "",
            "Default" => "",
            "侵略者" => "_aggressor",
            "Aggressor" => "_aggressor",
            "方旗骑士" => "_banneret",
            "Banneret" => "_banneret",
            "圣殿骑士" => "_templar",
            "Templar" => "_templar",
            "渊狱" => "_infernal",
            "Infernal" => "_infernal",
            _ => ""
        };
    }
}

[HarmonyPatch(typeof(CharacterModel), "VisualsPath", MethodType.Getter)]
class CustomCharacterVisualPath
{
    [HarmonyPrefix]
    static bool UseCustomScene(CharacterModel __instance, ref string? __result)
    {
        if (__instance is not Ironclad) return true;

        if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin)) return true;
        if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default") return true;

        __result = SceneHelper.GetScenePath("creature_visuals/ironclad" + TscnPathGetter.GetPath());
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.RestSiteAnimPath), MethodType.Getter)]
class RestSiteAnimPath
{
    [HarmonyPrefix]
    static bool Custom(CharacterModel __instance, ref string? __result)
    {
        if (__instance is not Ironclad) return true;

        if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin)) return true;
        if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default") return true;

        __result = SceneHelper.GetScenePath("rest_site/characters/ironclad" + TscnPathGetter.GetPath() + "_rest_site");
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.MerchantAnimPath), MethodType.Getter)]
class MerchantAnimPath
{
    [HarmonyPrefix]
    static bool Custom(CharacterModel __instance, ref string? __result)
    {
        if (__instance is not Ironclad) return true;

        if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin)) return true;
        if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default") return true;

        __result = SceneHelper.GetScenePath("merchant/characters/ironclad" + TscnPathGetter.GetPath() + "_merchant");
        return false;
    }
}