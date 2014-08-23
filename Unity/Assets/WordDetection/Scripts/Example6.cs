using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example6 : Example4
{
    /// <summary>
    /// Reference to cube
    /// </summary>
    public Transform Cube = null;

    private const string WORD_PUSH_TO_TALK = "Push To Talk";
    private const string WORD_RESET = "Reset";
    private const string WORD_GROW = "Grow";
    private const string WORD_SHRINK = "Shrink";
    private const string WORD_LEFT = "Left";
    private const string WORD_RIGHT = "Right";
    private const string WORD_UP = "Up";
    private const string WORD_DOWN = "Down";

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

        WordDetails wordNoise = new WordDetails() { Label = WORD_NOISE };

        // prepopulate words
        AudioWordDetection.Words.Add(wordNoise);
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_PUSH_TO_TALK });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_RESET });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_GROW });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_SHRINK });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_LEFT });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_RIGHT });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_UP });
        AudioWordDetection.Words.Add(new WordDetails() { Label = WORD_DOWN });

        AudioWordDetection.WordsToIgnore.Add(WORD_PUSH_TO_TALK);

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

        Debug.Log(args.Details.Label);

        if (args.Details.Label == GetWord(WORD_RESET).Label)
        {
            m_command = Commands.Reset;
        }
        else if (args.Details.Label == GetWord(WORD_GROW).Label)
        {
            m_command = Commands.Grow;
        }
        else if (args.Details.Label == GetWord(WORD_SHRINK).Label)
        {
            m_command = Commands.Shrink;
        }
        else if (args.Details.Label == GetWord(WORD_LEFT).Label)
        {
            m_command = Commands.Left;
        }
        else if (args.Details.Label == GetWord(WORD_RIGHT).Label)
        {
            m_command = Commands.Right;
        }
        else if (args.Details.Label == GetWord(WORD_UP).Label)
        {
            m_command = Commands.Up;
        }
        else if (args.Details.Label == GetWord(WORD_DOWN).Label)
        {
            m_command = Commands.Down;
        }
    }

    void Update()
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

    protected override void SetupWordProfile(bool playAudio)
    {
        if (null == AudioWordDetection)
        {
            return;
        }

        base.SetupWordProfile(playAudio);

        if (m_buttonIndex == 1)
        {
            AudioWordDetection.DetectWords(GetWord(WORD_PUSH_TO_TALK).Wave);
        }
    }

    private const string FILE_PROFILES = "VerbalCommand_Example6.profiles";
   
    /// <summary>
    /// GUI event
    /// </summary>
    protected override void OnGUI()
    {        
        if (null == AudioWordDetection ||
            null == Mic ||
            string.IsNullOrEmpty(Mic.DeviceName))
        {
            return;
        }

        DisplayProfileLoadSave(FILE_PROFILES);

        GUILayout.Space(40);

        GUILayout.Label(string.Format("Active Command: {0}", m_command));

        Color backgroundColor = GUI.backgroundColor;

        for (int wordIndex = 0; wordIndex < AudioWordDetection.Words.Count; ++wordIndex)
        {
            WordDetails details = AudioWordDetection.Words[wordIndex];

            if (null == details)
            {
                continue;
            }

            if (AudioWordDetection.ClosestIndex == wordIndex)
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = backgroundColor;
            }

            if (details != GetWord(WORD_NOISE))
            {
                GUI.enabled = (null != GetWord(WORD_NOISE) && null != GetWord(WORD_NOISE).SpectrumReal);
            }

            bool showRow = true;
            if (details == GetWord(WORD_NOISE))
            {
                GUILayout.Label("First: Record a noise sample");
            }
            else if (details == GetWord(WORD_PUSH_TO_TALK))
            {
                showRow = false;
            }

            if (showRow)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(300));

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
                                               (null == details.SpectrumReal) ? "Rec" : "Re-Rec"), GUILayout.Height(45));
            }

            bool rectAvailable = false;
            Rect rect = new Rect();
            Event e = Event.current;
            if (details == GetWord(WORD_PUSH_TO_TALK))
            {
                if (null != e)
                {
                    rect = new Rect(400, 250, 200, 200);
                    rectAvailable = true;
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.green;
                    if (GUI.Button(rect, "Push To Talk\nHold Speak 1 Command"))
                    {
                    }
                    GUI.backgroundColor = oldColor;
                }
            }
            else
            {
                if (null != e)
                {
                    rect = GUILayoutUtility.GetLastRect();
                    rectAvailable = true;
                }
            }

            if (rectAvailable)
            {
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

            if (showRow)
            {
                GUILayout.Label(string.Format("{0:F2}", details.Score));
                //GUILayout.Label(string.Format("{0}", details.GetMinScore(DateTime.Now - TimeSpan.FromSeconds(1))));
                GUILayout.EndHorizontal();

                if (details != GetWord(WORD_NOISE))
                {
                    GUI.enabled = true;
                }

                GUILayout.Space(10);
            }

            if (details == GetWord(WORD_NOISE))
            {
                GUILayout.Label("Next: Record the voice command");
            }
        }

        GUI.backgroundColor = backgroundColor;
    }
}