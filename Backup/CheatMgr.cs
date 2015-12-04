using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class CheatMgr : MonoBehaviour
{
    [CompilerGenerated]
    private static ApplicationMgr.ScheduledCallback <>f__am$cacheB;
    private Map<string, string> m_cheatAlias = new Map<string, string>();
    private Map<string, string> m_cheatArgs = new Map<string, string>();
    private Map<string, string> m_cheatDesc = new Map<string, string>();
    private Map<string, string> m_cheatExamples = new Map<string, string>();
    private List<string> m_cheatHistory;
    private int m_cheatHistoryIndex = -1;
    private Rect m_cheatInputBackground;
    private string m_cheatTextBeforeScrollingThruHistory;
    private Map<string, List<ProcessCheatCallback>> m_funcMap = new Map<string, List<ProcessCheatCallback>>();
    private bool m_inputActive;
    private const int MAX_HISTORY_LINES = 0x19;
    private static CheatMgr s_instance;

    public void Awake()
    {
        s_instance = this;
        this.m_cheatHistory = new List<string>();
        Cheats.Initialize();
    }

    private int ComputeLongestFuncIndex(List<string> funcs)
    {
        int num = 0;
        for (int i = 1; i < funcs.Count; i++)
        {
            string str = funcs[i];
            if (str.Length > funcs[num].Length)
            {
                num = i;
            }
        }
        return num;
    }

    private string ExtractFunc(string inputCommand)
    {
        char[] trimChars = new char[] { '/' };
        inputCommand = inputCommand.TrimStart(trimChars);
        inputCommand = inputCommand.Trim();
        int num = 0;
        List<string> funcs = new List<string>();
        foreach (string str in this.m_funcMap.Keys)
        {
            funcs.Add(str);
            if (str.Length > funcs[num].Length)
            {
                num = funcs.Count - 1;
            }
        }
        foreach (string str2 in this.m_cheatAlias.Keys)
        {
            funcs.Add(str2);
            if (str2.Length > funcs[num].Length)
            {
                num = funcs.Count - 1;
            }
        }
        int num2 = 0;
        while (num2 < inputCommand.Length)
        {
            char c = inputCommand[num2];
            int index = 0;
            while (index < funcs.Count)
            {
                string str3 = funcs[index];
                if (num2 == str3.Length)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        return str3;
                    }
                    funcs.RemoveAt(index);
                    if (index <= num)
                    {
                        num = this.ComputeLongestFuncIndex(funcs);
                    }
                }
                else if (str3[num2] != c)
                {
                    funcs.RemoveAt(index);
                    if (index <= num)
                    {
                        num = this.ComputeLongestFuncIndex(funcs);
                    }
                }
                else
                {
                    index++;
                }
            }
            if (funcs.Count == 0)
            {
                return null;
            }
            num2++;
        }
        if (funcs.Count > 1)
        {
            foreach (string str4 in funcs)
            {
                if (inputCommand == str4)
                {
                    return str4;
                }
            }
            return null;
        }
        string str5 = funcs[0];
        if (num2 < str5.Length)
        {
            return null;
        }
        return str5;
    }

    public static CheatMgr Get()
    {
        return s_instance;
    }

    public Map<string, List<ProcessCheatCallback>>.KeyCollection GetCheatCommands()
    {
        return this.m_funcMap.Keys;
    }

    private string GetOriginalFunc(string func)
    {
        string str;
        if (!this.m_cheatAlias.TryGetValue(func, out str))
        {
            str = func;
        }
        return str;
    }

    public bool HandleKeyboardInput()
    {
        if (ApplicationMgr.IsPublic())
        {
            return false;
        }
        if (!Input.GetKeyUp(KeyCode.BackQuote))
        {
            return false;
        }
        Rect rect = new Rect(0f, 0f, 1f, 0.05f);
        this.m_cheatInputBackground = rect;
        this.m_cheatInputBackground.x *= Screen.width * 0.95f;
        this.m_cheatInputBackground.y *= Screen.height;
        this.m_cheatInputBackground.width *= Screen.width;
        this.m_cheatInputBackground.height *= Screen.height * 1.03f;
        this.m_inputActive = true;
        this.m_cheatHistoryIndex = -1;
        this.ReadCheatHistoryOption();
        this.m_cheatTextBeforeScrollingThruHistory = null;
        UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
            m_owner = base.gameObject,
            m_preprocessCallback = new UniversalInputManager.TextInputPreprocessCallback(this.OnInputPreprocess),
            m_rect = rect,
            m_color = new Color?(Color.white),
            m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(this.OnInputComplete)
        };
        UniversalInputManager.Get().UseTextInput(parms, false);
        return true;
    }

    public void OnGUI()
    {
        if (this.m_inputActive)
        {
            if (!UniversalInputManager.Get().IsTextInputActive())
            {
                this.m_inputActive = false;
            }
            else
            {
                GUI.depth = 0x3e8;
                GUI.backgroundColor = Color.black;
                GUI.Box(this.m_cheatInputBackground, GUIContent.none);
                GUI.Box(this.m_cheatInputBackground, GUIContent.none);
                GUI.Box(this.m_cheatInputBackground, GUIContent.none);
            }
        }
    }

    private void OnInputComplete(string inputCommand)
    {
        this.m_inputActive = false;
        inputCommand = inputCommand.TrimStart(new char[0]);
        if (!string.IsNullOrEmpty(inputCommand))
        {
            string str = this.ProcessCheat(inputCommand);
            if (!string.IsNullOrEmpty(str))
            {
                UIStatus.Get().AddError(str);
            }
        }
    }

    private bool OnInputPreprocess(Event e)
    {
        string cheatTextBeforeScrollingThruHistory;
        if (e.type != EventType.KeyDown)
        {
            return false;
        }
        KeyCode keyCode = e.keyCode;
        if ((keyCode == KeyCode.BackQuote) && string.IsNullOrEmpty(UniversalInputManager.Get().GetInputText()))
        {
            UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
            return true;
        }
        if (this.m_cheatHistory.Count < 1)
        {
            return false;
        }
        if (keyCode == KeyCode.UpArrow)
        {
            if (this.m_cheatHistoryIndex < (this.m_cheatHistory.Count - 1))
            {
                string inputText = UniversalInputManager.Get().GetInputText();
                if (this.m_cheatTextBeforeScrollingThruHistory == null)
                {
                    this.m_cheatTextBeforeScrollingThruHistory = inputText;
                }
                string text = this.m_cheatHistory[++this.m_cheatHistoryIndex];
                UniversalInputManager.Get().SetInputText(text);
                if (<>f__am$cacheB == null)
                {
                    <>f__am$cacheB = delegate (object u) {
                        TextEditor stateObject = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        if (stateObject != null)
                        {
                            stateObject.MoveTextEnd();
                        }
                    };
                }
                ApplicationMgr.Get().ScheduleCallback(0f, false, <>f__am$cacheB, null);
            }
            return true;
        }
        if (keyCode != KeyCode.DownArrow)
        {
            return false;
        }
        if (this.m_cheatHistoryIndex <= 0)
        {
            this.m_cheatHistoryIndex = -1;
            if (this.m_cheatTextBeforeScrollingThruHistory == null)
            {
                return false;
            }
            cheatTextBeforeScrollingThruHistory = this.m_cheatTextBeforeScrollingThruHistory;
            this.m_cheatTextBeforeScrollingThruHistory = null;
        }
        else
        {
            cheatTextBeforeScrollingThruHistory = this.m_cheatHistory[--this.m_cheatHistoryIndex];
        }
        UniversalInputManager.Get().SetInputText(cheatTextBeforeScrollingThruHistory);
        return true;
    }

    public string ProcessCheat(string inputCommand)
    {
        string str2;
        string[] strArray;
        string func = this.ExtractFunc(inputCommand);
        if (func == null)
        {
            char[] separator = new char[] { ' ' };
            return ("\"" + inputCommand.Split(separator)[0] + "\" cheat command not found!");
        }
        int length = func.Length;
        if (length == inputCommand.Length)
        {
            str2 = string.Empty;
            strArray = new string[] { string.Empty };
        }
        else
        {
            str2 = inputCommand.Remove(0, length + 1);
            MatchCollection matchs = Regex.Matches(str2, @"\S+");
            if (matchs.Count == 0)
            {
                strArray = new string[] { string.Empty };
            }
            else
            {
                strArray = new string[matchs.Count];
                for (int j = 0; j < matchs.Count; j++)
                {
                    strArray[j] = matchs[j].Value;
                }
            }
        }
        string originalFunc = this.GetOriginalFunc(func);
        List<ProcessCheatCallback> list = this.m_funcMap[originalFunc];
        bool flag = false;
        for (int i = 0; i < list.Count; i++)
        {
            ProcessCheatCallback callback = list[i];
            flag = callback(func, strArray, str2) || flag;
        }
        if (flag && ((this.m_cheatHistory.Count < 1) || !this.m_cheatHistory[0].Equals(inputCommand)))
        {
            this.m_cheatHistory.Remove(inputCommand);
            this.m_cheatHistory.Insert(0, inputCommand);
        }
        if (this.m_cheatHistory.Count > 0x19)
        {
            this.m_cheatHistory.RemoveRange(0x18, this.m_cheatHistory.Count - 0x19);
        }
        this.m_cheatHistoryIndex = 0;
        this.m_cheatTextBeforeScrollingThruHistory = null;
        this.WriteCheatHistoryOption();
        if (!flag)
        {
            return ("\"" + func + "\" cheat command executed, but failed!");
        }
        return null;
    }

    private void ReadCheatHistoryOption()
    {
        string str = Options.Get().GetString(Option.CHEAT_HISTORY);
        char[] separator = new char[] { ';' };
        this.m_cheatHistory = new List<string>(str.Split(separator));
    }

    public void RegisterCheatAlias(string func, params string[] aliases)
    {
        List<ProcessCheatCallback> list;
        if (!this.m_funcMap.TryGetValue(func, out list))
        {
            Debug.LogError(string.Format("CheatMgr.RegisterCheatAlias() - cannot register aliases for func {0} because it does not exist", func));
        }
        else
        {
            foreach (string str in aliases)
            {
                this.m_cheatAlias[str] = func;
            }
        }
    }

    public void RegisterCheatHandler(string func, ProcessCheatCallback callback, string desc = null, string argDesc = null, string exampleArgs = null)
    {
        this.RegisterCheatHandler_(func, callback);
        if (desc != null)
        {
            this.m_cheatDesc[func] = desc;
        }
        if (argDesc != null)
        {
            this.m_cheatArgs[func] = argDesc;
        }
        if (exampleArgs != null)
        {
            this.m_cheatExamples[func] = exampleArgs;
        }
    }

    private void RegisterCheatHandler_(string func, ProcessCheatCallback callback)
    {
        if (string.IsNullOrEmpty(func.Trim()))
        {
            Debug.LogError("CheatMgr.RegisterCheatHandler() - FAILED to register a null, empty, or all-whitespace function name");
        }
        else
        {
            List<ProcessCheatCallback> list;
            if (this.m_funcMap.TryGetValue(func, out list))
            {
                if (!list.Contains(callback))
                {
                    list.Add(callback);
                }
            }
            else
            {
                list = new List<ProcessCheatCallback>();
                this.m_funcMap.Add(func, list);
                list.Add(callback);
            }
        }
    }

    public void UnregisterCheatHandler(string func, ProcessCheatCallback callback)
    {
        this.UnregisterCheatHandler_(func, callback);
    }

    private void UnregisterCheatHandler_(string func, ProcessCheatCallback callback)
    {
        List<ProcessCheatCallback> list;
        if (this.m_funcMap.TryGetValue(func, out list))
        {
            list.Remove(callback);
        }
    }

    private void WriteCheatHistoryOption()
    {
        Options.Get().SetString(Option.CHEAT_HISTORY, string.Join(";", this.m_cheatHistory.ToArray()));
    }

    public Map<string, string> cheatArgs
    {
        get
        {
            return this.m_cheatArgs;
        }
    }

    public Map<string, string> cheatDesc
    {
        get
        {
            return this.m_cheatDesc;
        }
    }

    public Map<string, string> cheatExamples
    {
        get
        {
            return this.m_cheatExamples;
        }
    }

    public delegate bool ProcessCheatCallback(string func, string[] args, string rawArgs);
}

