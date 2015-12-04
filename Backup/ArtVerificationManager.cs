using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ArtVerificationManager : MonoBehaviour
{
    private int m_cardsToLoad;
    private const float START_DELAY_SEC = 1f;

    private void CleanUpCard(CardDef def)
    {
        if (def != null)
        {
            UnityEngine.Object.Destroy(def);
        }
    }

    private void FinishVerification()
    {
        UnityEngine.Debug.Log("Finished");
        GeneralUtils.ExitApplication();
    }

    private void LoadCards()
    {
        GameDbf.Load();
        List<string> allCardIds = GameUtils.GetAllCardIds();
        this.m_cardsToLoad = allCardIds.Count;
        foreach (string str in allCardIds)
        {
            CardPortraitQuality quality = new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN);
            DefLoader.Get().LoadCardDef(str, new DefLoader.LoadDefCallback<CardDef>(this.OnCardDefLoaded), null, quality);
        }
    }

    private void OnCardDefLoaded(string cardID, CardDef def, object userData)
    {
        this.m_cardsToLoad--;
        this.CleanUpCard(def);
        if (this.m_cardsToLoad <= 0)
        {
            this.FinishVerification();
        }
    }

    private void Start()
    {
        base.StartCoroutine(this.StartVerification());
    }

    [DebuggerHidden]
    private IEnumerator StartVerification()
    {
        return new <StartVerification>c__Iterator296 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <StartVerification>c__Iterator296 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ArtVerificationManager <>f__this;

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
                    UnityEngine.Debug.Log("Preparing to verify art.");
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    goto Label_0085;

                case 1:
                    UnityEngine.Debug.Log("Starting art verification now. This may take a few minutes.");
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 2;
                    goto Label_0085;

                case 2:
                    this.<>f__this.LoadCards();
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0085:
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

