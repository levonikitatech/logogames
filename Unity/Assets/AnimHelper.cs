using UnityEngine;
using System.Collections;

public class AnimHelper : MonoBehaviour {

	public UIPanel helloPanel;

	// Use this for initialization
	public void SetWait () {
		var animator = GetComponent<Animator>();
		animator.SetInteger("state", 2);
	}

	public void ShowHelloPanel() {
		if(helloPanel != null)
		{
			var anim = helloPanel.GetComponent<Animator>();
			if(anim != null) anim.SetBool("Show", true);
		}
	}

}
