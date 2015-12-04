using System;
using UnityEngine;

public class ManaCounter : MonoBehaviour
{
    public UberText m_availableManaPhone;
    public UberText m_permanentManaPhone;
    private GameObject m_phoneGem;
    public GameObject m_phoneGemContainer;
    private Player m_player;
    public Player.Side m_Side;
    private UberText m_textMesh;

    private void Awake()
    {
        this.m_textMesh = base.GetComponent<UberText>();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_phoneGem = AssetLoader.Get().LoadActor("Resource_Large_phone", false, false);
            GameUtils.SetParent(this.m_phoneGem, this.m_phoneGemContainer, true);
        }
    }

    public GameObject GetPhoneGem()
    {
        return this.m_phoneGem;
    }

    public Player GetPlayer()
    {
        return this.m_player;
    }

    public void SetPlayer(Player player)
    {
        this.m_player = player;
    }

    private void Start()
    {
        object[] args = new object[] { "0", "0" };
        this.m_textMesh.Text = GameStrings.Format("GAMEPLAY_MANA_COUNTER", args);
    }

    public void UpdateText()
    {
        string str;
        int tag = this.m_player.GetTag(GAME_TAG.RESOURCES);
        if (!base.gameObject.activeInHierarchy)
        {
            base.gameObject.SetActive(true);
        }
        int numAvailableResources = this.m_player.GetNumAvailableResources();
        if ((UniversalInputManager.UsePhoneUI != null) && (tag >= 10))
        {
            str = numAvailableResources.ToString();
        }
        else
        {
            object[] args = new object[] { numAvailableResources, tag };
            str = GameStrings.Format("GAMEPLAY_MANA_COUNTER", args);
        }
        this.m_textMesh.Text = str;
        if ((UniversalInputManager.UsePhoneUI != null) && (this.m_availableManaPhone != null))
        {
            this.m_availableManaPhone.Text = numAvailableResources.ToString();
            this.m_permanentManaPhone.Text = tag.ToString();
        }
    }
}

