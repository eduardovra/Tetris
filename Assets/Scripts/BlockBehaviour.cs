using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockBehaviour : MonoBehaviour {

	public Dictionary<int, GameObject> horizontalCollisions, verticalCollisions;

	// Use this for initialization
	void Start () {
		horizontalCollisions = new Dictionary<int, GameObject> ();
		verticalCollisions = new Dictionary<int, GameObject> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter (Collider other) {
		//Debug.Log (transform.name + " " + transform.position + " collided with " + other.transform.name + " " + other.transform.position);

		if (transform.name == other.transform.name) {
			GameObject gameObject = other.transform.gameObject;

			Debug.Log("Adding to collision list " + other.transform.GetInstanceID());

			// Horizontal
			if (transform.position.y == other.transform.position.y) {
				horizontalCollisions.Add(other.transform.GetInstanceID (), gameObject);
			}
			// Vertical
			else if (transform.position.x == other.transform.position.x) {
				verticalCollisions.Add(other.transform.GetInstanceID (), gameObject);
			}
		}
	}

	void OnTriggerExit (Collider other) {
		//Debug.Log (transform.name + " " + transform.position + " stopped colliding with " + other.transform.name + " " + other.transform.position);
		//GameObject gameObject = other.transform.gameObject;

		horizontalCollisions.Remove (other.transform.GetInstanceID ());
		verticalCollisions.Remove (other.transform.GetInstanceID ());
	}

	public List<GameObject> GetCollisions () {
		Dictionary<int, GameObject> dict = new Dictionary<int ,GameObject> ();

		foreach (GameObject obj in horizontalCollisions.Values) {
			dict.Add(obj.GetInstanceID(), obj);
		}

		foreach (GameObject obj in verticalCollisions.Values) {
			dict.Add(obj.GetInstanceID(), obj);
		}

		return new List<GameObject>(dict.Values);
	}


}
