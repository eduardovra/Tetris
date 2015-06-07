using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BlockBehaviour : MonoBehaviour {

	public int instanceID;
	public bool isSettled_P;
	public List<GameObject> hor, ver;
	public Dictionary<int, GameObject> horizontalCollisions, verticalCollisions;
	
	private bool dying = false;
	private GameObject comboManager;
	private ComboBehaviour comboBehaviour;

	// Use this for initialization
	void Start () {
		horizontalCollisions = new Dictionary<int, GameObject> ();
		verticalCollisions = new Dictionary<int, GameObject> ();

		horizontalCollisions.Add (gameObject.GetInstanceID (), gameObject);
		verticalCollisions.Add (gameObject.GetInstanceID (), gameObject);

		comboManager = GameObject.Find ("Main Camera");
		comboBehaviour = comboManager.GetComponent<ComboBehaviour> ();
		comboBehaviour.AddBlockToList (gameObject);
		//comboManager.SendMessage ("AddBlockToList", gameObject);

		//Collider collider = GetComponent<Collider> ();
		//collider.isTrigger = true;

		instanceID = GetInstanceID ();
	}

	void OnDestroy () {
		//comboManager.SendMessage ("RemoveBlockFromList", gameObject);
		comboBehaviour.RemoveBlockFromList (gameObject);
	}
	
	// Update is called once per frame
	void Update () {

		if (dying) {
			AnimateDying ();
		}

		UpdateCollisions ();

		// debbuging
		isSettled_P = isSettled ();
		hor = new List<GameObject> (horizontalCollisions.Values);
		ver = new List<GameObject> (verticalCollisions.Values);
	}

	//
	// Collisions handling
	//
	void UpdateCollisions () {
		RaycastHit hit;
		Vector3[] vertical = { Vector3.up, Vector3.down };
		Vector3[] horizontal = { Vector3.left, Vector3.right };

		horizontalCollisions.Clear ();
		verticalCollisions.Clear ();

		horizontalCollisions.Add (gameObject.GetInstanceID (), gameObject);
		verticalCollisions.Add (gameObject.GetInstanceID (), gameObject);

		foreach (Vector3 dir in vertical) {
			if (Physics.Raycast (transform.position, dir, out hit, 0.5f)) {
				verticalCollisions.Add (hit.transform.gameObject.GetInstanceID (), hit.transform.gameObject);
			}
		}

		foreach (Vector3 dir in horizontal) {
			if (Physics.Raycast (transform.position, dir, out hit, 0.5f)) {
				horizontalCollisions.Add (hit.transform.gameObject.GetInstanceID (), hit.transform.gameObject);
			}
		}
	}

	//
	// Trigger management
	//

	void OnTriggerEnter (Collider other) {
		GameObject gameObject = other.gameObject;

		//Debug.Log ("on trigger enter " + gameObject);

		// Horizontal
		if (transform.position.y == other.transform.position.y) {
			horizontalCollisions.Add(gameObject.GetInstanceID (), gameObject);
		}
		// Vertical
		else if (transform.position.x == other.transform.position.x) {
			verticalCollisions.Add(gameObject.GetInstanceID (), gameObject);
		}
	}

	void OnTriggerExit (Collider other) {
		GameObject gameObject = other.gameObject;

		//Debug.Log ("on trigger exit " + gameObject);

		horizontalCollisions.Remove (gameObject.GetInstanceID ());
		verticalCollisions.Remove (gameObject.GetInstanceID ());
	}

	//
	// Return collisions excluding blocks that are not settled
	//
	
	public bool isSettled () {
//		RaycastHit hit;
		bool settled = false;
		
		// if there is a block under, we are settled
		if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
			settled = true;
/*
		foreach (GameObject block in verticalCollisions.Values) {
			if (block.transform.position.y < transform.position.y) {
				settled = true;
			}
		}
*/
		return settled;
	}

	private List<GameObject> GetCollisions (List<GameObject> inputCollisions) {
		List<GameObject> collisions = new List<GameObject>();

		if (inputCollisions.Count >= 3) {
			foreach (GameObject block in inputCollisions) {

				if (block.gameObject.name == gameObject.name) {
					BlockBehaviour blockBehaviour = block.GetComponent<BlockBehaviour> ();

					if (blockBehaviour.isSettled ()) {
						collisions.Add (block);
					}
				}
			}
		}

		return collisions;
	}

	public List<GameObject> GetHorizontalCollisions () {
		return GetCollisions (new List<GameObject> (horizontalCollisions.Values));
	}

	public List<GameObject> GetVerticalCollisions () {
		return GetCollisions (new List<GameObject> (verticalCollisions.Values));
	}

	//
	// Dying animation
	//

	void Die () {
		dying = true;
	}

	void AnimateDying () {
		transform.localScale -= Vector3.one * Time.deltaTime;

		if (transform.localScale.x <= 0.0f) {
			Destroy (gameObject);
		}
	}
}
