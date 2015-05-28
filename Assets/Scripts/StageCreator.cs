using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageCreator : MonoBehaviour {
	
	public GameObject[] Block_Prefabs;
	public float speedUp = 0.2f;
	public float speedDown = 50.0f;
	public List<GameObject> BaseBlocks;

	private Dictionary<int, GameObject> Blocks;
	private GameObject cursor;
	GameObject rootBlocks;

	// Use this for initialization
	void Start () {

		Blocks = new Dictionary<int, GameObject> ();
		BaseBlocks = new List<GameObject> ();
		rootBlocks = GameObject.Find ("Blocks").gameObject;
		cursor = GameObject.Find ("Cursor");

		for (int x = -3; x <= 3; x++) {
			for (int y = -1; y <= 4; y++) {
				int index = Random.Range (0, Block_Prefabs.Length);
				GameObject block = InstantiateBlock (Block_Prefabs [index], new Vector3 (x, y, 0), rootBlocks);

				if (y == -1) {
					BaseBlocks.Add(block);
				}

				Debug.Log("Adding " + block.GetInstanceID());
				Blocks.Add(block.GetInstanceID(), block);
			}
		}
	}

	GameObject InstantiateBlock (GameObject prefab, Vector3 position, GameObject parent) {
		GameObject block = Instantiate (prefab, position, Quaternion.identity) as GameObject;
		block.transform.parent = parent.transform;
		block.name = prefab.name;

		return block;
	}
	
	// Update is called once per frame
	void Update () {
		List<GameObject> blocksToDestroy = GetBlocksToDestroy ();

		DestroyBlocks (blocksToDestroy);

		MoveBlocks ();

		UpdateBaseBlocks ();
	}

	void MoveBlocks () {
		float y_Up = speedUp * Time.deltaTime;

		cursor.transform.Translate (0, y_Up, 0);

		foreach (GameObject block in Blocks.Values) {
			RaycastHit hit;

			if (Physics.Raycast (block.transform.position, Vector3.down, out hit)) {
				Debug.Log(hit.distance);
				if (hit.distance >= 0.5) {
					float y_Down = speedDown * Time.deltaTime;
					y_Down = hit.transform.position.y + 0.5f;
					//block.transform.Translate (0, y_Down, 0);
					block.transform.Translate (Vector3.down * Time.deltaTime * speedDown);
				}
				else {
					block.transform.Translate (0, y_Up, 0);
				}
			}
			else {
				block.transform.Translate (0, y_Up, 0);
			}
		}
	}

	void UpdateBaseBlocks () {
		float y = BaseBlocks [0].transform.position.y;

		if (y > 0) {
			BaseBlocks.Clear ();

			for (int x = -3; x <= 3; x++) {
				int index = Random.Range (0, Block_Prefabs.Length);
				GameObject block = InstantiateBlock (Block_Prefabs [index], new Vector3 (x, y - 1, 0), rootBlocks);
				BaseBlocks.Add(block);
				Debug.Log("Adding " + block.GetInstanceID());
				Blocks.Add(block.GetInstanceID(), block);
			}
		}
	}

	List<GameObject> GetBlocksToDestroy () {
		List<GameObject> blocksToDestroy = new List<GameObject> ();
		
		foreach (GameObject block in Blocks.Values) {
			BlockBehaviour behaviour = block.GetComponent (typeof(BlockBehaviour)) as BlockBehaviour;
			
			List<GameObject> horizontalBlocksList = behaviour.GetHorizontalCollisions ();
			
			if (horizontalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (horizontalBlocksList);
				blocksToDestroy.Add (block);
			}
			
			List<GameObject> verticalBlocksList = behaviour.GetVerticalCollisions ();
			
			if (verticalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (verticalBlocksList);
				blocksToDestroy.Add (block);
			}
		}

		return blocksToDestroy;
	}

	void DestroyBlocks (List<GameObject> blocksToDestroy) {
		foreach (GameObject block in blocksToDestroy) {
			Debug.Log ("Destroying " + block.GetInstanceID ());
			Blocks.Remove (block.GetInstanceID ());
			Destroy (block);
		}
	}
}
