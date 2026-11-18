using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CrusaderSkin.Code.ModConfig;

public static class RitsuLibModConfig
{
	private const string ModId = "CrusaderSkin";

	private static readonly object FileLock = new object();

	private static readonly ConcurrentDictionary<string, JsonNode?> Hot = new ConcurrentDictionary<string, JsonNode>(StringComparer.Ordinal);

	private static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>
	{
		["CardVolumeOffset"] = 0,
		["PlayCardSfx"] = true,
        ["InterruptRecoverSfx"] = true,
        ["PlayCardAnims"] = true,
        ["PlayCardVfx"] = true,
        ["PlaySmiteRecoverVfx"] = true,
        ["UseLowHealthIdle"] = true,
		["SwitchBannerImmediately"] = false,
        ["SwitchScriptureImmediately"] = false,
        ["ShowTeammateCardSelectAnim"] = true,
		["ShowHitAnimIfFullyBlocked"] = false,
        ["CurrentSkin"] = "Default"  //注意，dropdown的selected返回的居然是options的label(value)而不是value(key)
    };

	private static string DataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlayTheSpire2", ModId);

	private static string SchemaPath => Path.Combine(DataDir, "ritsu_interop_schema.json");

	private static string StatePath => Path.Combine(DataDir, "ritsu_interop_state.json");

	public static object CreateRitsuLibSettingsSchema()
	{
		Directory.CreateDirectory(DataDir);
		File.WriteAllText(SchemaPath, BuildDefaultSchemaJson());
		return SchemaPath;
	}

