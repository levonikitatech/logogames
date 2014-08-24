using UnityEngine;
using System.Collections;

public class AnimHelper : MonoBehaviour {

	// Use this for initialization
	public void SetWait () {
		var animator = GetComponent<Animator>();
		animator.SetInteger("state", 2);
	}
	

}
