using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class Carousel : MonoBehaviour
{
    private const float DRAG = 0.015f;
    public GameObject[] m_bones;
    private CarouselSettled m_carouselSettledListener;
    private CarouselStartedScrolling m_carouselStartedScrollingListener;
    public Collider m_collider;
    private bool m_doFlyIn;
    private float m_flyInState;
    private float m_hitCarouselPosition;
    private int m_hitIndex;
    private CarouselItem m_hitItem;
    private Vector3 m_hitWorldPosition;
    private int m_intPosition;
    private ItemClicked m_itemClickedListener;
    private ItemPulled m_itemPulledListener;
    private ItemReleased m_itemReleasedListener;
    private CarouselItem[] m_items;
    public int m_maxPosition = 4;
    private int m_momentumCounter;
    private float[] m_momentumHistory = new float[5];
    private int m_momentumTotal;
    private float m_moveAdjustment;
    public bool m_noMouseMovement;
    private int m_numItems;
    private float m_position;
    private int m_radius;
    private bool m_scrolling;
    private bool m_settleCallbackCalled;
    private float m_startX;
    private float m_targetPosition;
    private float m_totalMove;
    private bool m_touchActive;
    private Vector2 m_touchStart;
    private float m_touchX;
    public bool m_trackItemHit;
    public bool m_useFlyIn;
    private float m_velocity;
    private const float MIN_VELOCITY = 0.03f;
    private const float SCROLL_THRESHOLD = 10f;

    public bool AreVisibleItemsLoaded()
    {
        for (int i = 0; i < this.m_numItems; i++)
        {
            int index = (this.m_intPosition + i) - this.m_radius;
            if ((index >= 0) && !this.m_items[index].IsLoaded())
            {
                Debug.Log("Not loaded " + index);
                return false;
            }
        }
        return true;
    }

    private void Awake()
    {
        this.m_numItems = this.m_bones.Length;
        this.m_radius = this.m_numItems / 2;
    }

    private float CalculateVelocity()
    {
        float num = 0f;
        for (int i = 0; i < this.m_momentumTotal; i++)
        {
            num += this.m_momentumHistory[i];
        }
        return ((-0.9f * num) / ((float) this.m_momentumTotal));
    }

    public void ClearItems()
    {
        if (this.m_items != null)
        {
            foreach (CarouselItem item in this.m_items)
            {
                item.Clear();
            }
        }
    }

    private float DistanceFromSettle()
    {
        float num = this.m_position - ((int) this.m_position);
        if (num > 0.5f)
        {
            return (1f - num);
        }
        return num;
    }

    private void DoFlyIn()
    {
        if (this.m_useFlyIn)
        {
            this.m_doFlyIn = true;
            this.m_flyInState = 0f;
        }
    }

    private float GetCarouselPosition(float x)
    {
        if (x >= this.m_bones[0].transform.position.x)
        {
            if (x > this.m_bones[this.m_bones.Length - 1].transform.position.x)
            {
                return (this.m_bones.Length - 1f);
            }
            float num = this.m_bones[0].transform.position.x;
            for (int i = 1; i < this.m_bones.Length; i++)
            {
                float num3 = this.m_bones[i].transform.position.x;
                if ((x >= num) && (x <= num3))
                {
                    return (i + ((x - num) / (num3 - num)));
                }
                num = num3;
            }
        }
        return 0f;
    }

    public int GetCurrentIndex()
    {
        return this.m_intPosition;
    }

    public CarouselItem GetCurrentItem()
    {
        if (this.m_items == null)
        {
            return null;
        }
        return this.m_items[this.m_intPosition];
    }

    public int GetTargetPosition()
    {
        return (int) this.m_targetPosition;
    }

    public void Initialize(CarouselItem[] items, int position = 0)
    {
        if (this.m_items != null)
        {
            this.ClearItems();
        }
        this.m_items = items;
        this.m_position = this.m_targetPosition = position;
        this.m_intPosition = position;
        this.DoFlyIn();
    }

    private void InitVelocity()
    {
        for (int i = 0; i < this.m_momentumHistory.Length; i++)
        {
            this.m_momentumHistory[i] = 0f;
        }
        this.m_momentumCounter = 0;
        this.m_momentumTotal = 0;
    }

    public bool IsScrolling()
    {
        return this.m_scrolling;
    }

    private int MouseHit(out CarouselItem itemHit)
    {
        itemHit = null;
        if ((this.m_items == null) || (this.m_items.Length <= 0))
        {
            return -1;
        }
        for (int i = 0; i < this.m_items.Length; i++)
        {
            RaycastHit hit;
            CarouselItem item = this.m_items[i];
            if ((item.GetGameObject() != null) && UniversalInputManager.Get().InputIsOver(item.GetGameObject(), out hit))
            {
                itemHit = item;
                return i;
            }
        }
        return -1;
    }

    public bool MouseOver()
    {
        CarouselItem item;
        return (this.MouseHit(out item) >= 0);
    }

    private void OnDestroy()
    {
        this.m_itemPulledListener = null;
        this.m_itemClickedListener = null;
        this.m_itemReleasedListener = null;
        this.m_carouselSettledListener = null;
        this.m_carouselStartedScrollingListener = null;
    }

    public void SetListeners(ItemPulled pulled, ItemClicked clicked, ItemReleased released, CarouselSettled settled = null, CarouselStartedScrolling scrolling = null)
    {
        this.m_itemPulledListener = pulled;
        this.m_itemClickedListener = clicked;
        this.m_itemReleasedListener = released;
        this.m_carouselSettledListener = settled;
        this.m_carouselStartedScrollingListener = scrolling;
    }

    public void SetPosition(int n, bool animate = false)
    {
        this.m_targetPosition = n;
        if (!animate)
        {
            this.m_position = this.m_targetPosition;
        }
        else
        {
            this.StartScrolling();
            this.m_settleCallbackCalled = false;
        }
        this.DoFlyIn();
    }

    private void SettlePosition(float bias = 0f)
    {
        float num;
        if (bias > 0.001f)
        {
            num = Mathf.Round(this.m_position + 0.5f);
        }
        else if (bias < -0.001f)
        {
            num = Mathf.Round(this.m_position - 0.5f);
        }
        else
        {
            num = Mathf.Round(this.m_position);
        }
        num = Math.Max(0f, num);
        num = Math.Min((float) this.m_maxPosition, num);
        this.m_targetPosition = num;
    }

    private void Start()
    {
        this.m_trackItemHit = true;
    }

    private void StartScrolling()
    {
        this.m_scrolling = true;
        if (this.m_carouselStartedScrollingListener != null)
        {
            this.m_carouselStartedScrollingListener();
        }
    }

    public void UpdateUI(bool mouseDown)
    {
        if (this.m_items != null)
        {
            CarouselItem item;
            bool flag = (this.m_position < 0f) || (this.m_position > this.m_maxPosition);
            if (this.m_touchActive)
            {
                float x = Input.mousePosition.x;
                float num3 = x - this.m_touchX;
                if (!this.m_scrolling && (Math.Abs((float) (this.m_touchStart.x - x)) >= 10f))
                {
                    this.StartScrolling();
                }
                float num4 = (num3 * 4.5f) / ((float) Screen.width);
                if (this.m_position < 0f)
                {
                    num4 /= 1f + (Math.Abs(this.m_position) * 5f);
                }
                if (!this.m_noMouseMovement)
                {
                    if (this.m_trackItemHit)
                    {
                        Vector3 vector;
                        UniversalInputManager.Get().GetInputPointOnPlane(this.m_hitWorldPosition, out vector);
                        float carouselPosition = this.GetCarouselPosition(vector.x);
                        this.m_position = (this.m_startX + this.m_hitCarouselPosition) - carouselPosition;
                    }
                    else
                    {
                        this.m_position -= num4;
                    }
                }
                this.m_momentumHistory[this.m_momentumCounter] = num4;
                this.m_momentumCounter++;
                this.m_momentumTotal++;
                flag = (this.m_position < 0f) || (this.m_position > this.m_maxPosition);
                if (this.m_momentumCounter >= this.m_momentumHistory.Length)
                {
                    this.m_momentumCounter = 0;
                }
                if (this.m_momentumTotal >= this.m_momentumHistory.Length)
                {
                    this.m_momentumTotal = this.m_momentumHistory.Length;
                }
                this.m_touchX = x;
                float num6 = Screen.height * 0.1f;
                float num7 = Screen.height * 0.275f;
                if (((this.m_itemPulledListener != null) && ((Input.mousePosition.y - this.m_touchStart.y) > num6)) && (Input.mousePosition.y > num7))
                {
                    this.m_itemPulledListener(this.m_hitItem, this.m_hitIndex);
                    this.m_touchActive = false;
                    this.m_velocity = 0f;
                    this.SettlePosition(0f);
                }
                if (!Input.GetMouseButton(0))
                {
                    if (!this.m_noMouseMovement && this.m_scrolling)
                    {
                        this.m_velocity = this.CalculateVelocity();
                        if (this.m_position < 0f)
                        {
                            this.m_targetPosition = 0f;
                            this.m_velocity = 0f;
                        }
                        else if (this.m_position >= this.m_maxPosition)
                        {
                            this.m_targetPosition = this.m_maxPosition;
                            this.m_velocity = 0f;
                        }
                        else if (Math.Abs(this.m_velocity) < 0.03f)
                        {
                            this.SettlePosition(this.m_velocity);
                            this.m_velocity = 0f;
                        }
                    }
                    if (this.m_itemReleasedListener != null)
                    {
                        this.m_itemReleasedListener();
                    }
                    this.m_touchActive = false;
                }
            }
            int index = this.MouseHit(out item);
            if (mouseDown && (index >= 0))
            {
                this.m_touchActive = true;
                this.m_touchX = Input.mousePosition.x;
                this.m_touchStart = Input.mousePosition;
                this.m_velocity = 0f;
                this.m_hitIndex = index;
                this.m_hitItem = item;
                this.m_scrolling = false;
                this.m_settleCallbackCalled = false;
                if (this.m_trackItemHit)
                {
                    RaycastHit hit;
                    UniversalInputManager.Get().GetInputHitInfo(out hit);
                    this.m_hitWorldPosition = hit.point;
                    this.m_hitCarouselPosition = this.GetCarouselPosition(this.m_hitWorldPosition.x);
                    this.m_startX = this.m_position;
                }
                this.InitVelocity();
                if (this.m_itemClickedListener != null)
                {
                    this.m_itemClickedListener(this.m_hitItem, index);
                }
            }
            if (!this.m_touchActive && (this.m_velocity != 0f))
            {
                if ((Math.Abs(this.m_velocity) < 0.03f) || flag)
                {
                    this.SettlePosition(this.m_velocity);
                    this.m_velocity = 0f;
                }
                else
                {
                    this.m_position += this.m_velocity;
                    this.m_velocity -= 0.015f * Math.Sign(this.m_velocity);
                }
            }
            if ((!this.m_touchActive && (this.m_targetPosition != this.m_position)) && (this.m_velocity == 0f))
            {
                this.m_position = (this.m_targetPosition * 0.15f) + (this.m_position * 0.85f);
                if (!this.m_settleCallbackCalled && (Math.Abs((float) (this.m_position - this.m_targetPosition)) < 0.1f))
                {
                    this.m_settleCallbackCalled = true;
                    if (this.m_carouselSettledListener != null)
                    {
                        this.m_carouselSettledListener();
                    }
                }
                if (Math.Abs((float) (this.m_position - this.m_targetPosition)) < 0.01f)
                {
                    this.m_position = this.m_targetPosition;
                    this.m_scrolling = false;
                }
            }
            this.m_intPosition = (int) Mathf.Round(this.m_position);
            this.UpdateVisibleItems();
            if (this.m_doFlyIn)
            {
                float num9 = Math.Min(0.03f, UnityEngine.Time.deltaTime);
                this.m_flyInState += num9 * 8f;
                if (this.m_flyInState > this.m_bones.Length)
                {
                    this.m_doFlyIn = false;
                }
            }
        }
    }

    private void UpdateVisibleItems()
    {
        float position = this.m_position;
        int num2 = Mathf.FloorToInt(position);
        float num3 = position - num2;
        float num4 = 1f - num3;
        int num5 = 0;
        for (int i = 0; i < this.m_items.Length; i++)
        {
            int index = ((i - num2) + this.m_radius) - 1;
            int num8 = (i - num2) + this.m_radius;
            if ((index < 0) || (num8 >= this.m_bones.Length))
            {
                this.m_items[i].Hide();
            }
            else
            {
                this.m_items[i].Show(this);
                if (this.m_items[i].IsLoaded())
                {
                    Vector3 localPosition = this.m_bones[index].transform.localPosition;
                    Vector3 vector2 = this.m_bones[num8].transform.localPosition;
                    Vector3 localScale = this.m_bones[index].transform.localScale;
                    Vector3 vector4 = this.m_bones[num8].transform.localScale;
                    Quaternion localRotation = this.m_bones[index].transform.localRotation;
                    Quaternion quaternion2 = this.m_bones[num8].transform.localRotation;
                    Vector3 vector5 = new Vector3((localPosition.x * num3) + (vector2.x * num4), (localPosition.y * num3) + (vector2.y * num4), (localPosition.z * num3) + (vector2.z * num4));
                    Vector3 vector6 = new Vector3((localScale.x * num3) + (vector4.x * num4), (localScale.y * num3) + (vector4.y * num4), (localScale.z * num3) + (vector4.z * num4));
                    Quaternion quaternion3 = new Quaternion((localRotation.x * num3) + (quaternion2.x * num4), (localRotation.y * num3) + (quaternion2.y * num4), (localRotation.z * num3) + (quaternion2.z * num4), (localRotation.w * num3) + (quaternion2.w * num4));
                    if (this.m_doFlyIn)
                    {
                        float num9 = 1f;
                        if (num5 >= (((int) this.m_flyInState) + 1))
                        {
                            num9 = 0f;
                        }
                        else if (num5 >= ((int) this.m_flyInState))
                        {
                            num9 = this.m_flyInState - ((float) Math.Floor((double) this.m_flyInState));
                        }
                        float num10 = 1f - num9;
                        Vector3 vector7 = new Vector3(81f, 9.4f, 4f);
                        this.m_items[i].GetGameObject().transform.localPosition = new Vector3((num9 * vector5.x) + (num10 * vector7.x), (num9 * vector5.y) + (num10 * vector7.y), (num9 * vector5.z) + (num10 * vector7.z));
                    }
                    else
                    {
                        this.m_items[i].GetGameObject().transform.localPosition = vector5;
                    }
                    this.m_items[i].GetGameObject().transform.localScale = vector6;
                    this.m_items[i].GetGameObject().transform.localRotation = quaternion3;
                }
                num5++;
            }
        }
    }

    public delegate void CarouselMoved();

    public delegate void CarouselSettled();

    public delegate void CarouselStartedScrolling();

    public delegate void ItemClicked(CarouselItem item, int index);

    public delegate void ItemPulled(CarouselItem item, int index);

    public delegate void ItemReleased();
}

