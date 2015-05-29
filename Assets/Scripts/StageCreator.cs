using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageCreator : MonoBehaviour {
	
	public GameObject[] Block_Prefabs;
	public float speedUp = 1.0f;
	public float speedDown = 1.0f;
	public Dictionary<int, GameObject> BaseBlocks;

	private Dictionary<int, GameObject> Blocks;
	private GameObject cursor;
	GameObject rootBlocks;

	// Use this for initialization
	void Start () {

		Blocks = new Dictionary<int, GameObject> ();
		BaseBlocks = new Dictionary<int, GameObject> ();
		rootBlocks = GameObject.Find ("Blocks").gameObject;
		cursor = GameObject.Find ("Cursor");

		for (int x = -3; x <= 3; x++) {
			for (int y = -1; y <= 3; y++) {
				int index = Random.Range (0, Block_Prefabs.Length);
				GameObject block = InstantiateBlock (Block_Prefabs [index], new Vector3 (x, y, 0), rootBlocks);

				if (y == -1) {
					BaseBlocks.Add(block.GetInstanceID(), block);
				}

				Blocks.Add(block.GetInstanceID(), block);
			}
		}
	}

	GameObject InstantiateBlock (GameObject prefab, Vector3 position, GameObject parent) {
		GameObject block = Instantiate (prefab, position, Quaternion.identity) as GameObject;
		block.transform.parent = parent.transform;
		block.name = prefab.name;

		Debug.Log (block.GetInstanceID());

		return block;
	}
	
	// Update is called once per frame
	void Update () {
		List<GameObject> blocksToDestroy = GetBlocksToDestroy ();

		DestroyBlocks (blocksToDestroy);

		//MoveBlocks ();

		UpdateBaseBlocks ();
	}

	void MoveBlocks () {
		Dictionary<int, Vector3> newBlockPositions = new Dictionary<int, Vector3> ();
		Vector3 Up = Vector3.up * Time.deltaTime * speedUp;

		cursor.transform.position += Up;

		foreach (KeyValuePair<int, GameObject> entry in Blocks) {
			RaycastHit hit;
			int key = entry.Key;
			GameObject block = entry.Value;
			
			if (Physics.Raycast (block.transform.position, Vector3.down, out hit) && hit.distance >= 0.5) {
				Vector3 Down = Vector3.down * Time.deltaTime * speedDown;
				newBlockPositions.Add(key, block.transform.position + Down);
			}
			else {
				newBlockPositions.Add(key, block.transform.position + Up);
			}
		}

		foreach (KeyValuePair<int, Vector3> entry in newBlockPositions) {
			int key = entry.Key;
			Vector3 position = entry.Value;
			GameObject block = Blocks[key];
			block.transform.position = position;
		}
	}

	void UpdateBaseBlocks () {
		float y = 0;

		foreach (GameObject block in BaseBlocks.Values) {
			y = block.transform.position.y;
			break;
		}

		if (y > 0) {
			BaseBlocks.Clear ();

			for (int x = -3; x <= 3; x++) {
				int index = Random.Range (0, Block_Prefabs.Length);
				GameObject block = InstantiateBlock (Block_Prefabs [index], new Vector3 (x, y - 1, 0), rootBlocks);
				BaseBlocks.Add(block.GetInstanceID(), block);
				Blocks.Add(block.GetInstanceID(), block);
			}
		}
	}

	List<GameObject> GetBlocksToDestroy () {
		List<GameObject> blocksToDestroy = new List<GameObject> ();

		foreach (GameObject block in Blocks.Values) {

			if (BaseBlocks.ContainsValue(block))
				continue;

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
			Blocks.Remove (block.GetInstanceID ());
			Destroy (block);
		}
	}
}
