using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComboBehaviour : MonoBehaviour {

	enum State {
		LookingForCombo,
		AnimateDestroy,
		BlocksFalling,
	};

	public delegate void StartMovingUpAction();
	public delegate void StopMovingAction();
	public static event StartMovingUpAction StartMovingUp;
	public static event StopMovingAction StopMoving;

	private Dictionary<int, GameObject> Blocks;
	private State state = State.LookingForCombo;
	private List<GameObject> blocksToDestroy;
	private List<GameObject> blocksToFall;

	// Use this for initialization
	void Start () {
		Blocks = new Dictionary<int, GameObject> ();
		blocksToFall = new List<GameObject> ();

		Debug.Log("State changed to LookingForCombo");
	}
	
	// Update is called once per frame
	void Update () {
		StartCoroutine(State_Machine ());
	}

	//
	// State machine
	//

	IEnumerator State_Machine () {
		switch (state) {
		case State.LookingForCombo:
			
			blocksToDestroy = GetBlocksToDestroy ();
			
			if (blocksToDestroy.Count > 0) {
				
				Debug.Log ("blocks to destroy " + blocksToDestroy.Count);
				
				if (StopMoving != null) {
					StopMoving ();
				}
				
				state = State.AnimateDestroy;
				Debug.Log("State changed to AnimateDestroy");
			}
			else {

				blocksToFall.Clear ();

				foreach (GameObject block in Blocks.Values) {
					BlockBehaviour blockBehaviour = block.GetComponent<BlockBehaviour> ();
					
					if (block.transform.position.y > 0.0f) {
						if (blockBehaviour.isSettled () == false) {
							blocksToFall.Add(block);
						}
					}
				}

				if (blocksToFall.Count > 0) {

					if (StopMoving != null) {
						StopMoving ();
					}

					state = State.BlocksFalling;
					Debug.Log("State changed to BlocksFalling");
				}

				foreach (GameObject block in blocksToFall) {
					block.SendMessage ("StartMovingDown");
				}
			}
			
			break;
		case State.AnimateDestroy:
			yield return StartCoroutine (DestroyBlocks ());
			
			// After animation, any blocks should fall ?
			foreach (GameObject block in Blocks.Values) {
				BlockBehaviour blockBehaviour = block.GetComponent<BlockBehaviour> ();
				
				if (block.transform.position.y > 0.0f) {
					if (blockBehaviour.isSettled () == false) {
						block.SendMessage ("StartMovingDown");
						blocksToFall.Add(block);
					}
				}
			}
			
			state = State.BlocksFalling;
			Debug.Log("State changed to BlocksFalling");
			
			break;
		case State.BlocksFalling:
			
			// all blocks settled ?
			bool allSettled = true;
			
			foreach (GameObject block in blocksToFall) {
				
				BlockBehaviour blockBehaviour = block.GetComponent<BlockBehaviour> ();
				
				if (blockBehaviour.isSettled () == false) {
					allSettled = false;
					break;
				}
				else {
					block.SendMessage ("StopMoving");
				}
			}
			
			if (allSettled) {
				blocksToFall.Clear ();
				
				if (StartMovingUp != null) {
					StartMovingUp ();
				}
				
				state = State.LookingForCombo;
				Debug.Log("State changed to LookingForCombo");
			}
			
			break;
		default:
			break;
		}
	}

	//
	// Blocks destruction management
	//

	List<GameObject> GetBlocksToDestroy () {
		List<GameObject> blocksToDestroy = new List<GameObject> ();
		
		foreach (GameObject block in Blocks.Values) {

			BlockBehaviour blockBehaviour = block.GetComponent<BlockBehaviour> ();
				
			List<GameObject> horizontalBlocksList = blockBehaviour.GetHorizontalCollisions ();
			
			if (horizontalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (horizontalBlocksList);
			}
			
			List<GameObject> verticalBlocksList = blockBehaviour.GetVerticalCollisions ();
			
			if (verticalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (verticalBlocksList);
			}
		}
		
		return blocksToDestroy;
	}
	
	IEnumerator DestroyBlocks () {

		foreach (GameObject block in blocksToDestroy) {
			if (block != null) {
				block.SendMessage("Die");
			}
		}

		yield return new WaitForSeconds(1);

		blocksToDestroy.Clear ();
	}

	//
	// List of blocks in scene controls
	//

	public void AddBlockToList (GameObject block) {
		Blocks.Add (block.GetInstanceID (), block);
	}

	public void RemoveBlockFromList (GameObject block) {
		Blocks.Remove (block.GetInstanceID ());
	}
}
