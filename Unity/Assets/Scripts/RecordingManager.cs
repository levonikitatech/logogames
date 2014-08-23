using UnityEngine;
using System.Collections;
using System.Linq;

public class RecordingManager : MonoBehaviour {


	SoundRecorder recorder;

	// Use this for initialization
	void Start () {
		recorder = GetComponent<SoundRecorder> ();
	}

	public void Update()
	{
		if (Input.GetKeyDown (KeyCode.T)) { //Push to talk
			Debug.Log("Started");
			recorder.StartRecording ();
		}
		//
		if (Input.GetKeyUp (KeyCode.T)) {
			Debug.Log("Stopped");
			recorder.StopRecording ();
			Play (recorder.LastRecorded);
		}
		//

		if (Input.GetKey (KeyCode.P)) {
			Debug.Log("Playing Last");
			Play (recorder.LastRecorded);
		}

		if (Input.GetKey (KeyCode.Q)) {
			Debug.Log("Playing 1");
			Play(recorder.LastRecords.Skip(0).FirstOrDefault());
		}
		if (Input.GetKey (KeyCode.W)) {
			Debug.Log("Playing 2");
			Play(recorder.LastRecords.Skip(1).FirstOrDefault());
		}
		if (Input.GetKey (KeyCode.E)) {
			Debug.Log("Playing 3");
			Play(recorder.LastRecords.Skip(2).FirstOrDefault());
		}
	}

	public void Play(AudioClip clip)
	{
		if (clip == null) return;
		if (audio.isPlaying) audio.Stop ();
		audio.clip = clip;
		audio.loop = false;
		audio.mute = false;
		audio.Play();
	}
}
