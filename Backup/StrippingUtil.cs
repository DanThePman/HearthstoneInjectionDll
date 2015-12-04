using System;
using UnityEngine;

public class StrippingUtil : MonoBehaviour
{
    private Behaviour dummy_11;
    private ParticleAnimator dummy_14;
    private Camera dummy_19;
    private MeshRenderer dummy_2;
    private Material dummy_20;
    private MeshRenderer dummy_21;
    private Renderer dummy_22;
    private ParticleRenderer dummy_23;
    private Texture dummy_24;
    private Texture2D dummy_25;
    private MeshFilter dummy_28;
    private OcclusionPortal dummy_29;
    private Avatar dummy_3;
    private Mesh dummy_30;
    private Skybox dummy_31;
    private QualitySettings dummy_32;
    private Shader dummy_33;
    private TextAsset dummy_34;
    private Rigidbody dummy_36;
    private Collider dummy_38;
    private Joint dummy_39;
    private AudioReverbZone dummy_4;
    private HingeJoint dummy_40;
    private MeshCollider dummy_41;
    private BoxCollider dummy_42;
    private AnimationClip dummy_44;
    private ConstantForce dummy_45;
    private AudioListener dummy_48;
    private AudioSource dummy_49;
    private GameObject dummy_5;
    private AudioClip dummy_50;
    private RenderTexture dummy_51;
    private ParticleEmitter dummy_53;
    private Animator dummy_54;
    private TrailRenderer dummy_55;
    private ParticleSystem dummy_56;
    private ConfigurableJoint dummy_57;
    private FixedJoint dummy_58;
    private WindZone dummy_59;
    private Component dummy_6;
    private Transform dummy_8;

    public void SeedMonotouchAheadOfTimeCompile()
    {
        ParticleSystem system = null;
        system.playbackSpeed++;
        system.startLifetime++;
        system.startSpeed++;
        system.startDelay++;
        system.startRotation++;
        system.emissionRate++;
        RenderToTexture texture = null;
        texture.m_ObjectToRender = null;
        texture.m_RealtimeRender &= true;
        texture.enabled &= true;
        FullScreenEffects effects = null;
        effects.BlendToColorAmount++;
        effects.VignettingEnable &= true;
        effects.VignettingIntensity++;
        MeshFilter filter = null;
        filter.mesh = null;
        RotateOverTime time = null;
        time.RotateSpeedX++;
        time.RotateSpeedY++;
        time.RotateSpeedZ++;
    }
}

