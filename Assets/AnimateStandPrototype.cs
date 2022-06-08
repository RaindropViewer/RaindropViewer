using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Assets;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Services.Bootstrap;
using UnityEngine;
using Animation = UnityEngine.Animation;
using Logger = OpenMetaverse.Logger;

//make the avatar play the standing animation.
public class AnimateStandPrototype : MonoBehaviour
{
    private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();

    public UUID standAnim => Animations.STAND;
    public UUID anim => standAnim;

    //actual animation data from secondlife servers.
    private byte[] animData;

    
    void OnEnable()
    {
        Animation animation = this.GetComponent<Animation>();

        var animationClip = createLegacyAnimationClip(standAnim);
        animation.AddClip(animationClip, "ScriptedAnimationClip");
        animation.Play("ScriptedAnimationClip");

    }

    private AnimationClip createLegacyAnimationClip(UUID uuid)
    {
        //1. download and get a reference to this animation.
        instance.Client.Assets.RequestAsset(uuid, AssetType.Animation, true, OnAnimReceived);
        
        
        throw new System.NotImplementedException();
    }
    
    // once the asset has arrived, we want to play it (locally).
    void OnAnimReceived(AssetDownload transfer, Asset asset)
    {
        if (!UnityMainThreadDispatcher.isOnMainThread())
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                OnAnimReceived(transfer, asset);
            });
        }
        
        if (transfer.Success)
        {
            Logger.Log("Animation " + anim + " download success " + asset.AssetData.Length + " bytes.", Helpers.LogLevel.Debug);
            animData = asset.AssetData;
        }
        else
        {
            Logger.Log("Animation " + anim + " download failed.", Helpers.LogLevel.Debug);
        }
    }

    // Add animations to the global decoded list
        // TODO garbage collect unused animations somehow
        public static void addanimation(OpenMetaverse.Assets.Asset asset, UUID tid, BinBVHAnimationReader b, UUID animKey)
        {
            // RenderAvatar av;
            // mAnimationTransactions.TryGetValue(tid, out av);
            // if (av == null)
            //     return;
            //
            // mAnimationTransactions.Remove(tid);
            //
            // if (asset != null)
            // {
            //     b = new BinBVHAnimationReader(asset.AssetData);
            //     mAnimationCache[asset.AssetID] = b;
            //     Logger.Log("Adding new decoded animaton known animations " + asset.AssetID, Helpers.LogLevel.Info);
            // }
            //
            // if (!av.glavatar.skel.mAnimationsWrapper.ContainsKey(animKey))
            // {
            //     Logger.Log($"Animation {animKey} is not in mAnimationsWrapper! ", Helpers.LogLevel.Warning);
            //     return;
            // }
            //
            // // This sets the anim in the wrapper class;
            // av.glavatar.skel.mAnimationsWrapper[animKey].anim = b;
            //
            // int pos = 0;
            // foreach (binBVHJoint joint in b.joints)
            // {
            //     binBVHJointState state;
            //
            //     state.lastkeyframe_rot = 0;
            //     state.nextkeyframe_rot = 1;
            //
            //     state.lastkeyframe_pos = 0;
            //     state.nextkeyframe_pos = 1;
            //
            //     state.currenttime_rot = 0;
            //     state.currenttime_pos = 0;
            //
            //     state.pos_loopinframe = 0;
            //     state.pos_loopoutframe = joint.positionkeys.Length - 1;
            //
            //     state.rot_loopinframe = 0;
            //     state.rot_loopoutframe = joint.rotationkeys.Length - 1;
            //
            //     state.easeoutfactor = 1.0f;
            //     state.easeoutrot = Quaternion.Identity;
            //
            //     if (b.Loop)
            //     {
            //         int frame = 0;
            //         foreach (binBVHJointKey key in joint.rotationkeys)
            //         {
            //             if (key.time == b.InPoint)
            //             {
            //                 state.rot_loopinframe = frame;
            //             }
            //
            //             if (key.time == b.OutPoint)
            //             {
            //                 state.rot_loopoutframe = frame;
            //             }
            //
            //             frame++;
            //         }
            //
            //         frame = 0;
            //         foreach (binBVHJointKey key in joint.positionkeys)
            //         {
            //             if (key.time == b.InPoint)
            //             {
            //                 state.pos_loopinframe = frame;
            //             }
            //
            //             if (key.time == b.OutPoint)
            //             {
            //                 state.pos_loopoutframe = frame;
            //             }
            //
            //             frame++;
            //         }
            //
            //     }
            //
            //     b.joints[pos].Tag = state;
            //     pos++;
            // }
            //
            // av.glavatar.skel.mAnimationsWrapper[animKey].playstate = animationwrapper.animstate.STATE_EASEIN;
            // recalcpriorities(av);

        }

    
    

    //creates a animation clip that move the gameobject
    private AnimationClip createLegacyAnimationClip()
    {
        // create animclip
        var clip = new AnimationClip();
        clip.legacy = true;
        clip.wrapMode = WrapMode.Loop;
        
        // add movement-curve to animclip
        var curve = createAnimationCurve();
        clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

        // add color-curve to animclip
        var colorCurve = createAnimationCurve_Red();
        clip.SetCurve("", typeof(Renderer), "material._BaseColor.r", colorCurve);

        return clip;
    }

    private AnimationCurve createAnimationCurve_Red()
    {
        var curve = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 0.0f);
        return curve;
    }

    private AnimationCurve createAnimationCurve()
    {
        Keyframe[] keys;
        keys = new Keyframe[3]; // 3 keyframes.
        keys[0] = new Keyframe(0.0f, 0.0f);
        keys[1] = new Keyframe(1.0f, 1.5f);
        keys[2] = new Keyframe(2.0f, 0.0f);
        var curve = new AnimationCurve(keys);
        return curve;
    }
}
