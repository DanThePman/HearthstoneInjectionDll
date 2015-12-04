using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FatalErrorDialog : MonoBehaviour
{
    private const float ButtonHeight = 31f;
    private const float ButtonWidth = 100f;
    private const float DialogHeight = 347f;
    private const float DialogPadding = 20f;
    private const float DialogWidth = 600f;
    private GUIStyle m_dialogStyle;
    private string m_text;

    private void Awake()
    {
        this.BuildText();
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private void BuildText()
    {
        List<FatalErrorMessage> messages = FatalErrorMgr.Get().GetMessages();
        if (messages.Count == 0)
        {
            this.m_text = string.Empty;
        }
        else
        {
            List<string> list2 = new List<string>();
            for (int i = 0; i < messages.Count; i++)
            {
                FatalErrorMessage message = messages[i];
                string text = message.m_text;
                if (!list2.Contains(text))
                {
                    list2.Add(text);
                }
            }
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < list2.Count; j++)
            {
                string str2 = list2[j];
                builder.Append(str2);
                builder.Append("\n");
            }
            builder.Remove(builder.Length - 1, 1);
            this.m_text = builder.ToString();
        }
    }

    private void InitGUIStyles()
    {
        if (this.m_dialogStyle == null)
        {
            Log.Mike.Print("FatalErrorDialog.InitGUIStyles()", new object[0]);
            GUIStyle style = new GUIStyle("box") {
                clipping = TextClipping.Overflow,
                stretchHeight = true,
                stretchWidth = true,
                wordWrap = true,
                fontSize = 0x10
            };
            this.m_dialogStyle = style;
        }
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.BuildText();
    }

    private void OnGUI()
    {
        this.InitGUIStyles();
        GUI.Box(this.DialogRect, string.Empty, this.m_dialogStyle);
        GUI.Box(this.DialogRect, this.m_text, this.m_dialogStyle);
        if (GUI.Button(this.ButtonRect, GameStrings.Get("GLOBAL_EXIT")))
        {
            Log.Mike.Print("FatalErrorDialog.OnGUI() - calling FatalErrorMgr.Get().NotifyExitPressed()", new object[0]);
            FatalErrorMgr.Get().NotifyExitPressed();
            Log.Mike.Print("FatalErrorDialog.OnGUI() - called FatalErrorMgr.Get().NotifyExitPressed()", new object[0]);
        }
    }

    private float ButtonLeft
    {
        get
        {
            return ((Screen.width - 100f) / 2f);
        }
    }

    private Rect ButtonRect
    {
        get
        {
            return new Rect(this.ButtonLeft, this.ButtonTop, 100f, 31f);
        }
    }

    private float ButtonTop
    {
        get
        {
            return (((this.DialogTop + 347f) - 20f) - 31f);
        }
    }

    private float DialogLeft
    {
        get
        {
            return ((Screen.width - 600f) / 2f);
        }
    }

    private Rect DialogRect
    {
        get
        {
            return new Rect(this.DialogLeft, this.DialogTop, 600f, 347f);
        }
    }

    private float DialogTop
    {
        get
        {
            return ((Screen.height - 347f) / 2f);
        }
    }
}