	private static string BuildDefaultSchemaJson()
	{
		RitsuLibModConfigEntity ritsuLibModConfigEntity = new RitsuLibModConfigEntity();
		ritsuLibModConfigEntity.modDisplayName = SimpleLocUtil.Simple("十字军皮肤", "Crusader Skin");
		RitsuLibModConfigEntity configEntity = ritsuLibModConfigEntity;
		RLMCPage mainPage = new RLMCPage();
        mainPage.pageId = "main";
        mainPage.title = SimpleLocUtil.Simple("主要设置", "Main");
        mainPage.description = SimpleLocUtil.Simple("主要设置", "Main");
        mainPage.sortOrder = 1;

        RLMCSection skinChangeSec = new RLMCSection();
        skinChangeSec.id = "skin_change";
        skinChangeSec.title = SimpleLocUtil.Simple("切换十字军皮肤", "Change Crusader Skin");
        DropdownEntry dropdown0 = new DropdownEntry();
        dropdown0.id = "CurrentSkin";
        dropdown0.key = dropdown0.id;
        dropdown0.label = SimpleLocUtil.Simple("十字军皮肤类型", "Crusader Skin Type");
        dropdown0.options = new Dictionary<string, string> {
			//"value" , "label"
			{"Initial", SimpleLocUtil.Simple("默认皮肤", "Default")},
            {"Aggressor", SimpleLocUtil.Simple("侵略者", "Aggressor")},
            {"Banneret", SimpleLocUtil.Simple("方旗骑士", "Banneret")},
            {"Templar", SimpleLocUtil.Simple("圣殿骑士", "Templar")},
            {"Infernal", SimpleLocUtil.Simple("渊狱", "Infernal")}
        };
        skinChangeSec.entries.Add(dropdown0);

        RLMCSection cardSfxSec = new RLMCSection();
        cardSfxSec.id = "card_sfx";
        cardSfxSec.title = SimpleLocUtil.Simple("卡牌音效", "Card Sfx");
        SliderEntry sliderEntry = new SliderEntry();
        sliderEntry.id = "CardVolumeOffset";
        sliderEntry.key = sliderEntry.id;
        sliderEntry.label = SimpleLocUtil.Simple("卡牌音量调节(分贝)", "Card Volume DB Offset");
        sliderEntry.description = SimpleLocUtil.Simple("默认值0已经很大声，注意保护耳朵", "Default Value 0 DB is already loud. Be careful to protect your ears.");
        sliderEntry.min = -20.0;
        sliderEntry.max = 20.0;
        sliderEntry.step = 1.0;
        cardSfxSec.entries.Add(sliderEntry);
        ToggleEntry toggle0 = new ToggleEntry();
		toggle0.id = "PlayCardSfx";
		toggle0.key = toggle0.id;
        toggle0.label = SimpleLocUtil.Simple("播放卡牌音效", "Play Card Sfx");
        toggle0.description = SimpleLocUtil.Simple("关闭后会恢复至原版游戏默认攻击音效", "If unchecked, attack cards will use the original SFX.");
        cardSfxSec.entries.Add(toggle0);
        ToggleEntry toggle1 = new ToggleEntry();
        toggle1.id = "InterruptRecoverSfx";
        toggle1.key = toggle1.id;
        toggle1.label = SimpleLocUtil.Simple("播放新动画时立刻打断收尾动画的音效", "Interrupt Recover Sfx When Playing New Anims");
        toggle1.description = SimpleLocUtil.Simple("关闭后不会打断收尾动画音效", "If unchecked, recover Sfx will not be interrupted.");
        cardSfxSec.entries.Add(toggle1);

        RLMCSection cardVfxSec = new RLMCSection();
        cardVfxSec.id = "card_vfx";
        cardVfxSec.title = SimpleLocUtil.Simple("卡牌动画和特效", "Card Anim And Vfx");
        ToggleEntry toggle2 = new ToggleEntry();
        toggle2.id = "PlayCardAnims";
        toggle2.key = toggle2.id;
        toggle2.label = SimpleLocUtil.Simple("播放卡牌动画", "Play Card Anims");
        toggle2.description = SimpleLocUtil.Simple("关闭后不会播放卡牌的选择和打出动画,卡牌特效以及音效", "If unchecked, card select/play anims, VFX and SFX wil not play.");
        cardVfxSec.entries.Add(toggle2);
        ToggleEntry toggle3 = new ToggleEntry();
        toggle3.id = "PlayCardVfx";
        toggle3.key = toggle3.id;
        toggle3.label = SimpleLocUtil.Simple("播放卡牌特效", "Play Card Vfx");
        toggle3.description = SimpleLocUtil.Simple("关闭后不会播放卡牌的打出特效", "If unchecked, card play VFX wil not play.");
        cardVfxSec.entries.Add(toggle3);
        ToggleEntry toggle4 = new ToggleEntry();
        toggle4.id = "PlaySmiteRecoverVfx";
        toggle4.key = toggle4.id;
        toggle4.label = SimpleLocUtil.Simple("播放惩击收尾动画特效", "Play Smite Recover Anim Vfx");
        toggle4.description = SimpleLocUtil.Simple("关闭后不会播放惩击收尾动画的特效", "If unchecked, smite recover anim VFX wil not play.");
        cardVfxSec.entries.Add(toggle4);

        RLMCSection animSec = new RLMCSection();
        animSec.id = "character_anim";
        animSec.title = SimpleLocUtil.Simple("角色动画", "Character Anim");
        ToggleEntry toggle5 = new ToggleEntry();
        toggle5.id = "UseLowHealthIdle";
        toggle5.key = toggle5.id;
        toggle5.label = SimpleLocUtil.Simple("展示低血量站立动画", "Show Low Health Idle Anim");
        toggle5.description = SimpleLocUtil.Simple("关闭后不会使用低血量站立动画", "If unchecked, low health idle anim wil not play.");
        animSec.entries.Add(toggle5);
        ToggleEntry toggle6 = new ToggleEntry();
        toggle6.id = "SwitchBannerImmediately";
        toggle6.key = toggle6.id;
        toggle6.label = SimpleLocUtil.Simple("立即切换旗帜动画", "Switch Banner Immediately");
        toggle6.description = SimpleLocUtil.Simple("开启后播放旗帜动画时不会混合过渡", "If checked, banner anims will have no mixed transition.");
        animSec.entries.Add(toggle6);
        ToggleEntry toggle7 = new ToggleEntry();
        toggle7.id = "SwitchScriptureImmediately";
        toggle7.key = toggle7.id;
        toggle7.label = SimpleLocUtil.Simple("立即切换卷轴动画", "Switch Scripture Immediately");
        toggle7.description = SimpleLocUtil.Simple("开启后播放卷轴动画时不会混合过渡", "If checked, scripture anims will have no mixed transition.");
        animSec.entries.Add(toggle7);
        ToggleEntry toggle8 = new ToggleEntry();
        toggle8.id = "ShowTeammateCardSelectAnim";
        toggle8.key = toggle8.id;
        toggle8.label = SimpleLocUtil.Simple("展示队友十字军的选卡动画", "Show Teammate Crusader Card Select Anim");
        toggle8.description = SimpleLocUtil.Simple("关闭后不会播放队友十字军的选卡动画", "If unchecked, teammate Crusader card select anims will not play.");
        animSec.entries.Add(toggle8);
        ToggleEntry toggle9 = new ToggleEntry();
        toggle9.id = "ShowHitAnimIfFullyBlocked";
        toggle9.key = toggle9.id;
        toggle9.label = SimpleLocUtil.Simple("完全格挡时播放受击动画", "Show Hit Anim If Fully Blocked");
        toggle9.description = SimpleLocUtil.Simple("开启后完全格挡伤害时也会播放受击动画", "If checked, hit anim will play even if damage is fully blocked.");
        animSec.entries.Add(toggle9);

        mainPage.sections.Add(skinChangeSec);
        mainPage.sections.Add(cardSfxSec);
        mainPage.sections.Add(cardVfxSec);
        mainPage.sections.Add(animSec);
        configEntity.pages.Add(mainPage);
		return JsonSerializer.Serialize(configEntity);
	}

