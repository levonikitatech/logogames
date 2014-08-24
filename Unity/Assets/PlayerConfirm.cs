using UnityEngine;
using System.Collections;

public class PlayerConfirm : MonoBehaviour {

	GameScriptController controller;
	public UIPanel panel;

	// Use this for initialization
	void Start () {
		var mainObject = GameObject.FindGameObjectWithTag ("MainScript");
		if (mainObject != null)
			controller = mainObject.GetComponent<GameScriptController> ();
	}


	public void OnClick()
	{
		if (controller != null) controller.stepIsConfirmed = true;

		if (panel != null) {
			var animator = panel.GetComponent<Animator>();
			if(animator != null) animator.SetBool("Show", false);
		}

	}
}
