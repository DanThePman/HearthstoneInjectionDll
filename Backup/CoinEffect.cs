using System;
using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    private string animToUse;
    public GameObject coin;
    private string coinDropAnim = "MulliganCoinDropGo2Card";
    private string coinDropAnim2 = "MulliganCoinDrop2_Edit";
    public GameObject coinGlow;
    private string coinGlowDropAnim = "MulliganCoinDrop1Glow_Edit";
    private string coinGlowDropAnim2 = "MulliganCoinDrop2Glow_Edit";
    private string coinSpawnAnim = "CoinSpawn1_edit";
    public GameObject coinSpawnObject;
    private string GlowanimToUse;

    public void DoAnim(bool localWin)
    {
        if (localWin)
        {
            this.animToUse = this.coinDropAnim2;
            this.GlowanimToUse = this.coinGlowDropAnim2;
        }
        else
        {
            this.animToUse = this.coinDropAnim;
            this.GlowanimToUse = this.coinGlowDropAnim;
        }
        this.coinSpawnObject.SetActive(true);
        this.coin.SetActive(true);
        this.coinGlow.SetActive(true);
        this.coinSpawnObject.GetComponent<Animation>().Stop(this.coinSpawnAnim);
        this.coin.GetComponent<Animation>().Stop(this.animToUse);
        this.coinGlow.GetComponent<Animation>().Stop(this.GlowanimToUse);
        this.coinSpawnObject.GetComponent<Animation>().Play(this.coinSpawnAnim);
        this.coin.GetComponent<Animation>().Play(this.animToUse);
        this.coinGlow.GetComponent<Animation>().Play(this.GlowanimToUse);
    }

    private void Start()
    {
    }
}

