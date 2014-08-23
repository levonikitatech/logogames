using UnityEditor;
using UnityEngine;

public class NoiseRemovalDialog : EditorWindow
{
    /// <summary>
    /// Open an instance of the panel
    /// </summary>
    /// <returns></returns>
    public static NoiseRemovalDialog GetPanel()
    {
        NoiseRemovalDialog window = GetWindow<NoiseRemovalDialog>("Noise Removal");
        window.position = new Rect(300, 300, 500, 500);
        return window;
    }

    /// <summary>
    /// Get Toolbox Window
    /// </summary>
    //[MenuItem("Window/Open Noise Removal")]
    private static void MenuGetPanel()
    {
        GetPanel();
    }

    void Update()
    {
        Repaint();
    }

    void DisplayControl(string label, string format, ref float field, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(160));
        string text = string.Format(format, field);
        string t = GUILayout.TextField(text, GUILayout.Width(50));
        float f = GUILayout.HorizontalSlider(field, min, max, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        if (!text.Equals(t))
        {
            if (float.TryParse(t, out f))
            {
                field = f;
            }
        }
        else if (field != f)
        {
            field = f;
        }

        field = Mathf.Max(field, min);
        field = Mathf.Min(field, max);
    }

    void OnGUI()
    {
        GUILayout.Label("Noise Removal by Dominic Mazzoni");
        GUILayout.Space(10);

        GUILayout.Label("Step 1");
        GUILayout.Label("Select a few seconds of just noise so the");
        GUILayout.Label("algorithm knows what to filter out, then");
        GUILayout.Label("click Get Noise Profile:");
        if (GUILayout.Button("Get Noise Profile", GUILayout.MinHeight(40)))
        {
            
        }
        GUILayout.Space(10);

        GUILayout.Label("Step 2");
        GUILayout.Label("Select all of the audio you want filtered,");
        GUILayout.Label("choose how much noise you want filtered");
        GUILayout.Label("out, and then click 'OK' to remove noise.");
        GUILayout.Space(10);

        if (GUILayout.Button("SetDefaults", GUILayout.MinHeight(40)))
        {
            m_noiseReduction = 24;
            m_sensitivity = 0f;
            m_frequencySmoothing = 150;
            m_attackDecayTime = 0.15f;
        }

        DisplayControl("Noise reduction (dB):", "{0:F0}", ref m_noiseReduction, 0, 48);
        DisplayControl("Sensitivity (dB):", "{0:F2}", ref m_sensitivity, -20, 20);
        DisplayControl("Frequency smoothing (Hz):", "{0:F0}", ref m_frequencySmoothing, 0, 1000);
        DisplayControl("Attack/decay time (secs):", "{0:F2}", ref m_attackDecayTime, 0, 1);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Noise:", GUILayout.Width(150));
        m_noiseMode = (NoiseModes)EditorGUILayout.EnumPopup(m_noiseMode);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Preview"))
        {
        }
        GUILayout.Label(string.Empty, GUILayout.Width(20));
        if (GUILayout.Button("OK"))
        {
        }
        if (GUILayout.Button("Cancel"))
        {
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
    }

    private float m_noiseReduction = 24;
    private float m_sensitivity = 0f;
    private float m_frequencySmoothing = 150;
    private float m_attackDecayTime = 0.15f;

    public enum NoiseModes
    {
        Remove,
        Isolate,
    }

    private NoiseModes m_noiseMode = NoiseModes.Remove;



    float mSensitivity;
    float mGain;
    float mFreq;
    float mTime;

    bool mbLeaveNoise;




    private enum UIComponent
    {
        ID_BUTTON_GETPROFILE = 10001,
        ID_BUTTON_LEAVENOISE,
        ID_RADIOBUTTON_KEEPSIGNAL,
        ID_RADIOBUTTON_KEEPNOISE,
        ID_SENSITIVITY_SLIDER,
        ID_GAIN_SLIDER,
        ID_FREQ_SLIDER,
        ID_TIME_SLIDER,
        ID_SENSITIVITY_TEXT,
        ID_GAIN_TEXT,
        ID_FREQ_TEXT,
        ID_TIME_TEXT,
    };

    private const int SENSITIVITY_MIN = 0; // Corresponds to -20 dB 
    private const int SENSITIVITY_MAX = 4000; // Corresponds to 20 dB

    private const int GAIN_MIN = 0;
    private const int GAIN_MAX = 48; // Corresponds to -48 dB

    private const int FREQ_MIN = 0;
    private const int FREQ_MAX = 100; // Corresponds to 1000 Hz

    private const int TIME_MIN = 0;
    private const int TIME_MAX = 100;

    // Corresponds to 1.000 seconds

    //BEGIN_EVENT_TABLE(NoiseRemovalDialog,wxDialog)
    //   EVT_BUTTON(wxID_OK, NoiseRemovalDialog::OnRemoveNoise)
    //   EVT_BUTTON(wxID_CANCEL, NoiseRemovalDialog::OnCancel)
    //   EVT_BUTTON(ID_EFFECT_PREVIEW, NoiseRemovalDialog::OnPreview)
    //   EVT_BUTTON(ID_BUTTON_GETPROFILE, NoiseRemovalDialog::OnGetProfile)
    //   EVT_RADIOBUTTON(ID_RADIOBUTTON_KEEPNOISE, NoiseRemovalDialog::OnKeepNoise)
    //   EVT_RADIOBUTTON(ID_RADIOBUTTON_KEEPSIGNAL, NoiseRemovalDialog::OnKeepNoise)
    //   EVT_SLIDER(ID_SENSITIVITY_SLIDER, NoiseRemovalDialog::OnSensitivitySlider)
    //   EVT_SLIDER(ID_GAIN_SLIDER, NoiseRemovalDialog::OnGainSlider)
    //   EVT_SLIDER(ID_FREQ_SLIDER, NoiseRemovalDialog::OnFreqSlider)
    //   EVT_SLIDER(ID_TIME_SLIDER, NoiseRemovalDialog::OnTimeSlider)
    //   EVT_TEXT(ID_SENSITIVITY_TEXT, NoiseRemovalDialog::OnSensitivityText)
    //   EVT_TEXT(ID_GAIN_TEXT, NoiseRemovalDialog::OnGainText)
    //   EVT_TEXT(ID_FREQ_TEXT, NoiseRemovalDialog::OnFreqText)
    //   EVT_TEXT(ID_TIME_TEXT, NoiseRemovalDialog::OnTimeText)
    //END_EVENT_TABLE()

    /*

    public NoiseRemovalDialog(EffectNoiseRemoval effect)
    {
        strint title = "Noise Removal";
        m_pEffect = effect;

        // NULL out the control members until the controls are created.
        m_pButton_GetProfile = NULL;
        m_pButton_Preview = NULL;
        m_pButton_RemoveNoise = NULL;

        Init();

        m_pButton_Preview =
            (wxButton*) wxWindow::FindWindowById(ID_EFFECT_PREVIEW, this);
        m_pButton_RemoveNoise =
            (wxButton*) wxWindow::FindWindowById(wxID_OK, this);
    }

    void OnGetProfile( wxCommandEvent &event )
    {
       EndModal(1);
    }

    void OnKeepNoise( wxCommandEvent &event )
    {
       mbLeaveNoise = mKeepNoise->GetValue();
    }

    void OnPreview(wxCommandEvent &event)
    {
       // Save & restore parameters around Preview, because we didn't do OK.
       bool oldDoProfile = m_pEffect->mDoProfile;
       bool oldLeaveNoise = m_pEffect->mbLeaveNoise;
       double oldSensitivity = m_pEffect->mSensitivity;
       double oldGain = m_pEffect->mNoiseGain;
       double oldFreq = m_pEffect->mFreqSmoothingHz;
       double oldTime = m_pEffect->mAttackDecayTime;

       TransferDataFromWindow();

       m_pEffect->mDoProfile = false;
       m_pEffect->mbLeaveNoise = mbLeaveNoise;
       m_pEffect->mSensitivity = mSensitivity;
       m_pEffect->mNoiseGain = -mGain;
       m_pEffect->mFreqSmoothingHz =  mFreq;
       m_pEffect->mAttackDecayTime =  mTime;
   
       m_pEffect->Preview();
   
       m_pEffect->mSensitivity = oldSensitivity;
       m_pEffect->mNoiseGain = oldGain;
       m_pEffect->mFreqSmoothingHz =  oldFreq;
       m_pEffect->mAttackDecayTime =  oldTime;
       m_pEffect->mbLeaveNoise = oldLeaveNoise;
       m_pEffect->mDoProfile = oldDoProfile;
    }

    void OnRemoveNoise( wxCommandEvent &event )
    {
       mbLeaveNoise = mKeepNoise->GetValue();
       EndModal(2);
    }

    void OnCancel(wxCommandEvent &event)
    {
       EndModal(0);
    }

    void PopulateOrExchange(ShuttleGui & S)
    {
       wxString step1Label;
       wxString step1Prompt;
       wxString step2Label;
       wxString step2Prompt;

       S.StartHorizontalLay(wxCENTER, false);
       {
          S.AddTitle(_("Noise Removal by Dominic Mazzoni"));
       }
       S.EndHorizontalLay();
   
       S.StartStatic(step1Label);
       {
          S.AddVariableText(step1Prompt);
          m_pButton_GetProfile = S.Id(ID_BUTTON_GETPROFILE).
             AddButton(_("&Get Noise Profile"));
       }
       S.EndStatic();

       S.StartStatic(step2Label);
       {
          S.AddVariableText(step2Prompt);

          S.StartMultiColumn(3, wxEXPAND);
          S.SetStretchyCol(2);
          {
             wxTextValidator vld(wxFILTER_NUMERIC);
             mGainT = S.Id(ID_GAIN_TEXT).AddTextBox(_("Noise re&duction (dB):"),
                                                    wxT(""),
                                                    0);
             S.SetStyle(wxSL_HORIZONTAL);
             mGainT->SetValidator(vld);
             mGainS = S.Id(ID_GAIN_SLIDER).AddSlider(wxT(""), 0, GAIN_MAX);
             mGainS->SetName(_("Noise reduction"));
             mGainS->SetRange(GAIN_MIN, GAIN_MAX);
             mGainS->SetSizeHints(150, -1);

             mSensitivityT = S.Id(ID_SENSITIVITY_TEXT).AddTextBox(_("&Sensitivity (dB):"),
                                                    wxT(""),
                                                    0);
             S.SetStyle(wxSL_HORIZONTAL);
             mSensitivityT->SetValidator(vld);
             mSensitivityS = S.Id(ID_SENSITIVITY_SLIDER).AddSlider(wxT(""), 0, SENSITIVITY_MAX);
             mSensitivityS->SetName(_("Sensitivity"));
             mSensitivityS->SetRange(SENSITIVITY_MIN, SENSITIVITY_MAX);
             mSensitivityS->SetSizeHints(150, -1);

             mFreqT = S.Id(ID_FREQ_TEXT).AddTextBox(_("Fr&equency smoothing (Hz):"),
                                                    wxT(""),
                                                    0);
             S.SetStyle(wxSL_HORIZONTAL);
             mFreqT->SetValidator(vld);
             mFreqS = S.Id(ID_FREQ_SLIDER).AddSlider(wxT(""), 0, FREQ_MAX);
             mFreqS->SetName(_("Frequency smoothing"));
             mFreqS->SetRange(FREQ_MIN, FREQ_MAX);
             mFreqS->SetSizeHints(150, -1);

             mTimeT = S.Id(ID_TIME_TEXT).AddTextBox(_("Attac&k/decay time (secs):"),
                                                    wxT(""),
                                                    0);
             S.SetStyle(wxSL_HORIZONTAL);
             mTimeT->SetValidator(vld);
             mTimeS = S.Id(ID_TIME_SLIDER).AddSlider(wxT(""), 0, TIME_MAX);
             mTimeS->SetName(_("Attack/decay time"));
             mTimeS->SetRange(TIME_MIN, TIME_MAX);
             mTimeS->SetSizeHints(150, -1);

             S.AddPrompt(_("Noise:"));
             mKeepSignal = S.Id(ID_RADIOBUTTON_KEEPSIGNAL)
                   .AddRadioButton(_("Re&move"));
             mKeepNoise = S.Id(ID_RADIOBUTTON_KEEPNOISE)
                   .AddRadioButtonToGroup(_("&Isolate"));
          }
          S.EndMultiColumn();
       }
       S.EndStatic();
    }

    bool TransferDataToWindow()
    {
       mSensitivityT->SetValue(wxString::Format(wxT("%.2f"), mSensitivity));
       mGainT->SetValue(wxString::Format(wxT("%d"), (int)mGain));
       mFreqT->SetValue(wxString::Format(wxT("%d"), (int)mFreq));
       mTimeT->SetValue(wxString::Format(wxT("%.2f"), mTime));
       mKeepNoise->SetValue(mbLeaveNoise);
       mKeepSignal->SetValue(!mbLeaveNoise);

       mSensitivityS->SetValue(TrapLong(mSensitivity*100.0 + (SENSITIVITY_MAX-SENSITIVITY_MIN+1)/2.0, SENSITIVITY_MIN, SENSITIVITY_MAX));
       mGainS->SetValue(TrapLong(mGain, GAIN_MIN, GAIN_MAX));
       mFreqS->SetValue(TrapLong(mFreq / 10, FREQ_MIN, FREQ_MAX));
       mTimeS->SetValue(TrapLong(mTime * TIME_MAX + 0.5, TIME_MIN, TIME_MAX));

       return true;
    }

    bool TransferDataFromWindow()
    {
       // Nothing to do here
       return true;
    }

    void OnSensitivityText(string val)
    {
       mSensitivityT->GetValue().ToDouble(&mSensitivity);
       mSensitivityS->SetValue(TrapLong(mSensitivity*100.0f + (SENSITIVITY_MAX-SENSITIVITY_MIN+1)/2.0f, SENSITIVITY_MIN, SENSITIVITY_MAX));
    }

    void OnGainText(string val)
    {
       mGainT->GetValue().ToDouble(&mGain);
       mGainS->SetValue(TrapLong(mGain, GAIN_MIN, GAIN_MAX));
    }

    void OnFreqText(string val)
    {
       mFreqT->GetValue().ToDouble(&mFreq);
       mFreqS->SetValue(TrapLong(mFreq / 10, FREQ_MIN, FREQ_MAX));
    }

    void OnTimeText(string val)
    {
       mTimeT->GetValue().ToDouble(&mTime);
       mTimeS->SetValue(TrapLong(mTime * TIME_MAX + 0.5f, TIME_MIN, TIME_MAX));
    }

    void OnSensitivitySlider(float val)
    {
       mSensitivity = mSensitivityS->GetValue()/100.0f - 20.0f;
       mSensitivityT->SetValue(wxString::Format(wxT("%.2f"), mSensitivity));
    }

    void OnGainSlider(float val)
    {
       mGain = mGainS->GetValue();
       mGainT->SetValue(wxString::Format(wxT("%d"), (int)mGain));
    }

    void OnFreqSlider(float val)
    {
       mFreq = mFreqS->GetValue() * 10;
       mFreqT->SetValue(wxString::Format(wxT("%d"), (int)mFreq));
    }

    void OnTimeSlider(float val)
    {
       mTime = mTimeS->GetValue() / (TIME_MAX*1.0f);
       mTimeT->SetValue(wxString::Format(wxT("%.2f"), mTime));
    }

    */
}