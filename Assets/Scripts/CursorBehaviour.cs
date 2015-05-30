using UnityEngine;
using System.Collections;

public class CursorBehaviour : MonoBehaviour {

	public float speedUp = 0.5f;

	private bool canMove = true;

	// Use this for initialization
	void Start () {
		StageCreator.StartMoving += StartMoving;
		StageCreator.StopMoving += StopMoving;
	}

	void onDestroy () {
		StageCreator.StartMoving -= StartMoving;
		StageCreator.StopMoving -= StopMoving;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.anyKeyDown) {
			GameObject leftBlock = GetBlock (transform.FindChild ("Left").gameObject);
			GameObject rightBlock = GetBlock (transform.FindChild ("Right").gameObject);
			
			if (Input.GetButtonDown ("Swap")) {
				if (leftBlock && rightBlock)
					SwapBlocks (leftBlock, rightBlock);
				else if (leftBlock)
					MoveBlock (leftBlock, Vector3.right);
				else if (rightBlock)
					MoveBlock (rightBlock, Vector3.left);
			} else if (Input.GetAxis ("Horizontal") > 0) {
				MoveCursor (Vector3.right);
			} else if (Input.GetAxis ("Horizontal") < 0) {
				MoveCursor (Vector3.left);
			} else if (Input.GetAxis ("Vertical") > 0) {
				MoveCursor (Vector3.up);
			} else if (Input.GetAxis ("Vertical") < 0) {
				MoveCursor (Vector3.down);
			}
		} else {
			if (canMove) {
				transform.position += Vector3.up * Time.deltaTime * speedUp;
			}
		}
	}

	void StartMoving () {
		canMove = true;
	}
	
	void StopMoving () {
		canMove = false;
	}

	GameObject GetBlock(GameObject cursor) {
		GameObject go = null;
		RaycastHit hit;

		if (Physics.Raycast (cursor.transform.position, Vector3.forward, out hit)) {
			go = hit.transform.gameObject;
		}

		return go;
	}

	void SwapBlocks(GameObject leftBlock, GameObject rightBlock) {
		Vector3 tempPosition = leftBlock.transform.position;
		leftBlock.transform.position = rightBlock.transform.position;
		rightBlock.transform.position = tempPosition;
	}

	void MoveBlock(GameObject block, Vector3 direction) {
		block.transform.position += direction;
	}

	void MoveCursor(Vector3 direction){
		transform.position += direction;
	}
}
