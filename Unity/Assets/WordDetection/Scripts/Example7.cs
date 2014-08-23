using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example7 : Example4
{
    public Animator m_animator = null;

    enum Commands
    {
        Noise,
        Charge,
        Dance,
        Elbow,
        Fall,
        Guitar,
        Head,
        Punch,
        Run,
    }

    Commands m_command = Commands.Noise;

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
        foreach (string val in Enum.GetNames(typeof(Commands)))
        {
            //Debug.Log(val);
            AudioWordDetection.Words.Add(new WordDetails() { Label = val });
        }

        //subscribe detection event
        AudioWordDetection.WordDetectedEvent += WordDetectedHandler;
    }

    /// <summary>
    /// Handle word detected event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void WordDetectedHandler(object sender, WordDetection.WordEventArgs args)
    {
        if (null == args.Details ||
            string.IsNullOrEmpty(args.Details.Label))
        {
            m_command = Commands.Noise;
            return;
        }

        m_command = (Commands)Enum.Parse(typeof(Commands), args.Details.Label, false);

        PlayAnimation(GetStateName(m_command));
    }

    string GetStateName(Commands command)
    {
        switch (command)
        {
            case Commands.Charge:
                return "Sophia@pointing onward charge - loop";
            case Commands.Dance:
                return "Sophia@the popular k-pop dance - loop";
            case Commands.Elbow:
                return "Sophia@male elbow punch - loop";
            case Commands.Fall:
                return "Sophia@floating in air flailing arms - loop";
            case Commands.Guitar:
                return "Sophia@playing a guitar - loop";
            case Commands.Head:
                return "Sophia@short hook punch to the head - loop";
            case Commands.Punch:
                return "Sophia@a hook punch - loop";
            case Commands.Run:
                return "Sophia@female run forward - loop";
            case Commands.Noise:
            default:
                return "Sophia@standard idle - loop";
        }
    }

    int GetStateHash(Commands command)
    {
        return Animator.StringToHash(GetStateName(command));
    }

    int GetCurrentStateHash()
    {
        int layer = 0;
        if (layer < m_animator.layerCount)
        {
            AnimatorStateInfo currentState = m_animator.GetCurrentAnimatorStateInfo(layer);
            return currentState.nameHash;
        }
        return 0;
    }

    void PlayAnimation(string animationState)
    {
        if (m_animator &&
            m_command != Commands.Noise)
        {
            int currentHash = GetCurrentStateHash();
            if (currentHash != GetStateHash(Commands.Noise))
            {
                int stateHash = Animator.StringToHash(animationState);
                int layer = 0;
                if (layer < m_animator.layerCount)
                {
                    m_animator.Play(stateHash, layer, 0f);
                    //Debug.Log(string.Format("Play: {0}", animationState));
                }
            }
        }
    }

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

            GUILayout.BeginHorizontal(GUILayout.MinWidth(600));
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
            //GUILayout.Label(details.Score.ToString());
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