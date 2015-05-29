using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockBehaviour : MonoBehaviour {

	public float speedUp = 0.5f;
	public float speedDown = 1.0f;
	public Dictionary<int, GameObject> horizontalCollisions, verticalCollisions;

	// Use this for initialization
	void Start () {
		horizontalCollisions = new Dictionary<int, GameObject> ();
		verticalCollisions = new Dictionary<int, GameObject> ();

		UpdateCollisions ();
	}
	
	// Update is called once per frame
	void Update () {

		MoveBlock ();

		if (transform.hasChanged) {
			transform.hasChanged = false;

			UpdateCollisions ();
		}
	}

	void MoveBlock () {
		RaycastHit hit;

		if (Physics.Raycast (transform.position, Vector3.down, out hit) && hit.distance >= 0.5) {
			transform.position += Vector3.down * Time.deltaTime * speedDown;
		}
		else {
			transform.position += Vector3.up * Time.deltaTime * speedUp;
		}
	}

	void OnTriggerEnter (Collider other) {

		if (transform.name == other.transform.name) {
			GameObject gameObject = other.transform.gameObject;

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

		horizontalCollisions.Remove (other.transform.GetInstanceID ());
		verticalCollisions.Remove (other.transform.GetInstanceID ());
	}

	void UpdateCollisions () {
		RaycastHit hit;
		
		horizontalCollisions.Clear();
		verticalCollisions.Clear();
		
		horizontalCollisions.Add(transform.GetInstanceID(), transform.gameObject);
		verticalCollisions.Add(transform.GetInstanceID(), transform.gameObject);
		
		Vector3[] horizontal_dirs = { Vector3.left, Vector3.right };
		
		foreach (Vector3 direction in horizontal_dirs) {
			if (Physics.Raycast (transform.position, direction, out hit)) {
				if (hit.transform.gameObject.tag == "Block" && hit.transform.name == transform.name)
					horizontalCollisions.Add(hit.transform.GetInstanceID(), hit.transform.gameObject);
			}
		}
		
		Vector3[] vertical_dirs = { Vector3.up, Vector3.down };
		
		foreach (Vector3 direction in vertical_dirs) {
			if (Physics.Raycast (transform.position, direction, out hit)) {
				if (hit.transform.gameObject.tag == "Block" && hit.transform.name == transform.name)
					verticalCollisions.Add(hit.transform.GetInstanceID(), hit.transform.gameObject);
			}
		}
	}

	public List<GameObject> GetHorizontalCollisions () {
		return new List<GameObject>(horizontalCollisions.Values);
	}

	public List<GameObject> GetVerticalCollisions () {
		return new List<GameObject>(verticalCollisions.Values);
	}
}
