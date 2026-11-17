using CrusaderSkin.Code.Audio;
using CrusaderSkin.Code.ModConfig;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrusaderSkin.Code.Helper;

public static class CrusaderHelper
{
    public static bool IsIronclad(Creature? creature)
    {
        return creature != null && creature.Player != null && creature.Player.Character is Ironclad;
    }
    public static bool IsIronclad(Player? player)
    {
        return player != null && player.Character is Ironclad;
    }
    public static bool IsCardOfIronclad(CardModel? cardModel)
    {
        return cardModel != null && cardModel.Pool is IroncladCardPool;
    }
    public static bool IsLowHealth(Creature? creature, decimal lowHealthPercent = 25m, decimal delta = 0m)
    {
        return creature != null && (creature.CurrentHp - delta) * 100m / creature.MaxHp <= lowHealthPercent;
    }
    public static bool IsHeavyHit(Creature? creature, decimal threshold = 30m)
    {
        if (creature == null) return false;

        return CombatManager.Instance.History.Entries.
            OfType<DamageReceivedEntry>().
            LastOrDefault(e => e.Result.TotalDamage >= threshold
            //&& e.Actor.IsMonster
            && e.Result.UnblockedDamage > 0m
            && e.Receiver == creature) != default;
    }
    public static void ResetAdvancedConditions(AnimationTree? animTree, Creature creature)
    {
        AnimationTree? animationTree = animTree;
        if (animationTree == null && creature != null)
        {
            NCreature? creatureNode = creature.GetCreatureNode();
            if (creatureNode != null)
            {
                animationTree = creatureNode.Visuals.GetNodeOrNull<AnimationTree>("AnimationTree");
            }
        }
        if (animationTree != null)
        {
            /*if (FlagellantConfig.ShouldUseDeathDoorIdle)
            {
                animationTree.Set("parameters/conditions/HitToIdle", !CrusaderHelper.IsLowHealth(creature));
                animationTree.Set("parameters/conditions/HitToDeathIdle", CrusaderHelper.IsLowHealth(creature));
            }
            else*/
            {
                //用进入Idle的信号来控制站立动画了，条件控制暂时没用了
                animationTree.Set("parameters/conditions/HitToIdle", true);
                animationTree.Set("parameters/conditions/HitToDeathIdle", false);
            }
        }
    }
    public static bool IsInAnyIdle(AnimationTree? animTree, Creature creature)
    {
        AnimationTree? animationTree = animTree;
        if (animationTree == null && creature != null)
        {
            NCreature? creatureNode = creature.GetCreatureNode();
            if (creatureNode != null)
            {
                animationTree = creatureNode.Visuals.GetNodeOrNull<AnimationTree>("AnimationTree");
            }
        }
        if (animationTree != null)
        {
            var state_machine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
            if (state_machine != null)
            {
                return state_machine.GetCurrentNode() == "Idle"; //||
                                                                 //state_machine.GetCurrentNode() == "Revive";
                                                                 //|| state_machine.GetFadingFromNode() == "Idle" || xxxxxx;
            }
        }
        return false;
    }
    public static void ClearDataCache()
    {
        _lastEffects.Clear();
    }
    public static void InitCrusaderVisualAfterCombatStart(AnimationTree? tree, Creature creature)
    {
        AnimationTree? animationTree = tree;
        NCreature? creatureNode = null;
        if (animationTree == null && creature != null)
        {
            creatureNode = creature.GetCreatureNode();
            if (creatureNode != null)
            {
                animationTree = creatureNode.Visuals.GetNodeOrNull<AnimationTree>("AnimationTree");
            }
        }
        if (animationTree != null && creatureNode != null)
        {
            var state_machine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
            if (state_machine != null)
            {
                var Idle_SM = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/Idle/playback");
                if (Idle_SM != null && IsLowHealth(creatureNode.Entity) && CrusaderSettings.UseLowHealthIdle)
                {
                    state_machine.Start("Idle");
                    Idle_SM.Travel("DeathIdle");
                }
            }
            SetBannerAndScriptureVisibility(creatureNode, "Idle");
            SetBladeFlameVisibility(creatureNode, "Idle");
        }
    }
    public static void SetBannerAndScriptureVisibility(NCreature nCreature, string animName)
    {
        if (nCreature == null) return;

        Node3D? Banner = nCreature.Visuals.GetNodeOrNull<Node3D>("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/msh_crusader_wpn_banner");
        Node3D? Scripture = nCreature.Visuals.GetNodeOrNull<Node3D>("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/msh_crusader_wpn_scripture");
        if (Banner != null)
        {
            Banner.Visible = ShouldShowBanner(animName);
        }
        if (Scripture != null)
        {
            Scripture.Visible = ShouldShowScripture(animName);
        }
    }
    public static void SetBladeFlameVisibility(NCreature nCreature, string animName)
    {
        if (nCreature == null) return;

        GpuParticles3D? BladeFlameLeft = nCreature.Visuals.GetNodeOrNull<GpuParticles3D>("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeFlameLeft");
        GpuParticles3D? BladeFlameRight = nCreature.Visuals.GetNodeOrNull<GpuParticles3D>("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeFlameRight");
        MeshInstance3D? BladeGlowBody = nCreature.Visuals.GetNodeOrNull<MeshInstance3D>("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeGlowBody");
        if (BladeFlameLeft != null && BladeFlameRight != null)
        {
            BladeFlameLeft.Emitting = ShouldShowBladeFlame(animName);
            BladeFlameRight.Emitting = ShouldShowBladeFlame(animName);
            BladeFlameLeft.Position = new Vector3(BladeFlameLeft.Position.X, GetBladeFlameParticlePositionY(animName), BladeFlameLeft.Position.Z);
            BladeFlameRight.Position = new Vector3(BladeFlameRight.Position.X, GetBladeFlameParticlePositionY(animName), BladeFlameRight.Position.Z);
        }
        if (BladeGlowBody != null)
        {
            BladeGlowBody.Visible = ShouldShowBladeFlame(animName);
        }
    }
    public static bool ShouldShowScripture(string animName)
    {
        if (!animName.Contains("CardPlay/")) return false;

        if (animName.Contains("Accus")
            || animName.Contains("Heal"))
        {
            return true;
        }
        return false;
    }
    public static bool ShouldShowBanner(string animName)
    {
        if (!animName.Contains("CardPlay/")) return false;

        if (animName.Contains("Insp")
            || animName.Contains("Rally")
            || animName.Contains("Tenacity"))
        {
            return true;
        }
        return false;
    }
    public static bool ShouldShowBladeFlame(string animName)
    {
        if (animName == "CardSelect/Holy"
            || animName == "CardPlay/Holy"
            || animName == "CardPlay/Radiance")
        {
            return true;
        }
        return false;
    }
    public static void TryPlayCombatEffect(string animName, NCreature nCreature)
    {
        if (!CrusaderSettings.PlayCardVfx) return;
        if (!CrusaderSettings.PlaySmiteRecoverVfx && animName == "CardPlay/Smite_Recover") return;

        ClearLastRecoverEffectAndSfx(nCreature, animName);

        string effectPath = "";
        bool shouldBackContainer = false;
        bool useCenterPos = false;
        switch (animName)
        {
            case "CardPlay/Smite":
                effectPath = "res://crusader_assets/Effects/SmiteEffect.tscn";
                break;
            case "CardPlay/Smite_Recover":
                effectPath = "res://crusader_assets/Effects/SmiteRecoverEffect.tscn";
                shouldBackContainer = true;
                break;
            case "CardPlay/Accus_Recover":
            case "CardPlay/Heal_Recover":
                effectPath = "res://crusader_assets/Effects/ScriptureRecoverEffect.tscn";
                break;
            case "CardPlay/Holy_Recover":
                effectPath = "res://crusader_assets/Effects/HolyRecoverEffect.tscn";
                break;
            case "CardPlay/Reap":
                effectPath = "res://crusader_assets/Effects/ReapEffect.tscn";
                break;
            case "CardPlay/Bulwark":
            case "CardPlay/Tenacity":
                effectPath = "res://crusader_assets/Effects/BulwarkEffect.tscn";
                shouldBackContainer = true;
                useCenterPos = true;
                break;
            case "CardPlay/Tenacity_Recover":
                effectPath = "res://crusader_assets/Effects/BannerRecoverEffect.tscn";
                shouldBackContainer = true;
                break;
            case "CardPlay/Insp":
                effectPath = "res://crusader_assets/Effects/InspEffect.tscn";
                shouldBackContainer = true;
                break;
            case "CardPlay/Insp_Recover":
                effectPath = "res://crusader_assets/Effects/BannerRecoverEffect.tscn";
                shouldBackContainer = true;
                break;
            case "CardPlay/Rally":
                effectPath = "res://crusader_assets/Effects/RallyEffect.tscn";
                break;
            case "CardPlay/Rally_Recover":
                effectPath = "res://crusader_assets/Effects/BannerRecoverEffect.tscn";
                shouldBackContainer = true;
                break;
            case "CardPlay/Stun":
                effectPath = "res://crusader_assets/Effects/StunEffect.tscn";
                break;
            case "CardPlay/Stun_Recover":
                effectPath = "res://crusader_assets/Effects/StunRecoverEffect.tscn";
                break;
            case "CardPlay/Mercy":
                effectPath = "res://crusader_assets/Effects/MercyEffect.tscn";
                shouldBackContainer = true;
                break;
            default:
                return;
        }
        LoadEffect(animName, effectPath, nCreature, shouldBackContainer, useCenterPos);
    }
    private class LastEffect
    {
        public LastEffect(bool IsRecoverEffect, Node2D EffectNode)
        {
            isRecoverEffect = IsRecoverEffect;
            effectNode = EffectNode;
        }
        public bool isRecoverEffect = false;
        public Node2D effectNode = null;
    }
    private static readonly Dictionary<NCreature, LastEffect> _lastEffects = new();
    private static void LoadEffect(string animName, string effectPath, NCreature nCreature, bool shouldBackContainer, bool useCenterPos)
    {
        Node2D effectNode = PreloadManager.Cache.GetScene(effectPath).Instantiate<Node2D>();
        if (effectNode == null) return;

        if (effectNode != null && effectNode.GetParent() == null && NCombatRoom.Instance != null && nCreature != null)
        {
            RegisterLastEffect(nCreature, animName.Contains("_Recover"), effectNode);
            var parentNode = shouldBackContainer ? NCombatRoom.Instance.BackCombatVfxContainer : NCombatRoom.Instance.CombatVfxContainer;
            parentNode.AddChild(effectNode);

            Marker2D centerPos = nCreature.Visuals.GetNodeOrNull<Marker2D>("%CenterPos");
            effectNode.GlobalPosition = centerPos != null && useCenterPos ? centerPos.GlobalPosition + GetEffectNodeOffSet(animName, nCreature) : nCreature.GlobalPosition;
            effectNode.Scale *= nCreature.Visuals.Scale;
            if (nCreature.Visuals.GetNodeOrNull<Node2D>("%Visuals") is Node2D visualsNode && visualsNode.Scale.X < 0)
            {
                //人物转身时特效也翻转
                effectNode.Scale *= new Vector2(-1, 1);
            }
            if (!shouldBackContainer)
            {
                //挂在BackCombatVfxContainer的特效无需注意镜头缩放
                effectNode.Scale *= nCreature.Entity.CombatState.Encounter.GetCameraScaling();
            }
        }

        var AnimPlayer = effectNode.GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");
        if (AnimPlayer != null)
        {
            AnimPlayer.Stop();
            AnimPlayer.Play("Show");
            AnimPlayer.AnimationFinished += (animName) =>
            {
                //每次都是新生成的特效结点，不用判断是否重复连接信号
                if (effectNode != null && !effectNode.IsQueuedForDeletion())
                {
                    effectNode.QueueFreeSafely();
                }
            };
        }
    }
    private static Vector2 GetEffectNodeOffSet(string animName, NCreature nCreature)
    {
        if (nCreature == null) return Vector2.Zero;
        Control bounds = nCreature.Visuals.GetNodeOrNull<Control>("%Bounds");
        if (bounds == null) return Vector2.Zero;

        float offSetX = 0.0f;
        float offSetY = 0.0f;

        //想偏移多少改这里
        float offSetScaleX = 0.0f;  //限定了float类型，防止直接填俩int进去相除还不知道咋回事呢
        float offSetScaleY = 0.0f;  //限定了float类型，防止直接填俩int进去相除还不知道咋回事呢
        switch (animName)
        {
            case "CardPlay/Bulwark":
                offSetScaleX = 1.2f;
                offSetScaleY = -2.5f;
                break;
            case "CardPlay/Tenacity":
                offSetScaleX = 0.0f;
                offSetScaleY = -2.5f;
                break;
            default:
                break;
        }
        offSetX = offSetScaleX != 0.0f ? bounds.Size.X * (offSetScaleX / 10.0f) * Math.Min(1.0f, nCreature.Visuals.Scale.X) : 0.0f;
        offSetY = offSetScaleY != 0.0f ? bounds.Size.Y * (offSetScaleY / 10.0f) * Math.Min(1.0f, nCreature.Visuals.Scale.Y) : 0.0f;
        return new Vector2(offSetX, offSetY);
    }
    private static float GetBladeFlameParticlePositionY(string animName)
    {
        float offSetY = 0.0f;
        switch (animName)
        {
            case "CardPlay/Radiance":
                offSetY = -0.1f;
                break;
            default:
                offSetY = 0.1f;
                break;
        }
        return offSetY;
    }
    public static bool CanTravelTo(string nodeName)
    {
        switch (nodeName)
        {
            case "Smite":
            case "Stun":
            case "Accus":
            case "Heal":
            case "Insp":
            case "Rally":
            case "Tenacity":
            case "Reap":
            case "Bulwark":
            case "Radiance":
            case "Holy":
            case "Mercy":
                return true;
            default:
                return false;
        }
    }
    private static void ClearLastRecoverEffectAndSfx(NCreature nCreature, string animName)
    {
        if (!GodotObject.IsInstanceValid(nCreature)) return;

        if (_lastEffects.TryGetValue(nCreature, out var lastEffect))
        {
            if (lastEffect != null && lastEffect.isRecoverEffect && GodotObject.IsInstanceValid(lastEffect.effectNode))
            {
                lastEffect.effectNode.Visible = false;
                lastEffect.effectNode.QueueFreeSafely();

                lastEffect.isRecoverEffect = false;
                lastEffect.effectNode = null;

                _lastEffects.Remove(nCreature);
            }
        }
        //AnimPatch中的顺序是先播放声音再播放特效，所以要排除Recover动画刚播放后就把自己声音清除的情况
        //潜在问题: xxx_Recover动画一般是我约定的动画最后一段的命名，所以如果xxx_Recover之后又有一段后续动画，会导致xxx_Recover的音效被自动清除
        if (!animName.Contains("_Recover") && CrusaderSettings.InterruptRecoverSfx)
        {
            CombatAudioManager.StopPlayingCombatSfx(2);
        }
    }
    private static void RegisterLastEffect(NCreature nCreature, bool isRecoverEffect, Node2D effectNode)
    {
        if (!GodotObject.IsInstanceValid(nCreature)) return;

        _lastEffects[nCreature] = new LastEffect(isRecoverEffect, effectNode);
    }
}
