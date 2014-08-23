using System;
using System.Collections.Generic;

/// <summary>
/// Audio word details
/// </summary>
public class WordDetails
{
    public float[] Wave = null;
    public float[] SpectrumReal = null;
    public float[] SpectrumImag = null;
    public string Label = string.Empty;
    public UnityEngine.AudioClip Audio = null;
    
    public float Score = 0f;

    public Dictionary<float, DateTime> Scores = new Dictionary<float, DateTime>();
    DateTime m_timerScore = DateTime.MinValue;
    public void AddMinScore(float score)
    {
        // record the score every N ms
        if (m_timerScore < DateTime.Now)
        {
            m_timerScore = DateTime.Now + TimeSpan.FromMilliseconds(100);
            Scores[score] = DateTime.Now;
        }
    }
    public float GetMinScore(DateTime cutOff)
    {
        float min = -1f;
        Dictionary<float,DateTime>.KeyCollection.Enumerator enumator = Scores.Keys.GetEnumerator();
        int index = 0;
        if (enumator.MoveNext())
        {
            float score = enumator.Current;
            if (Scores[score] < cutOff)
            {
                Scores.Remove(score);
            }
            else
            {
                if (index == 0)
                {
                    min = score;
                }
                else
                {
                    min = (score < min) ? score : min;
                }
                ++index;
            }
        }
        if (min == -1f)
        {
            return Score;
        }
        else
        {
            return min;
        }
    }
}