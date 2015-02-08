using UnityEngine;
using System.Collections;

public class Gold : MonoBehaviour {

	public int value;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void OnTriggerEnter(Collider collision) {
		
		Debug.Log ("OnTriggerEnter "+collision);

		GameObject.Find ("HUD Canvas").GetComponent<HUDController> ().AddGold (value);

		Destroy (gameObject);

	}
	

}
