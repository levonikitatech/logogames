//#define USE_WORD_MIN_SCORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// Handle word detection
/// </summary>
public class WordDetection : MonoBehaviour
{
    /// <summary>
    /// Reference to the spectrum microphone
    /// </summary>
    public SpectrumMicrophone Mic = null;

    /// <summary>
    /// List of words to detect
    /// </summary>
    public List<WordDetails> Words = new List<WordDetails>();

    /// <summary>
    /// List of words to ignore
    /// </summary>
    public List<string> WordsToIgnore = new List<string>();

    /// <summary>
    /// The closest matched word index
    /// </summary>
    public int ClosestIndex = 0;

    /// <summary>
    /// Threshold for word detection
    /// </summary>
    public int Threshold = 60;

    /// <summary>
    /// Args for a detected word event
    /// </summary>
    public class WordEventArgs : EventArgs
    {
        /// <summary>
        /// The details about the detected word
        /// </summary>
        public WordDetails Details = null;
    }

    /// <summary>
    /// Word detection event
    /// </summary>
    public EventHandler<WordEventArgs> WordDetectedEvent = null;

    /// <summary>
    /// The zero index is noise
    /// </summary>
    private const int FIRST_WORD_INDEX = 0;

    /// <summary>
    /// The normalized wave
    /// </summary>
    private float[] m_wave = null;

    /// <summary>
    /// The spectrum data, real
    /// </summary>
    private float[] m_spectrumReal = null;

    /// <summary>
    /// The spectrum data, imaginary
    /// </summary>
    private float[] m_spectrumImag = null;

    /// <summary>
    /// Flag to normalize wave samples
    /// </summary>
    public bool NormalizeWave = false;

    /// <summary>
    /// Flag to enable push to talk
    /// </summary>
    public bool UsePushToTalk = false;

