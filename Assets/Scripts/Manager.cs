using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour {

	public Dictionary<int, GameObject> Blocks;

	// Use this for initialization
	void Start () {
		Blocks = new Dictionary<int, GameObject> ();
		GameObject rootObject = GameObject.Find ("Blocks").gameObject;

		foreach (Transform child in rootObject.transform) {
			Debug.Log("Adding " + child.GetInstanceID());
			Blocks.Add(child.GetInstanceID(), child.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {

		List<GameObject> blocksToDestroy = new List<GameObject> ();

		foreach (GameObject block in Blocks.Values) {
			BlockBehaviour behaviour = block.GetComponent(typeof(BlockBehaviour)) as BlockBehaviour;

			List<GameObject> blocksList = behaviour.GetCollisions();

			if (blocksList.Count > 1) {
				blocksToDestroy.AddRange(blocksList);
				blocksToDestroy.Add(block);
			}
		}

		foreach (GameObject block in blocksToDestroy) {
			Debug.Log("Destroying " + block.transform.GetInstanceID());
			Blocks.Remove(block.transform.GetInstanceID());
			Destroy (block);
		}
	}
}
