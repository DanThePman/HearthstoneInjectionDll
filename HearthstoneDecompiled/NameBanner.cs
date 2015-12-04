using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NameBanner : MonoBehaviour
{
    public float FUDGE_FACTOR = 0.1915f;
    public GameObject m_alphaBanner;
    public GameObject m_alphaBannerBone;
    public GameObject m_alphaBannerLeft;
    public GameObject m_alphaBannerMiddle;
    public GameObject m_alphaBannerRight;
    public GameObject m_alphaBannerSkinned;
    public Transform m_classBone;
    private Transform m_classBoneToUse;
    public UberText m_className;
    public TournamentMedal m_medal;
    public GameObject m_medalAlphaBanner;
    public GameObject m_medalAlphaBannerLeft;
    public GameObject m_medalAlphaBannerMiddle;
    public GameObject m_medalAlphaBannerRight;
    public GameObject m_medalBannerBone;
    public GameObject m_medalBannerSkinned;
    public Transform m_medalClassBone;
    public Transform m_medalNameBone;
    public Transform m_nameBone;
    private Transform m_nameBoneToUse;
    public UberText m_playerName;
    private Player.Side m_playerSide;
    private const float SKINNED_BANNER_MIN_SIZE = 12f;
    private const float SKINNED_MEDAL_BANNER_MIN_SIZE = 17f;
    private const float UNKNOWN_NAME_WAIT = 5f;

    private void AdjustBanner()
    {
        Vector3 vector = TransformUtil.ComputeWorldScale(this.m_playerName.gameObject);
        float num = (this.FUDGE_FACTOR * vector.x) * this.m_playerName.GetTextWorldSpaceBounds().size.x;
        float num2 = (this.m_playerName.GetTextWorldSpaceBounds().size.x * vector.x) + num;
        float x = this.m_alphaBannerMiddle.GetComponent<Renderer>().bounds.size.x;
        float num4 = this.m_playerName.GetTextBounds().size.x;
        MeshRenderer renderer = this.m_medalAlphaBannerMiddle.GetComponentsInChildren<MeshRenderer>(true)[0];
        float num5 = renderer.bounds.size.x;
        if (num2 > x)
        {
            if (GameUtils.ShouldShowRankedMedals())
            {
                TransformUtil.SetLocalScaleX(this.m_medalAlphaBannerMiddle, num4 / num5);
                TransformUtil.SetPoint(this.m_medalAlphaBannerRight, Anchor.LEFT, renderer.gameObject, Anchor.RIGHT, new Vector3(0f, 0f, 0f));
            }
            else
            {
                TransformUtil.SetLocalScaleX(this.m_alphaBannerMiddle, num2 / x);
                TransformUtil.SetPoint(this.m_alphaBannerRight, Anchor.LEFT, this.m_alphaBannerMiddle, Anchor.RIGHT, new Vector3(-num, 0f, 0f));
            }
        }
    }

    private void AdjustSkinnedBanner()
    {
        float num;
        if (GameUtils.ShouldShowRankedMedals())
        {
            num = -this.m_playerName.GetTextBounds().size.x - 10f;
            if (num > -17f)
            {
                num = -17f;
            }
            Vector3 localPosition = this.m_medalBannerBone.transform.localPosition;
            this.m_medalBannerBone.transform.localPosition = new Vector3(num, localPosition.y, localPosition.z);
        }
        else
        {
            num = -this.m_playerName.GetTextBounds().size.x - 1f;
            if (num > -12f)
            {
                num = -12f;
            }
            Vector3 vector2 = this.m_alphaBannerBone.transform.localPosition;
            this.m_alphaBannerBone.transform.localPosition = new Vector3(num, vector2.y, vector2.z);
        }
    }

    private void Awake()
    {
        this.m_className.gameObject.SetActive(false);
        this.m_playerName.Text = string.Empty;
        this.m_nameBoneToUse = this.m_nameBone;
    }

    public void FadeClass()
    {
        if ((this.m_playerSide == Player.Side.FRIENDLY) && !string.IsNullOrEmpty(GameState.Get().GetGameEntity().GetAlternatePlayerName()))
        {
            iTween.FadeTo(base.gameObject, 0f, 1f);
        }
        else
        {
            iTween.FadeTo(this.m_className.gameObject, 0f, 1f);
            object[] args = new object[] { "position", this.m_nameBoneToUse.localPosition, "isLocal", true, "time", 1f };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(this.m_playerName.gameObject, hashtable);
        }
    }

    public void FadeIn()
    {
        if (this.m_alphaBannerSkinned != null)
        {
            iTween.FadeTo(this.m_alphaBannerSkinned.gameObject, 1f, 1f);
        }
        else
        {
            iTween.FadeTo(this.m_alphaBanner.gameObject, 1f, 1f);
        }
        iTween.FadeTo(this.m_playerName.gameObject, 1f, 1f);
    }

    public Player.Side GetPlayerSide()
    {
        return this.m_playerSide;
    }

    private bool IsReady(Player p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.GetName() == null)
        {
            return false;
        }
        if (p.GetRank() == null)
        {
            return false;
        }
        return true;
    }

    public void SetClass(string className, bool isAI)
    {
        this.m_className.gameObject.SetActive(true);
        this.m_className.Text = className;
        if (isAI)
        {
            this.m_playerName.transform.localPosition = this.m_classBoneToUse.localPosition;
        }
        else
        {
            this.m_playerName.transform.localPosition = this.m_classBoneToUse.localPosition;
        }
    }

    public void SetName(string name)
    {
        this.m_playerName.Text = name;
        if (this.m_alphaBannerSkinned != null)
        {
            this.AdjustSkinnedBanner();
        }
        else
        {
            this.AdjustBanner();
        }
    }

    public void SetPlayerSide(Player.Side side)
    {
        this.m_playerSide = side;
        this.UpdateAnchor();
        base.StartCoroutine(this.UpdateName());
        base.StartCoroutine(this.UpdateUnknownName());
    }

    public void Unload()
    {
        UnityEngine.Object.DestroyImmediate(base.gameObject);
    }

    private void Update()
    {
        this.UpdateAnchor();
    }

    private void UpdateAnchor()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            if (this.m_playerSide == Player.Side.FRIENDLY)
            {
                OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_RIGHT, false, CanvasScaleMode.HEIGHT);
            }
            else
            {
                OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_LEFT, false, CanvasScaleMode.HEIGHT);
            }
        }
        else
        {
            Vector3 vector;
            if (this.m_playerSide == Player.Side.FRIENDLY)
            {
                OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_LEFT, false, CanvasScaleMode.HEIGHT);
                vector = new Vector3(0f, 5f, 22f);
            }
            else
            {
                OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.TOP_LEFT, false, CanvasScaleMode.HEIGHT);
                vector = new Vector3(0f, 5f, -10f);
            }
            base.transform.localPosition = vector;
        }
    }

    public void UpdateMedalChange(MedalInfoTranslator medalInfo)
    {
        if ((medalInfo != null) && medalInfo.IsDisplayable())
        {
            this.m_medal.SetMedal(medalInfo, false);
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateName()
    {
        return new <UpdateName>c__IteratorC0 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UpdateUnknownName()
    {
        return new <UpdateUnknownName>c__IteratorC1 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateName>c__IteratorC0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Map<int, Player>.ValueCollection.Enumerator <$s_600>__2;
        internal NameBanner <>f__this;
        internal string <altName>__5;
        internal MedalInfoTranslator <medalInfoTranslator>__6;
        internal string <nameToDisplay>__4;
        internal Player <p>__0;
        internal Player <player>__3;
        internal Map<int, Player> <playerMap>__1;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                case 1:
                    if (GameState.Get().GetPlayerMap().Count == 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0497;
                    }
                    this.<p>__0 = null;
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_012D;

                case 4:
                    goto Label_02C7;

                case 5:
                    goto Label_0449;

                default:
                    goto Label_0495;
            }
            while (this.<p>__0 == null)
            {
                this.<playerMap>__1 = GameState.Get().GetPlayerMap();
                this.<$s_600>__2 = this.<playerMap>__1.Values.GetEnumerator();
                try
                {
                    while (this.<$s_600>__2.MoveNext())
                    {
                        this.<player>__3 = this.<$s_600>__2.Current;
                        if (this.<player>__3.GetSide() == this.<>f__this.m_playerSide)
                        {
                            this.<p>__0 = this.<player>__3;
                            goto Label_00F7;
                        }
                    }
                }
                finally
                {
                    this.<$s_600>__2.Dispose();
                }
            Label_00F7:
                this.$current = null;
                this.$PC = 2;
                goto Label_0497;
            }
        Label_012D:
            while (this.<p>__0.GetName() == null)
            {
                this.$current = null;
                this.$PC = 3;
                goto Label_0497;
            }
            this.<nameToDisplay>__4 = this.<p>__0.GetName();
            if (this.<p>__0.IsLocalUser())
            {
                this.<altName>__5 = GameState.Get().GetGameEntity().GetAlternatePlayerName();
                if (!string.IsNullOrEmpty(this.<altName>__5))
                {
                    this.<nameToDisplay>__4 = this.<altName>__5;
                }
            }
            this.<>f__this.SetName(this.<nameToDisplay>__4);
            this.<>f__this.m_nameBoneToUse = this.<>f__this.m_nameBone;
            this.<>f__this.m_classBoneToUse = this.<>f__this.m_classBone;
            if (this.<>f__this.m_alphaBannerSkinned == null)
            {
                this.<>f__this.m_alphaBanner.SetActive(true);
            }
            else
            {
                this.<>f__this.m_alphaBannerSkinned.SetActive(true);
                if (this.<>f__this.m_alphaBanner != null)
                {
                    this.<>f__this.m_alphaBanner.SetActive(false);
                }
                this.<>f__this.m_medalBannerSkinned.SetActive(false);
            }
            if (this.<>f__this.m_medalAlphaBanner != null)
            {
                this.<>f__this.m_medalAlphaBanner.SetActive(false);
            }
            if (this.<>f__this.m_medalBannerSkinned != null)
            {
                this.<>f__this.m_medalBannerSkinned.SetActive(false);
            }
            this.<>f__this.m_medal.gameObject.SetActive(false);
            if (!GameUtils.ShouldShowRankedMedals())
            {
                goto Label_03F8;
            }
        Label_02C7:
            while (this.<p>__0.GetRank() == null)
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_0497;
            }
            this.<medalInfoTranslator>__6 = this.<p>__0.GetRank();
            if ((this.<medalInfoTranslator>__6 != null) && this.<medalInfoTranslator>__6.IsDisplayable())
            {
                this.<>f__this.m_nameBoneToUse = this.<>f__this.m_medalNameBone;
                this.<>f__this.m_classBoneToUse = this.<>f__this.m_medalClassBone;
                if (this.<>f__this.m_medalBannerSkinned == null)
                {
                    this.<>f__this.m_medalAlphaBanner.SetActive(true);
                }
                else
                {
                    this.<>f__this.m_medalBannerSkinned.SetActive(true);
                    this.<>f__this.m_alphaBannerSkinned.SetActive(false);
                    if (this.<>f__this.m_medalAlphaBanner != null)
                    {
                        this.<>f__this.m_medalAlphaBanner.SetActive(false);
                    }
                }
                if (this.<>f__this.m_alphaBanner != null)
                {
                    this.<>f__this.m_alphaBanner.SetActive(false);
                }
                this.<>f__this.m_medal.gameObject.SetActive(true);
                this.<>f__this.m_medal.SetMedal(this.<medalInfoTranslator>__6, false);
            }
        Label_03F8:
            this.<>f__this.m_playerName.transform.localPosition = this.<>f__this.m_nameBoneToUse.localPosition;
            if (GameMgr.Get().IsTutorial())
            {
                goto Label_0495;
            }
        Label_0449:
            while (this.<p>__0.GetHero().GetClass() == TAG_CLASS.INVALID)
            {
                this.$current = null;
                this.$PC = 5;
                goto Label_0497;
            }
            this.<>f__this.SetClass(GameStrings.GetClassName(this.<p>__0.GetHero().GetClass()).ToUpper(), this.<p>__0.IsAI());
            this.$PC = -1;
        Label_0495:
            return false;
        Label_0497:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateUnknownName>c__IteratorC1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal NameBanner <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!(this.<>f__this.m_playerName.Text != string.Empty))
                    {
                        this.<>f__this.SetName(GameStrings.Get("GAMEPLAY_UNKNOWN_OPPONENT_NAME"));
                        this.$PC = -1;
                        break;
                    }
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