	public static void SetRitsuLibSettingValue(string key, object? value)
	{
		SetCore(key, value);
	}

	public static object? GetRitsuLibSettingValue(string key)
	{
		return GetCore(key);
	}

	public static void SaveRitsuLibSettings()
	{
		lock (FileLock)
		{
			string statePath = StatePath;
			JsonObject jsonObject = new JsonObject();
			foreach (KeyValuePair<string, JsonNode> item in Hot)
			{
				jsonObject[item.Key] = ((item.Value == null) ? null : JsonNode.Parse(item.Value.ToJsonString()));
			}
			File.WriteAllText(statePath, jsonObject.ToJsonString(new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
	}

	public static bool GetRitsuLibSettingBool(string key)
	{
		return CoerceBool(GetCore(key));
	}

	public static void SetRitsuLibSettingBool(string key, bool value)
	{
		SetCore(key, value);
	}

	public static double GetRitsuLibSettingDouble(string key)
	{
		return CoerceDouble(GetCore(key));
	}

	public static void SetRitsuLibSettingDouble(string key, double value)
	{
		SetCore(key, value);
	}

	public static int GetRitsuLibSettingInt(string key)
	{
		return CoerceInt(GetCore(key));
	}

	public static void SetRitsuLibSettingInt(string key, int value)
	{
		SetCore(key, value);
	}

	public static string? GetRitsuLibSettingString(string key)
	{
		return GetCore(key)?.ToString();
	}

	public static void SetRitsuLibSettingString(string key, string value)
	{
		SetCore(key, value);
	}

	public static void InvokeRitsuLibSettingAction(string key)
	{
		if (!(key == "reset_all"))
		{
			return;
		}
		Hot.Clear();
		try
		{
			if (File.Exists(StatePath))
			{
				File.Delete(StatePath);
			}
		}
		catch
		{
		}
	}

	private static void SetCore(string key, object? value)
	{
		LoadIfNeeded();
		Hot[key] = JsonSerializer.SerializeToNode(value);
	}

	private static object? GetCore(string key)
	{
		LoadIfNeeded();
		if (!Hot.TryGetValue(key, out JsonNode value) || value == null)
		{
			return Defaults.GetValueOrDefault(key);
		}
		if (value is JsonValue jsonValue)
		{
			return jsonValue.GetValue<object>();
		}
		return value;
	}

	private static void LoadIfNeeded()
	{
		if (Hot.Count > 0)
		{
			return;
		}
		lock (FileLock)
		{
			if (Hot.Count > 0 || !File.Exists(StatePath))
			{
				return;
			}
			JsonObject jsonObject = JsonNode.Parse(File.ReadAllText(StatePath))?.AsObject();
			if (jsonObject == null)
			{
				return;
			}
			foreach (KeyValuePair<string, JsonNode> item in jsonObject)
			{
				Hot[item.Key] = item.Value;
			}
		}
	}

	private static bool CoerceBool(object? o)
	{
		if (o is bool)
		{
			if ((bool)o)
			{
				return true;
			}
			return false;
		}
		if (o != null)
		{
			if (o is JsonValue jsonValue)
			{
				bool value;
				return jsonValue.TryGetValue<bool>(out value) && value;
			}
			bool result;
			return bool.TryParse(o.ToString(), out result) && result;
		}
		return false;
	}

	private static double CoerceDouble(object? o)
	{
		if (o != null)
		{
			if (!(o is JsonValue jsonValue))
			{
				if (o is IConvertible value)
				{
					return Convert.ToDouble(value);
				}
				double result;
				return double.TryParse(o.ToString(), out result) ? result : 0.0;
			}
			return jsonValue.GetValue<double>();
		}
		return 0.0;
	}

	private static int CoerceInt(object? o)
	{
		if (o != null)
		{
			if (!(o is JsonValue jsonValue))
			{
				if (o is IConvertible value)
				{
					return Convert.ToInt32(value);
				}
				int result;
				return int.TryParse(o.ToString(), out result) ? result : 0;
			}
			return jsonValue.GetValue<int>();
		}
		return 0;
	}
}
