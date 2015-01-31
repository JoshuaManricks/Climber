using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Maze mazePrefab;

	public CameraType cameraType;
	public Player OVRCam;
	public Player standCam;

	private Maze mazeInstance;

	private Player playerInstance;

	private void Start () {
		StartCoroutine(BeginGame());
	}
	
	private void Update () {
//		if (Input.GetKeyDown(KeyCode.Space)) {
//			RestartGame();
//		}
	}

	private IEnumerator BeginGame () {
//		Camera.main.clearFlags = CameraClearFlags.Skybox;
//		Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
		mazeInstance = Instantiate(mazePrefab) as Maze;
		yield return StartCoroutine(mazeInstance.Generate());

		if (cameraType == CameraType.OVR) playerInstance = Instantiate(OVRCam) as Player;
		else playerInstance = Instantiate(standCam) as Player;
		
		playerInstance.SetLocation(mazeInstance.GetCell(mazeInstance.RandomCoordinates));

//		Camera.main.clearFlags = CameraClearFlags.Depth;
//		Camera.main.rect = new Rect(0f, 0f, 0.5f, 0.5f);


		UpdateRenderSettings ();
	}

	private void UpdateRenderSettings () {
		if (cameraType == CameraType.OVR) {
			float col = 4/255;
			RenderSettings.ambientLight = new Color(col,col,col);
		}
	}

	private void RestartGame () {
		StopAllCoroutines();
		Destroy(mazeInstance.gameObject);
		if (playerInstance != null) {
			Destroy(playerInstance.gameObject);
		}
		StartCoroutine(BeginGame());
	}
}