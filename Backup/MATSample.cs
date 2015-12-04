using System;
using UnityEngine;

public class MATSample : MonoBehaviour
{
    private void Awake()
    {
    }

    public static string getSampleiTunesIAPReceipt()
    {
        return "dGhpcyBpcyBhIHNhbXBsZSBpb3MgYXBwIHN0b3JlIHJlY2VpcHQ=";
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle {
            fontStyle = FontStyle.Bold,
            fontSize = 50,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10f, 5f, (float) (Screen.width - 20), (float) (Screen.height / 10)), "MAT Unity Test App", style);
        GUI.skin.button.fontSize = 40;
        if (GUI.Button(new Rect(10f, (float) (Screen.height / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Start MAT"))
        {
            MonoBehaviour.print("Start MAT clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((2 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Set Delegate"))
        {
            MonoBehaviour.print("Set Delegate clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((3 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Enable Debug Mode"))
        {
            MonoBehaviour.print("Enable Debug Mode clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((4 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Allow Duplicates"))
        {
            MonoBehaviour.print("Allow Duplicates clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((5 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Measure Session"))
        {
            MonoBehaviour.print("Measure Session clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((6 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Measure Event"))
        {
            MonoBehaviour.print("Measure Event clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((7 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Measure Event With Event Items"))
        {
            MonoBehaviour.print("Measure Event With Event Items clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((8 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Test Setter Methods"))
        {
            MonoBehaviour.print("Test Setter Methods clicked");
        }
        else if (GUI.Button(new Rect(10f, (float) ((9 * Screen.height) / 10), (float) (Screen.width - 20), (float) (Screen.height / 10)), "Test Getter Methods"))
        {
            MonoBehaviour.print("Test Getter Methods clicked");
        }
    }
}

