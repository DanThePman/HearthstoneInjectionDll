using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class TGTArcheryTarget : MonoBehaviour
{
    public GameObject m_Arrow;
    public GameObject m_ArrowBone01;
    public GameObject m_ArrowBone02;
    private int m_ArrowCount;
    private GameObject[] m_arrows;
    private List<int> m_AvailableTargetDummyArrows;
    public BoxCollider m_BoxCollider01;
    public BoxCollider m_BoxCollider02;
    public BoxCollider m_BoxColliderBullseye;
    public Transform m_BullseyeCenterBone;
    public int m_BullseyePercent = 5;
    private float m_bullseyeRadius;
    public Transform m_BullseyeRadiusBone;
    public Transform m_CenterBone;
    private bool m_clearingArrows;
    public GameObject m_Collider01;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitBullseyeSoundPrefab;
    public float m_HitIntensity;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitTargetDummySoundPrefab;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitTargetSoundPrefab;
    private int m_lastArrow = 1;
    private bool m_lastArrowWasBullseye;
    private GameObject m_lastBullseyeArrow;
    private float m_lastClickTime;
    public int m_Levelup = 50;
    public int m_MaxArrows;
    public float m_MaxRandomOffset = 0.3f;
    public Transform m_OuterRadiusBone;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_RemoveArrowSoundPrefab;
    public GameObject m_SplitArrow;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_SplitArrowSoundPrefab;
    public List<TGTArrow> m_TargetDummyArrows;
    public int m_TargetDummyPercent = 1;
    public GameObject m_TargetPhysics;
    private float m_targetRadius;
    public GameObject m_TargetRoot;

    [DebuggerHidden]
    private IEnumerator ClearArrows()
    {
        return new <ClearArrows>c__Iterator11 { <>f__this = this };
    }

    private void FireArrow(TGTArrow arrow, Vector3 hitPosition, bool bullseye)
    {
        arrow.transform.position = hitPosition;
        bool flag = false;
        if (UnityEngine.Time.timeSinceLevelLoad > (this.m_lastClickTime + 0.8f))
        {
            flag = true;
        }
        this.m_lastClickTime = UnityEngine.Time.timeSinceLevelLoad;
        int bullseyePercent = this.m_BullseyePercent;
        if (flag)
        {
            bullseyePercent *= 2;
        }
        if (bullseyePercent > 80)
        {
            bullseyePercent = 80;
        }
        if (bullseye && (UnityEngine.Random.Range(0, 100) < bullseyePercent))
        {
            int num2 = 2;
            if (flag)
            {
                num2 = 8;
            }
            if ((this.m_lastArrowWasBullseye && !this.m_SplitArrow.activeSelf) && (bullseye && (UnityEngine.Random.Range(0, 100) < num2)))
            {
                this.m_SplitArrow.transform.position = this.m_lastBullseyeArrow.transform.position;
                this.m_SplitArrow.transform.rotation = this.m_lastBullseyeArrow.transform.rotation;
                TGTArrow component = this.m_SplitArrow.GetComponent<TGTArrow>();
                TGTArrow arrow3 = this.m_lastBullseyeArrow.GetComponent<TGTArrow>();
                this.m_SplitArrow.SetActive(true);
                component.FireArrow(false);
                component.Bullseye();
                this.PlaySound(this.m_SplitArrowSoundPrefab);
                component.m_ArrowRoot.transform.position = arrow3.m_ArrowRoot.transform.position;
                component.m_ArrowRoot.transform.rotation = arrow3.m_ArrowRoot.transform.rotation;
                this.m_lastBullseyeArrow.SetActive(false);
                this.m_lastArrowWasBullseye = false;
                this.m_lastBullseyeArrow = null;
            }
            else
            {
                arrow.gameObject.SetActive(true);
                arrow.Bullseye();
                this.PlaySound(this.m_HitBullseyeSoundPrefab);
                arrow.m_ArrowRoot.transform.localPosition = Vector3.zero;
                this.m_lastBullseyeArrow = arrow.gameObject;
                this.m_lastArrowWasBullseye = true;
            }
        }
        else
        {
            this.m_lastArrowWasBullseye = false;
            this.m_lastBullseyeArrow = null;
            arrow.gameObject.SetActive(true);
            if (bullseye)
            {
                Vector2 vector = (Vector2) ((UnityEngine.Random.insideUnitCircle.normalized * this.m_bullseyeRadius) * 2f);
                arrow.m_ArrowRoot.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
                arrow.FireArrow(true);
                this.PlaySound(this.m_HitTargetSoundPrefab);
            }
            else
            {
                Vector2 vector2 = (Vector2) (UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0f, this.m_MaxRandomOffset));
                arrow.m_ArrowRoot.transform.localPosition = new Vector3(vector2.x, vector2.y, 0f);
                if (Vector3.Distance(arrow.m_ArrowRoot.transform.position, this.m_CenterBone.position) > this.m_targetRadius)
                {
                    arrow.m_ArrowRoot.transform.localPosition = Vector3.zero;
                }
                if (Vector3.Distance(arrow.m_ArrowRoot.transform.position, this.m_BullseyeCenterBone.position) < this.m_bullseyeRadius)
                {
                    arrow.m_ArrowRoot.transform.localPosition = Vector3.zero;
                }
                arrow.FireArrow(true);
                this.PlaySound(this.m_HitTargetSoundPrefab);
            }
        }
    }

    private void HandleHits()
    {
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_Collider01))
        {
            this.HnadleFireArrow();
        }
    }

    private void HitTargetDummy()
    {
        int index = 0;
        if (this.m_AvailableTargetDummyArrows.Count > 1)
        {
            index = UnityEngine.Random.Range(0, this.m_AvailableTargetDummyArrows.Count);
        }
        TGTArrow arrow = this.m_TargetDummyArrows[this.m_AvailableTargetDummyArrows[index]];
        arrow.gameObject.SetActive(true);
        arrow.FireArrow(false);
        TGTTargetDummy.Get().ArrowHit();
        this.PlaySound(this.m_HitTargetDummySoundPrefab);
        if (this.m_AvailableTargetDummyArrows.Count > 1)
        {
            this.m_AvailableTargetDummyArrows.RemoveAt(index);
        }
        else
        {
            this.m_AvailableTargetDummyArrows.Clear();
        }
    }

    private void HnadleFireArrow()
    {
        if (!this.m_clearingArrows)
        {
            this.m_ArrowCount++;
            if (this.m_ArrowCount > this.m_Levelup)
            {
                this.m_ArrowCount = 0;
                this.m_MaxRandomOffset *= 0.95f;
                this.m_BullseyePercent += 4;
            }
            if ((UnityEngine.Random.Range(0, 100) < this.m_TargetDummyPercent) && (this.m_AvailableTargetDummyArrows.Count > 0))
            {
                this.HitTargetDummy();
            }
            else
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool bullseye = false;
                if (this.m_BoxColliderBullseye.Raycast(ray, out hit, 100f))
                {
                    bullseye = true;
                }
                if (this.m_BoxCollider02.Raycast(ray, out hit, 100f))
                {
                    this.m_lastArrow++;
                    if (this.m_lastArrow >= this.m_MaxArrows)
                    {
                        this.m_lastArrow = 0;
                        base.StartCoroutine(this.ClearArrows());
                    }
                    else
                    {
                        GameObject obj2 = this.m_arrows[this.m_lastArrow];
                        TGTArrow component = obj2.GetComponent<TGTArrow>();
                        this.FireArrow(component, hit.point, bullseye);
                        obj2.transform.eulerAngles = hit.normal;
                        this.ImpactTarget();
                    }
                }
            }
        }
    }

    private void ImpactTarget()
    {
        this.m_TargetPhysics.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range(this.m_HitIntensity * 0.25f, this.m_HitIntensity), 0f, 0f);
    }

    private bool IsOver(GameObject go)
    {
        if (go == null)
        {
            return false;
        }
        if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
        {
            return false;
        }
        if (!UniversalInputManager.Get().InputIsOver(go))
        {
            return false;
        }
        return true;
    }

    private void PlaySound(string soundPrefab)
    {
        if (!string.IsNullOrEmpty(soundPrefab))
        {
            string str = FileUtils.GameAssetPathToName(soundPrefab);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, base.gameObject);
            }
        }
    }

    private void Start()
    {
        this.m_arrows = new GameObject[this.m_MaxArrows];
        for (int i = 0; i < this.m_MaxArrows; i++)
        {
            this.m_arrows[i] = UnityEngine.Object.Instantiate<GameObject>(this.m_Arrow);
            this.m_arrows[i].transform.position = new Vector3(-15f, -15f, -15f);
            this.m_arrows[i].transform.parent = this.m_TargetRoot.transform;
            this.m_arrows[i].SetActive(false);
        }
        this.m_arrows[0].SetActive(true);
        this.m_arrows[0].transform.position = this.m_ArrowBone01.transform.position;
        this.m_arrows[0].transform.rotation = this.m_ArrowBone01.transform.rotation;
        this.m_arrows[1].SetActive(true);
        this.m_arrows[1].transform.position = this.m_ArrowBone02.transform.position;
        this.m_arrows[1].transform.rotation = this.m_ArrowBone02.transform.rotation;
        this.m_lastArrow = 2;
        this.m_targetRadius = Vector3.Distance(this.m_CenterBone.position, this.m_OuterRadiusBone.position);
        this.m_bullseyeRadius = Vector3.Distance(this.m_BullseyeCenterBone.position, this.m_BullseyeRadiusBone.position);
        this.m_AvailableTargetDummyArrows = new List<int>();
        for (int j = 0; j < this.m_TargetDummyArrows.Count; j++)
        {
            this.m_AvailableTargetDummyArrows.Add(j);
        }
        this.m_SplitArrow.SetActive(false);
    }

    private void Update()
    {
        this.HandleHits();
    }

    [CompilerGenerated]
    private sealed class <ClearArrows>c__Iterator11 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject[] <$s_164>__0;
        internal int <$s_165>__1;
        internal TGTArcheryTarget <>f__this;
        internal GameObject <arrowGO>__2;

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
                    this.<>f__this.m_clearingArrows = true;
                    this.<$s_164>__0 = this.<>f__this.m_arrows;
                    this.<$s_165>__1 = 0;
                    goto Label_010D;

                case 1:
                    break;

                case 2:
                    if (this.<>f__this.m_SplitArrow.activeSelf)
                    {
                        this.<>f__this.m_SplitArrow.SetActive(false);
                        this.<>f__this.m_TargetPhysics.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range((float) (this.<>f__this.m_HitIntensity * -0.25f), (float) (this.<>f__this.m_HitIntensity * -0.5f)), 0f, 0f);
                        this.<>f__this.PlaySound(this.<>f__this.m_RemoveArrowSoundPrefab);
                    }
                    this.<>f__this.m_lastArrowWasBullseye = false;
                    this.<>f__this.m_lastBullseyeArrow = null;
                    this.<>f__this.m_clearingArrows = false;
                    this.$PC = -1;
                    goto Label_01EE;

                default:
                    goto Label_01EE;
            }
        Label_00FF:
            this.<$s_165>__1++;
        Label_010D:
            if (this.<$s_165>__1 < this.<$s_164>__0.Length)
            {
                this.<arrowGO>__2 = this.<$s_164>__0[this.<$s_165>__1];
                if (this.<arrowGO>__2.activeSelf)
                {
                    this.<arrowGO>__2.SetActive(false);
                    this.<>f__this.m_TargetPhysics.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range((float) (this.<>f__this.m_HitIntensity * -0.25f), (float) (this.<>f__this.m_HitIntensity * -0.5f)), 0f, 0f);
                    this.<>f__this.PlaySound(this.<>f__this.m_RemoveArrowSoundPrefab);
                    this.$current = new WaitForSeconds(0.2f);
                    this.$PC = 1;
                    goto Label_01F0;
                }
                goto Label_00FF;
            }
            this.$current = new WaitForSeconds(0.2f);
            this.$PC = 2;
            goto Label_01F0;
        Label_01EE:
            return false;
        Label_01F0:
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
}

