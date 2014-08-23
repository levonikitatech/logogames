using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example10 : Example4
{
    public MeshRenderer m_Noise = null;
    public MeshRenderer m_Aw = null;
    public MeshRenderer m_Oo = null;
    public MeshRenderer m_Ee = null;
    public MeshRenderer m_Sh = null;
    public MeshRenderer m_P = null;
    public MeshRenderer m_F = null;
    public MeshRenderer m_Th = null;
    public MeshRenderer m_I = null;

    private DateTime m_timerClearMic = DateTime.MinValue;

    enum Commands
    {
        Noise,
        Aw,
        Oo,
        Ee,
        ShChJ,
        PBM,
        FV,
        Th,
        I,
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

        Dictionary<Commands, WordDetails> words = new Dictionary<Commands, WordDetails>();

        // prepopulate words
        foreach (string val in Enum.GetNames(typeof(Commands)))
        {
            //Debug.Log(val);
            Commands command = (Commands)Enum.Parse(typeof(Commands), val);
            WordDetails details = new WordDetails() { Label = val };
            AudioWordDetection.Words.Add(details);
            words[command] = details;
#if !UNITY_WEBPLAYER
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
#endif
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
        }
        else
        {
            m_command = (Commands) Enum.Parse(typeof (Commands), args.Details.Label, false);

            if (m_command != Commands.Noise)
            {
                m_timerClearMic = DateTime.Now + TimeSpan.FromMilliseconds(200);
            }
        }
    }

    void SaveProfile(WordDetails details)
    {
#if !UNITY_WEBPLAYER
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
#endif
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

        GUILayout.Label(string.Empty);

        GUILayout.Label(m_command.ToString());

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

            if (GUILayout.Button("Play", GUILayout.Width(100), GUILayout.Height(45)))
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
                    (null == details.SpectrumReal) ? "not set" : "set"), GUILayout.Width(100), GUILayout.Height(45));

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

    void Update()
    {
        m_Noise.enabled = m_command == Commands.Noise;
        m_Aw.enabled = m_command == Commands.Aw;
        m_Oo.enabled = m_command == Commands.Oo;
        m_Ee.enabled = m_command == Commands.Ee;
        m_Sh.enabled = m_command == Commands.ShChJ;
        m_P.enabled = m_command == Commands.PBM;
        m_F.enabled = m_command == Commands.FV;
        m_Th.enabled = m_command == Commands.Th;
        m_I.enabled = m_command == Commands.I;

        if (m_timerClearMic != DateTime.MinValue)
        {
            if (m_timerClearMic < DateTime.Now)
            {
                m_timerClearMic = DateTime.MinValue;
                Mic.ClearData();
            }
        }
    }
}