    public bool LoadProfiles(FileInfo fi)
    {
        try
        {
            using (FileStream fs = File.Open(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    LoadProfiles(br);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to load profiles exception={0}", ex));
            return false;
        }
    }

    public bool LoadProfilesPrefs(string key)
    {
        try
        {
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.LogError("Player prefs missing key");
                return false;
            }
            
            Words.Clear();

            int wordCount = PlayerPrefs.GetInt(key);

            for (int wordIndex = 0; wordIndex < wordCount; ++wordIndex)
            {
                string wordKey = string.Format("{0}_{1}", key, wordIndex);
                if (PlayerPrefs.HasKey(wordKey))
                {
                    string base64 = PlayerPrefs.GetString(wordKey);
                    byte[] buffer = System.Convert.FromBase64String(base64);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            WordDetails details = new WordDetails();
                            Words.Add(details);
                            LoadWord(br, details);

                            Debug.Log(string.Format("Key={0} size={1} label={2}", wordKey, base64.Length, details.Label));
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to load profiles exception={0}", ex));
            return false;
        }
    }

    public void LoadProfiles(BinaryReader br)
    {
        Words.Clear();

        int wordCount = br.ReadInt32();
        for (int wordIndex = 0; wordIndex < wordCount; ++wordIndex)
        {
            WordDetails details = new WordDetails();
            Words.Add(details);

            LoadWord(br, details);
        }
    }

    public void LoadWord(BinaryReader br, WordDetails details)
    {
        details.Label = br.ReadString();
        int channels = br.ReadInt32();
        int frequency = br.ReadInt32();
        int size = br.ReadInt32();
        details.Wave = new float[size];
        for (int index = 0; index < size; ++index)
        {
            details.Wave[index] = br.ReadSingle();
        }
        int halfSize = size / 2;
        details.SpectrumImag = new float[halfSize];
        details.SpectrumReal = new float[halfSize];
        Mic.GetSpectrumData(details.Wave, details.SpectrumReal, details.SpectrumImag, FFTWindow.Rectangular);
        if (null == details.Audio)
        {
            details.Audio = AudioClip.Create(Guid.NewGuid().ToString(), size, channels, frequency, false, false);
        }
        if (null != details.Audio && null != details.Wave)
        {
            details.Audio.SetData(details.Wave, 0);
        }
    }

    public void SaveProfiles(FileInfo fi)
    {
        try
        {
            using (FileStream fs = File.Open(fi.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    SaveProfiles(bw);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to save profiles exception={0}", ex));
        }
    }

    public void SaveProfilesPrefs(string key)
    {
        try
        {
            int count = 0;
            for (int wordIndex = 0; wordIndex < Words.Count; ++wordIndex)
            {
                WordDetails details = Words[wordIndex];
                if (null == details)
                {
                    continue;
                }
                ++count;
            }
            PlayerPrefs.SetInt(key, count);

            Debug.Log(string.Format("Saving profiles count={0}", count));

            int index = 0;
            for (int wordIndex = 0; wordIndex < Words.Count; ++wordIndex)
            {
                WordDetails details = Words[wordIndex];
                if (null == details)
                {
                    continue;
                }

                string wordKey = string.Format("{0}_{1}", key, index);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        SaveWord(bw, details);
                        bw.Flush();

                        ms.Position = 0;
                        byte[] buffer = ms.GetBuffer();
                        string base64 = System.Convert.ToBase64String(buffer);
                        PlayerPrefs.SetString(wordKey, base64);

                        Debug.Log(string.Format("Key={0} size={1} label={2}", wordKey, base64.Length, details.Label));
                    }
                }

                ++index;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Failed to save profiles exception={0}", ex));
        }
    }

    public void SaveProfiles(BinaryWriter bw)
    {
        int count = 0;
        for (int index = 0; index < Words.Count; ++index)
        {
            WordDetails details = Words[index];
            if (null == details)
            {
                continue;
            }
            ++count;
        }
        bw.Write(count);

        for (int index = 0; index < Words.Count; ++index)
        {
            WordDetails details = Words[index];
            if (null == details)
            {
                continue;
            }
            SaveWord(bw, details);
        }
    }

    public void SaveWord(BinaryWriter bw, WordDetails details)
    {
        bw.Write(details.Label);
        bw.Write(details.Audio.channels);
        bw.Write(details.Audio.frequency);
        if (null == details.Wave)
        {
            bw.Write(0);
        }
        else
        {
            bw.Write(details.Wave.Length);
            foreach (float f in details.Wave)
            {
                bw.Write(f);
            }
        }
    }

    /// <summary>
    /// Update event
    /// </summary>
    private void OnGUI()
    {
        if (null == Mic ||
            string.IsNullOrEmpty(Mic.DeviceName))
        {
            return;
        }

        float[] wave = Mic.GetLastData();
        if (null != wave)
        {
            //allocate for the wave copy
            int size = wave.Length;
            int halfSize = size/2;
            if (null == m_wave ||
                m_wave.Length != size)
            {
                m_wave = new float[size];
            }

            //trim the wave
            int position = Mic.GetPosition();

            //shift array
            for (int index = 0, i = position; index < size; ++index, i = (i + 1)%size)
            {
                m_wave[index] = wave[i];
            }

            if (NormalizeWave)
            {
                //normalize the array
                Mic.NormalizeWave(m_wave);
            }

            if (null == m_spectrumReal ||
                m_spectrumReal.Length != halfSize)
            {
                m_spectrumReal = new float[halfSize];
            }

            if (null == m_spectrumImag ||
                m_spectrumImag.Length != halfSize)
            {
                m_spectrumImag = new float[halfSize];
            }

            //get the spectrum for the normalized wave
            Mic.GetSpectrumData(m_wave, m_spectrumReal, m_spectrumImag, FFTWindow.Rectangular);

            if (!UsePushToTalk)
            {
                DetectWords(wave);
            }
        }
    }

    public void DetectWords(float[] wave)
    {
        float minScore = 0f;

        int closestIndex = 0;
        WordDetails closestWord = null;

        int size = wave.Length;
        int halfSize = size / 2;
        for (int wordIndex = FIRST_WORD_INDEX; wordIndex < Words.Count; ++wordIndex)
        {
            WordDetails details = Words[wordIndex];

            if (null == details)
            {
                continue;
            }

            if (WordsToIgnore.Contains(details.Label))
            {
                continue;
            }

            float[] spectrum = details.SpectrumReal;
            if (null == spectrum)
            {
                //Debug.LogError(string.Format("Word profile not set: {0}", details.Label));
                details.Score = -1;
                continue;
            }
            if (null == m_spectrumReal)
            {
                details.Score = -1;
                continue;
            }
            if (spectrum.Length != halfSize ||
                m_spectrumReal.Length != halfSize)
            {
                details.Score = -1;
                continue;
            }

            float score = 0;
            for (int index = 0; index < halfSize;)
            {
                float sumSpectrum = 0f;
                float sumProfile = 0f;
                int nextIndex = index + Threshold;
                for (; index < nextIndex && index < halfSize; ++index)
                {
                    sumSpectrum += Mathf.Abs(m_spectrumReal[index]);
                    sumProfile += Mathf.Abs(spectrum[index]);
                }
                sumProfile = sumProfile/(float) Threshold;
                sumSpectrum = sumSpectrum/(float) Threshold;
                float val = Mathf.Abs(sumSpectrum - sumProfile);
                score += Mathf.Abs(val);
            }

            details.Score = score;

#if USE_WORD_MIN_SCORE
                details.AddMinScore(score);
                score = details.GetMinScore(DateTime.Now - TimeSpan.FromSeconds(1));
#endif

            if (wordIndex == FIRST_WORD_INDEX)
            {
                closestIndex = wordIndex;
                minScore = score;
                closestWord = details;
            }
            else if (score < minScore)
            {
                closestIndex = wordIndex;
                minScore = score;
                closestWord = details;
            }
        }

        if (ClosestIndex != closestIndex)
        {
            ClosestIndex = closestIndex;
            if (null != WordDetectedEvent)
            {
                WordEventArgs args = new WordEventArgs();
                args.Details = closestWord;
                //Debug.Log(args.Details.Label);
                WordDetectedEvent.Invoke(this, args);
            }
        }
    }
}