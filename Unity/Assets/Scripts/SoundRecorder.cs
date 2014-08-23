using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SoundRecorder : MonoBehaviour {

	public AudioClip LastRecorded;
	public ICollection<AudioClip> LastRecords;

	string selectedDevice;

	int minFreq;
	int maxFreq;

	float startTime;

	public int maxRecordTime = 10; // secs

	public void Start ()
	{
		LastRecords = new List<AudioClip> ();

		selectedDevice = Microphone.devices [0].ToString ();
		
		Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);//Gets the frequency of the device
		if ((minFreq + maxFreq) == 0)//These 2 lines of code are mainly for windows computers
			maxFreq = 44100;

		startTime = 0;
	}

	public void Update()
	{
		if (startTime >= maxRecordTime && Microphone.IsRecording (selectedDevice))
			StopRecording ();
	}

	public void StartRecording()
	{
		startTime = Time.time;
		var rec = Microphone.Start (selectedDevice, true, maxRecordTime, maxFreq);

		LastRecords.Add (rec);
		LastRecorded = rec;

		Debug.Log (LastRecords.Count);
	}

	public void StopRecording()
	{
		Microphone.End (selectedDevice);
		startTime = 0;
	}
}
