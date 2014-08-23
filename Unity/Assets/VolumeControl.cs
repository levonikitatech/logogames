using UnityEngine;
using System.Collections;

public class VolumeControl : MonoBehaviour {

	MicControlC micControl;
	UISlider slider;

	public void Start()
	{
		var mainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		micControl = mainCamera.GetComponent<MicControlC> ();
		slider = GetComponent<UISlider> ();
	}

	public void Update()
	{
		var volume = slider.sliderValue;
		micControl.sourceVolume = volume * 100;
	}
}
