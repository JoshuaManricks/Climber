using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDController : MonoBehaviour {


	public Text goldDisplay;
	int goldCount;

	// Use this for initialization
	void Start () {
//		CentralDispatch.AddGold += AddGold;
	}

	public void AddGold(int value) {

		goldCount += value;

		goldDisplay.text = goldCount.ToString();
	}
}
