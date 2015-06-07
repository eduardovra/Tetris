using UnityEngine;
using System.Collections;

public class UpwardsMovementBehaviour : MonoBehaviour {

	enum MovementState {
		Stopped,
		MovingUp,
		MovingDown,
	};

	public float speedUp = 0.1f;
	public float speedDown = 0.8f;
	private MovementState state = MovementState.MovingUp;

	// Use this for initialization
	void Start () {
		ComboBehaviour.StartMovingUp += StartMovingUp;
		ComboBehaviour.StopMoving += StopMoving;
	}

	void onDestroy () {
		ComboBehaviour.StartMovingUp -= StartMovingUp;
		ComboBehaviour.StopMoving -= StopMoving;
	}
	
	// Update is called once per frame
	void Update () {

		switch (state) {
		case MovementState.MovingUp:
			MoveUp ();
			break;
		case MovementState.MovingDown:
			MoveDown ();
			break;
		default:
			break;
		}
	}

	//
	// Movement control
	//

	void MoveUp () {
		transform.position += Vector3.up * Time.deltaTime * speedUp;
	}

	void MoveDown () {
		RaycastHit hit;
		//BlockBehaviour blockBehaviour = GetComponent<BlockBehaviour> ();

		if (Physics.Raycast (transform.position, Vector3.down, out hit)) {
			Vector3 new_position = transform.position + (Vector3.down * Time.deltaTime * speedDown);

			if ((new_position.y - hit.transform.position.y) <= 0.5f) {
				Debug.Log ("Ajustando");
				new_position = hit.transform.position;
				new_position.y += 0.5f;
			}

			transform.position = new_position;
		}

//		if (blockBehaviour.isSettled () == false) {
//			transform.position += Vector3.down * Time.deltaTime * speedDown;
//		}
	}

	//
	// State changing
	//

	void StartMovingUp () {
		state = MovementState.MovingUp;
	}

	void StartMovingDown () {
		state = MovementState.MovingDown;
	}
	
	void StopMoving () {
		state = MovementState.Stopped;
	}
}
