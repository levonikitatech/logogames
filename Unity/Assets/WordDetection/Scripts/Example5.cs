using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example5 : Example4
{
    /// <summary>
    /// Reference to cube
    /// </summary>
    public Transform Cube = null;

    enum Commands
    {
        Idle,
        Reset,
        Grow,
        Shrink,
        Left,
        Right,
        Up,
        Down,
    }

    Commands m_command = Commands.Idle;    

    /// <summary>
    /// Initialize the example
    /// </summary>
    protected override void Start()
    {
        if (null == AudioWordDetection ||
            null == Mic)
        {
            Debug.LogError("Missing meta references");
            return;
        }

        // prepopulate words
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Noise" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Reset" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Grow" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Shrink" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Left" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Right" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Up" });
        AudioWordDetection.Words.Add(new WordDetails() { Label = "Down" });

        //subscribe detection event
        AudioWordDetection.WordDetectedEvent += WordDetectedHandler;
    }

    Vector3 m_resetPosition = new Vector3(0, 0, 11);
    Vector3 m_resetRotation = new Vector3(0, 0, 0);
    Vector3 m_resetScale = new Vector3(10, 15, 2);
    Vector3 m_cubeShrink = new Vector3(5, 10, 1);
    Vector3 m_cubeGrow = new Vector3(10, 26, 4);
    Vector3 m_cubeUp = new Vector3(0, 5, 11);
    Vector3 m_cubeDown = new Vector3(0, -5, 11);

    /// <summary>
    /// Handle word detected event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void WordDetectedHandler(object sender, WordDetection.WordEventArgs args)
    {
        if (null == args.Details)
        {
            return;
        }

        if (args.Details.Label == "Reset")
        {
            m_command = Commands.Reset;
        }
        else if (args.Details.Label == "Grow")
        {
            m_command = Commands.Grow;
        }
        else if (args.Details.Label == "Shrink")
        {
            m_command = Commands.Shrink;
        }
        else if (args.Details.Label == "Left")
        {
            m_command = Commands.Left;
        }
        else if (args.Details.Label == "Right")
        {
            m_command = Commands.Right;
        }
        else if (args.Details.Label == "Up")
        {
            m_command = Commands.Up;
        }
        else if (args.Details.Label == "Down")
        {
            m_command = Commands.Down;
        }
    }

    protected virtual void ExampleUpdate()
    {
        if (null == Cube)
        {
            return;
        }

        Vector3 val;
        switch (m_command)
        {
            case Commands.Reset:
                Cube.localPosition = Vector3.Lerp(Cube.localPosition, m_resetPosition, Time.deltaTime);
                Cube.localRotation = Quaternion.Euler(Vector3.Lerp(Cube.localRotation.eulerAngles, m_resetRotation, Time.deltaTime));
                Cube.localScale = Vector3.Lerp(Cube.localScale, m_resetScale, Time.deltaTime);
                break;
            case Commands.Grow:
                Cube.localScale = Vector3.Lerp(Cube.localScale, m_cubeGrow, Time.deltaTime);
                break;
            case Commands.Shrink:
                Cube.localScale = Vector3.Lerp(Cube.localScale, m_cubeShrink, Time.deltaTime);
                break;
            case Commands.Left:
                val = Cube.localRotation.eulerAngles;
                val.y += 90 * Time.deltaTime;
                Cube.localRotation = Quaternion.Euler(val);
                break;
            case Commands.Right:
                val = Cube.localRotation.eulerAngles;
                val.y -= 90 * Time.deltaTime;
                Cube.localRotation = Quaternion.Euler(val);
                break;
            case Commands.Up:
                Cube.localPosition = Vector3.Lerp(Cube.localPosition, m_cubeUp, Time.deltaTime);
                break;
            case Commands.Down:
                Cube.localPosition = Vector3.Lerp(Cube.localPosition, m_cubeDown, Time.deltaTime);
                break;
        }
    }

    private const string FILE_PROFILES = "VerbalCommand_Example5.profiles";
   
    /// <summary>
    /// GUI event
    /// </summary>
    protected override void OnGUI()
    {
        ExampleUpdate();

        if (null == AudioWordDetection ||
            null == Mic ||
            string.IsNullOrEmpty(Mic.DeviceName))
        {
            return;
        }

        DisplayProfileLoadSave(FILE_PROFILES);

        Color backgroundColor = GUI.backgroundColor;

        for (int wordIndex = 0; wordIndex < AudioWordDetection.Words.Count; ++wordIndex)
        {
            if (AudioWordDetection.ClosestIndex == wordIndex)
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = backgroundColor;
            }

            WordDetails noise = GetWord(WORD_NOISE);

            if (null == noise)
            {
                continue;
            }

            if (wordIndex > 0)
            {
                GUI.enabled = null != noise.SpectrumReal;
            }

            GUILayout.BeginHorizontal();
            WordDetails details = AudioWordDetection.Words[wordIndex];

            if (GUILayout.Button("Play", GUILayout.Height(45)))
            {
                if (null != details.Audio)
                {
                    if (NormalizeWave)
                    {
                        audio.PlayOneShot(details.Audio, 0.1f);
                    }
                    else
                    {
                        audio.PlayOneShot(details.Audio);
                    }
                }

                // show profile
                RefExample.OverrideSpectrumImag = true;
                RefExample.SpectrumImag = details.SpectrumReal;
            }

            if (wordIndex == 0)
            {
                GUILayout.Label(details.Label, GUILayout.Width(150), GUILayout.Height(45));
            }
            else
            {
                details.Label = GUILayout.TextField(details.Label, GUILayout.Width(150), GUILayout.Height(45));
            }

            GUILayout.Button(string.Format("{0}",
                    (null == details.SpectrumReal) ? "not set" : "set"), GUILayout.Height(45));

            Event e = Event.current;
            if (null != e)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                bool overButton = rect.Contains(e.mousePosition);

                if (m_buttonIndex == -1 &&
                    m_timerStart == DateTime.MinValue &&
                    Input.GetMouseButton(0) &&
                    overButton)
                {
                    //Debug.Log("Initial button down");
                    m_buttonIndex = wordIndex;
                    m_startPosition = Mic.GetPosition();
                    m_timerStart = DateTime.Now + TimeSpan.FromSeconds(Mic.CaptureTime);
                }
                if (m_buttonIndex == wordIndex)
                {
                    bool buttonUp = Input.GetMouseButtonUp(0);
                    if (m_timerStart > DateTime.Now &&
                        !buttonUp)
                    {
                        //Debug.Log("Button still pressed");
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        m_timerStart < DateTime.Now)
                    {
                        //Debug.Log("Button timed out");
                        SetupWordProfile(false);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        buttonUp &&
                        m_buttonIndex != -1)
                    {
                        //Debug.Log("Button is no longer pressed");
                        SetupWordProfile(true);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                    }
                }
            }
            GUILayout.Label(details.Score.ToString());
            //GUILayout.Label(string.Format("{0}", details.GetMinScore(DateTime.Now - TimeSpan.FromSeconds(1))));
            GUILayout.EndHorizontal();

            if (wordIndex > 0)
            {
                GUI.enabled = null != noise.SpectrumReal;
            }
            
            GUILayout.Space(10);
        }

        GUI.backgroundColor = backgroundColor;
    }
}