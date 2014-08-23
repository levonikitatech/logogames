using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example9 : Example4
{
    public Animator m_animator = null;

    enum Commands
    {
        Noise,
        Go,
        Action,
        Back,
        Charge,
        Dance,
        Elbow,
        Fall,
        Guitar,
        Head,
        Misc,
        Punch,
        Run,
    }

    Commands m_command = Commands.Noise;

    // Noise + Go
    List<WordDetails> m_set1 = new List<WordDetails>();

    // Noise + Groups
    List<WordDetails> m_set2 = new List<WordDetails>();

    // Noise + Actions
    List<WordDetails> m_set3 = new List<WordDetails>();

    // Noise + Misc
    List<WordDetails> m_set4 = new List<WordDetails>();

    enum Modes
    {
        Set1,
        Set2,
        Set3,
        Set4,
    }

    private Modes m_mode = Modes.Set1;

    private DateTime m_modeTimer = DateTime.MinValue;

    private bool m_recordingSample = false;

    private bool m_wordsChanged = false;

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

        Dictionary<Commands, WordDetails> words = new Dictionary<Commands, WordDetails>();
        
        // prepopulate words
        foreach (string val in Enum.GetNames(typeof(Commands)))
        {
            //Debug.Log(val);
            Commands command = (Commands) Enum.Parse(typeof (Commands), val);
            WordDetails details = new WordDetails() { Label = val };
            words[command] = details;
            try
            {
                string path = string.Format("Assets/{0}_{1}.profile", GetType().Name, val);
                if (File.Exists(path))
                {
                    using (
                        FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        )
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            AudioWordDetection.LoadWord(br, details);
                            //Debug.Log(string.Format("Loaded profile: {0}", path));
                            details.Label = val;
                        }
                    }
                }
                else
                {
                    Debug.Log(string.Format("Profile not available for: {0}", path));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to load word: {0}", ex));
            }
        }

        // set 1
        m_set1.Add(words[Commands.Noise]);
        m_set1.Add(words[Commands.Go]);

        // set 2
        m_set2.Add(words[Commands.Noise]);
        m_set2.Add(words[Commands.Back]);
        m_set2.Add(words[Commands.Action]);
        m_set2.Add(words[Commands.Misc]);

        // set 3
        m_set3.Add(words[Commands.Noise]);
        m_set3.Add(words[Commands.Back]);
        m_set3.Add(words[Commands.Charge]);
        m_set3.Add(words[Commands.Elbow]);
        m_set3.Add(words[Commands.Head]);
        m_set3.Add(words[Commands.Punch]);

        // set 4
        m_set4.Add(words[Commands.Noise]);
        m_set4.Add(words[Commands.Back]);
        m_set4.Add(words[Commands.Dance]);
        m_set4.Add(words[Commands.Fall]);
        m_set4.Add(words[Commands.Guitar]);
        m_set4.Add(words[Commands.Run]);

        AudioWordDetection.Words = m_set1;
        m_mode = Modes.Set1;

        //subscribe detection event
        AudioWordDetection.WordDetectedEvent += WordDetectedHandler;
    }

    void SaveProfile(WordDetails details)
    {
        string path = string.Format("Assets/{0}_{1}.profile", GetType().Name, details.Label);
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                using (
                    FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write,
                        FileShare.ReadWrite)
                    )
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        AudioWordDetection.SaveWord(bw, details);
                        //Debug.Log(string.Format("Save profile: {0}", details.Label));
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("Failed to save profile: {0}", details.Label));
            }
        }
    }

    /// <summary>
    /// Handle word detected event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void WordDetectedHandler(object sender, WordDetection.WordEventArgs args)
    {
        // skip detection while recording
        if (m_recordingSample)
        {
            return;
        }

        if (null == args.Details ||
            string.IsNullOrEmpty(args.Details.Label))
        {
            m_command = Commands.Noise;
            return;
        }

        if (m_modeTimer > DateTime.Now)
        {
            m_command = Commands.Noise;
            return;
        }

        //Debug.Log(args.Details.Label);
        switch (m_mode)
        {
            case Modes.Set1:
                if (args.Details.Label == Commands.Go.ToString())
                {
                    AudioWordDetection.Words = m_set2;
                    m_mode = Modes.Set2;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(1000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                break;
            case Modes.Set2:
                if (args.Details.Label == Commands.Back.ToString())
                {
                    AudioWordDetection.Words = m_set1;
                    m_mode = Modes.Set1;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(1000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                else if (args.Details.Label == Commands.Action.ToString())
                {
                    AudioWordDetection.Words = m_set3;
                    m_mode = Modes.Set3;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(2000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                else if (args.Details.Label == Commands.Misc.ToString())
                {
                    AudioWordDetection.Words = m_set4;
                    m_mode = Modes.Set4;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(2000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                break;
            case Modes.Set3:
            case Modes.Set4:
                if (args.Details.Label == Commands.Back.ToString())
                {
                    AudioWordDetection.Words = m_set2;
                    m_mode = Modes.Set2;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(1000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                else if (args.Details.Label != Commands.Noise.ToString())
                {
                    AudioWordDetection.Words = m_set1;
                    m_mode = Modes.Set1;
                    m_modeTimer = DateTime.Now + TimeSpan.FromMilliseconds(2000);
                    Mic.ClearData();
                    m_wordsChanged = true;
                    m_command = Commands.Noise;
                }
                break;
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
            if (m_wordsChanged)
            {
                m_wordsChanged = false;
                GUIUtility.ExitGUI();
            }

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
                    m_recordingSample = true;
                }
                if (m_buttonIndex == wordIndex)
                {
                    bool buttonUp = Input.GetMouseButtonUp(0);
                    if (m_timerStart > DateTime.Now &&
                        !buttonUp)
                    {
                        //Debug.Log("Button still pressed");
                        m_recordingSample = true;
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        m_timerStart < DateTime.Now)
                    {
                        //Debug.Log("Button timed out");
                        SetupWordProfile(false);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                        m_recordingSample = false;
                        Mic.ClearData();
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        buttonUp &&
                        m_buttonIndex != -1)
                    {
                        //Debug.Log("Button is no longer pressed");
                        SetupWordProfile(true);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                        m_recordingSample = false;
                        Mic.ClearData();
                        SaveProfile(details);
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