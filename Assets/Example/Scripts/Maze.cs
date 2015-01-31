using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour {

	public IntVector2 size;

	public int tileScale;

	public bool roof = false;

	public MazeCell cellPrefab;

	public float generationStepDelay;

	public MazePassage passagePrefab;

	public MazeDoor doorPrefab;

	[Range(0f, 1f)]
	public float doorProbability;

	public MazeWall[] wallPrefabs;

	public MazeRoomSettings[] roomSettings;

	private MazeCell[,] cells;

	private List<MazeRoom> rooms = new List<MazeRoom>();

	/// <summary>
	/// Gets the random coordinates.
	/// </summary>
	/// <value>The random coordinates.</value>
	public IntVector2 RandomCoordinates {
		get {
			return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
		}
	}
	/// <summary>
	/// Containses the coordinates.
	/// </summary>
	/// <returns><c>true</c>, if coordinates was containsed, <c>false</c> otherwise.</returns>
	/// <param name="coordinate">Coordinate.</param>
	public bool ContainsCoordinates (IntVector2 coordinate) {
		return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	}

	/// <summary>
	/// Gets the cell.
	/// </summary>
	/// <returns>The cell.</returns>
	/// <param name="coordinates">Coordinates.</param>
	public MazeCell GetCell (IntVector2 coordinates) {
		return cells[coordinates.x, coordinates.z];
	}

	public IEnumerator Generate () {
//		WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell>();
		DoFirstGenerationStep(activeCells);

		while (activeCells.Count > 0) {
//			yield return null;
			DoNextGenerationStep(activeCells);
		}
//		for (int i = 0; i < rooms.Count; i++) {
//			rooms[i].Hide();
		//		}
		yield return null;
	}

	private void DoFirstGenerationStep (List<MazeCell> activeCells) {

		//set the start point to be at a random position on an edge
		IntVector2 startPos = new IntVector2 (Mathf.FloorToInt((size.x * Random.value)), 0);

		MazeCell newCell = CreateCell(startPos);
		newCell.Initialize(CreateRoom(-1));
		activeCells.Add(newCell);
	}

	private void DoNextGenerationStep (List<MazeCell> activeCells) {
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells[currentIndex];
		if (currentCell.IsFullyInitialized) {
			
			
			AddTreasure (currentCell);
			activeCells.RemoveAt(currentIndex);
			return;
		}
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (ContainsCoordinates(coordinates)) {
			MazeCell neighbor = GetCell(coordinates);
			if (neighbor == null) {
				neighbor = CreateCell(coordinates);
				CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
			else if (currentCell.room.settingsIndex == neighbor.room.settingsIndex) {
				CreatePassageInSameRoom(currentCell, neighbor, direction);
			}
			else {
				CreateWall(currentCell, neighbor, direction);
			}
		}
		else {
			CreateWall(currentCell, null, direction);
		}
	}

	public float treasureRate = 0.5f;
	public GameObject treasure;
	Vector3 tPos = new Vector3(0,0.01f,0);
	private void AddTreasure (MazeCell currentCell) {

		if (Random.value > treasureRate) {
			//instiate new treasure
			GameObject t = Instantiate(treasure) as GameObject;
			//add to cell
			t.transform.parent = currentCell.gameObject.transform;
			t.transform.localPosition = tPos;//Vector3.zero;

			Debug.Log ("ADD TREASURE "+t.transform.position);
		}



	}

	private MazeCell CreateCell (IntVector2 coordinates) {
		MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
		cells[coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.setRoof (roof);
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
			newCell.transform.localPosition = new Vector3((coordinates.x - size.x * 0.5f + 0.5f)*tileScale, 0f, (coordinates.z - size.z * 0.5f + 0.5f)*tileScale);
		return newCell;
	}

	private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
		MazePassage passage = Instantiate(prefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(prefab) as MazePassage;
		if (passage is MazeDoor) {
			otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
		}
		else {
			otherCell.Initialize(cell.room);
		}
		passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	private void CreatePassageInSameRoom (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
		MazePassage passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(passagePrefab) as MazePassage;
		passage.Initialize(otherCell, cell, direction.GetOpposite());
		if (cell.room != otherCell.room) {
			MazeRoom roomToAssimilate = otherCell.room;
			cell.room.Assimilate(roomToAssimilate);
			rooms.Remove(roomToAssimilate);
			Destroy(roomToAssimilate);
		}
	}


	private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {

		MazeWall wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
		wall.Initialize(cell, otherCell, direction);

		if (otherCell != null) {
			wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
			wall.Initialize(otherCell, cell, direction.GetOpposite());
		}
	}

	private MazeRoom CreateRoom (int indexToExclude) {
		MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
		newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
		if (newRoom.settingsIndex == indexToExclude) {
			newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
		}
		newRoom.settings = roomSettings[newRoom.settingsIndex];
		rooms.Add(newRoom);
		return newRoom;
	}
}