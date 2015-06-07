using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ControllerBehaviour : MonoBehaviour {
	
	public class Color_Names {
		public Color color;
		public string name;
	}

	public GameObject Block_Prefab;
	public Color[] colors;
	public string[] names;

	private Dictionary<int, GameObject> BaseBlocks;
	private GameObject rootBlocks;

	// Use this for initialization
	void Start () {

		BaseBlocks = new Dictionary<int, GameObject> ();
		rootBlocks = GameObject.Find ("Blocks").gameObject;

		for (int x = -3; x <= 3; x++) {
			for (int y = -1; y <= 1; y++) {
				int index = Random.Range (0, colors.Length);
				GameObject block = InstantiateBlock (names[index], Block_Prefab, colors[index], new Vector3 (x, y, 0), rootBlocks);
				
				if (y == -1) {
					BaseBlocks.Add(block.GetInstanceID(), block);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateBaseBlocks ();
	}

	void UpdateBaseBlocks () {
		float y = 0;
		
		foreach (GameObject block in BaseBlocks.Values) {
			if (block) {
				y = block.transform.position.y;
				break;
			}
		}
		
		if (y > 0) {
			BaseBlocks.Clear ();
			
			for (int x = -3; x <= 3; x++) {
				int index = Random.Range (0, colors.Length);
				GameObject block = InstantiateBlock (names[index], Block_Prefab, colors[index], new Vector3 (x, y - 1, 0), rootBlocks);
				BaseBlocks.Add(block.GetInstanceID(), block);
			}
		}
	}

	GameObject InstantiateBlock (string name, GameObject prefab, Color color, Vector3 position, GameObject parent) {
		GameObject block = Instantiate (prefab, position, Quaternion.identity) as GameObject;
		block.transform.parent = parent.transform;
		block.name = name;

		Renderer renderer = block.GetComponent<Renderer> ();
		renderer.material.color = color;

		block.SendMessage ("StartMovingUp");

		return block;
	}
}
