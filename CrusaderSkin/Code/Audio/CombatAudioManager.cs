using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using CrusaderSkin.Code.ModConfig;

namespace CrusaderSkin.Code.Audio;

public static class CombatAudioManager
{
    private static AudioStreamPlayer? _cardSelectAudioPlayer;
    public static AudioStreamPlayer CardSelectAudioPlayer
    {
        get
        {
            // IsInstanceValid 能同时检测 null 和已被 QueueFree 的对象
            if (!GodotObject.IsInstanceValid(_cardSelectAudioPlayer))
            {
                _cardSelectAudioPlayer = new AudioStreamPlayer();
            }
            return _cardSelectAudioPlayer;
        }
    }

    private static AudioStreamPlayer? _cardPlayAudioPlayer;
    public static AudioStreamPlayer CardPlayAudioPlayer
    {
        get
        {
            // IsInstanceValid 能同时检测 null 和已被 QueueFree 的对象
            if (!GodotObject.IsInstanceValid(_cardPlayAudioPlayer))
            {
                _cardPlayAudioPlayer = new AudioStreamPlayer();
            }
            return _cardPlayAudioPlayer;
        }
    }
    private static AudioStreamPlayer? _cardPlayRecoverAudioPlayer;
    public static AudioStreamPlayer CardPlayRecoverAudioPlayer
    {
        get
        {
            // IsInstanceValid 能同时检测 null 和已被 QueueFree 的对象
            if (!GodotObject.IsInstanceValid(_cardPlayRecoverAudioPlayer))
            {
                _cardPlayRecoverAudioPlayer = new AudioStreamPlayer();
            }
            return _cardPlayRecoverAudioPlayer;
        }
    }

    public static void PlayCombatSfx(string AudioName, bool bIsTemp = false, bool bIsFullPathName = false, float VolumeDb = -10.0f, int audioPlayerIndex = 0) //声音素材音量太大了，减小一点
    {
        if (NonInteractiveMode.IsActive) return;

        AudioStream stream;
        string path = bIsFullPathName ? AudioName : CombatAudioCfg.GetCrusaderPath(AudioName);

        if (path == null || path == "") return;
        try
        {
            stream = PreloadManager.Cache.GetAsset<AudioStream>(path);
        }
        catch
        {
            Log.Info($"[FlagellantCombatAudioManager] Could not load audio: {path}");
            return;
        }

        //单例模式的audioPlayer，不要手动释放
        var audioPlayer = bIsTemp ? new AudioStreamPlayer() : GetPlayerAudioPlayer(audioPlayerIndex);

        audioPlayer.VolumeDb = 0.0f;
        audioPlayer.VolumeDb += VolumeDb + CrusaderSettings.CardVolumeOffset;
        audioPlayer.Stream = stream;
        audioPlayer.Bus = "SFX";
        if (bIsTemp)
        {
            audioPlayer.Finished += () => audioPlayer.QueueFreeSafely();
        }

        var combatRoom = NCombatRoom.Instance;
        if (combatRoom != null)
        {
            if (audioPlayer.GetParent() == null)
            {
                combatRoom.AddChild(audioPlayer);
            }
            audioPlayer.Play();
        }
        else if (bIsTemp)
        {
            audioPlayer.QueueFreeSafely();
        }
    }

    public static void StopPlayingCombatSfx(int index = 0)
    {
        switch (index)
        {
            case 0:
            default:
                CardSelectAudioPlayer.Stop();
                return;
            case 1:
                CardPlayAudioPlayer.Stop();
                return;
            case 2:
                CardPlayRecoverAudioPlayer.Stop();
                return;
        }
    }

    private static AudioStreamPlayer GetPlayerAudioPlayer(int index = 0)
    {
        switch (index)
        {
            case 0:
            default:
                return CardSelectAudioPlayer;
            case 1:
                return CardPlayAudioPlayer;
            case 2:
                return CardPlayRecoverAudioPlayer;
        }
    }
}