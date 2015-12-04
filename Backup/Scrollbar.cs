using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scrollbar : MonoBehaviour
{
    protected Bounds m_childrenBounds;
    protected bool m_isActive = true;
    protected bool m_isDragging;
    public GameObject m_scrollArea;
    protected Vector3 m_scrollAreaEndPos;
    protected Vector3 m_scrollAreaStartPos;
    protected float m_scrollValue;
    public BoxCollider m_scrollWindow;
    protected float m_scrollWindowHeight;
    public Vector3 m_sliderEndLocalPos;
    public Vector3 m_sliderStartLocalPos;
    protected float m_stepSize;
    public ScrollBarThumb m_thumb;
    protected Vector3 m_thumbPosition;
    public GameObject m_track;

    protected virtual void Awake()
    {
        this.m_scrollWindowHeight = this.m_scrollWindow.size.z;
        this.m_scrollWindow.enabled = false;
    }

    public void Drag()
    {
        float num;
        Vector3 min = this.m_track.GetComponent<BoxCollider>().bounds.min;
        Camera camera = CameraUtils.FindFirstByLayer(this.m_track.layer);
        Plane plane = new Plane(-camera.transform.forward, min);
        Ray ray = camera.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
        if (plane.Raycast(ray, out num))
        {
            Vector3 vector2 = base.transform.InverseTransformPoint(ray.GetPoint(num));
            TransformUtil.SetLocalPosZ(this.m_thumb.gameObject, Mathf.Clamp(vector2.z, this.m_sliderStartLocalPos.z, this.m_sliderEndLocalPos.z));
            this.m_scrollValue = Mathf.Clamp01((vector2.z - this.m_sliderStartLocalPos.z) / (this.m_sliderEndLocalPos.z - this.m_sliderStartLocalPos.z));
            this.UpdateScrollAreaPosition(false);
        }
    }

    protected virtual void GetBoundsOfChildren(GameObject go)
    {
        this.m_childrenBounds = TransformUtil.GetBoundsOfChildren(go);
    }

    public virtual void Hide()
    {
        this.m_isActive = false;
        this.ShowThumb(false);
        base.gameObject.SetActive(false);
    }

    public void Init()
    {
        this.m_scrollValue = 0f;
        this.m_stepSize = 1f;
        this.m_thumb.transform.localPosition = this.m_sliderStartLocalPos;
        this.m_scrollAreaStartPos = this.m_scrollArea.transform.position;
        this.UpdateScrollAreaBounds();
    }

    public virtual bool InputIsOver()
    {
        return UniversalInputManager.Get().InputIsOver(base.gameObject);
    }

    public bool IsActive()
    {
        return this.m_isActive;
    }

    public void OverrideScrollWindowHeight(float scrollWindowHeight)
    {
        this.m_scrollWindowHeight = scrollWindowHeight;
    }

    public void Scroll(float amount, bool lerp = true)
    {
        this.m_scrollValue = Mathf.Clamp01(this.m_scrollValue + amount);
        this.UpdateThumbPosition();
        this.UpdateScrollAreaPosition(lerp);
    }

    private void ScrollDown()
    {
        this.Scroll(this.m_stepSize, true);
    }

    public void ScrollTo(float value, bool clamp = true, bool lerp = true)
    {
        this.m_scrollValue = !clamp ? value : Mathf.Clamp01(value);
        this.UpdateThumbPosition();
        this.UpdateScrollAreaPosition(lerp);
    }

    private void ScrollUp()
    {
        this.Scroll(-this.m_stepSize, true);
    }

    public virtual void Show()
    {
        this.m_isActive = true;
        this.ShowThumb(true);
        base.gameObject.SetActive(true);
    }

    protected void ShowThumb(bool show)
    {
        if (this.m_thumb != null)
        {
            this.m_thumb.gameObject.SetActive(show);
        }
    }

    private void Update()
    {
        if (this.m_isActive)
        {
            if (this.InputIsOver())
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    this.ScrollDown();
                }
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    this.ScrollUp();
                }
            }
            if (this.m_thumb.IsDragging())
            {
                this.Drag();
            }
        }
    }

    public void UpdateScrollAreaBounds()
    {
        this.GetBoundsOfChildren(this.m_scrollArea);
        float num2 = this.m_childrenBounds.size.z - this.m_scrollWindowHeight;
        this.m_scrollAreaEndPos = this.m_scrollAreaStartPos;
        if (num2 <= 0f)
        {
            this.m_scrollValue = 0f;
            this.Hide();
        }
        else
        {
            int num3 = (int) Mathf.Ceil(num2 / 5f);
            this.m_stepSize = 1f / ((float) num3);
            this.m_scrollAreaEndPos.z += num2;
            this.Show();
        }
        this.UpdateThumbPosition();
        this.UpdateScrollAreaPosition(false);
    }

    private void UpdateScrollAreaPosition(bool tween)
    {
        if (this.m_scrollArea != null)
        {
            Vector3 vector = this.m_scrollAreaStartPos + ((Vector3) (this.m_scrollValue * (this.m_scrollAreaEndPos - this.m_scrollAreaStartPos)));
            if (tween)
            {
                object[] args = new object[] { "position", vector, "time", 0.2f, "isLocal", false };
                iTween.MoveTo(this.m_scrollArea, iTween.Hash(args));
            }
            else
            {
                this.m_scrollArea.transform.position = vector;
            }
        }
    }

    private void UpdateThumbPosition()
    {
        this.m_thumbPosition = Vector3.Lerp(this.m_sliderStartLocalPos, this.m_sliderEndLocalPos, Mathf.Clamp01(this.m_scrollValue));
        this.m_thumb.transform.localPosition = this.m_thumbPosition;
        this.m_thumb.transform.localScale = Vector3.one;
        if ((this.m_scrollValue < 0f) || (this.m_scrollValue > 1f))
        {
            float z = 1f / ((this.m_scrollValue >= 0f) ? this.m_scrollValue : (-this.m_scrollValue + 1f));
            float num2 = this.m_thumb.transform.parent.InverseTransformPoint((this.m_scrollValue >= 0f) ? this.m_thumb.GetComponent<Renderer>().bounds.min : this.m_thumb.GetComponent<Renderer>().bounds.max).z;
            float num3 = (this.m_thumbPosition.z - num2) * (z - 1f);
            TransformUtil.SetLocalPosZ(this.m_thumb, this.m_thumbPosition.z + num3);
            TransformUtil.SetLocalScaleZ(this.m_thumb, z);
        }
    }

    public float ScrollValue
    {
        get
        {
            return this.m_scrollValue;
        }
    }
}

