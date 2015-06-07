using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tetris : MonoBehaviour {

	public enum GameState {
		UpdateCursor,
		LookingForMatch,
		GoingUp,
		UpdateBaseBlocks,
		LookingForBlocksToFall,
		BlocksFalling,
		AnimateDestroy,
	}

	public GameObject Block_Prefab;
	public GameObject Cursor_Prefab;
	public float speedUp = 0.5f;
	public float speedDown = 25.0f;
	public GameState state = GameState.UpdateCursor;
	public Queue<Cursor.Key> keys_pressed;

	public Dictionary<int, Block> Blocks;
	public Cursor cursor;

	// Use this for initialization
	void Start () {
		keys_pressed = new Queue<Cursor.Key> ();
		cursor = new Cursor (Cursor_Prefab);
		Create_Blocks ();
	}
	
	// Update is called once per frame
	void Update () {
		State_Machine ( cursor.Capture_Input () );
	}
	
	void Create_Blocks () {
		GameObject rootObject = new GameObject ("Blocks");
		Blocks = new Dictionary<int, Block> ();

		for (int x = -3; x <= 3; x++) {
			for (int y = -1; y <= 3; y++) {
				Block block = new Block (Block_Prefab, new Vector3 (x, y, 0), rootObject);
				
				if (y == -1) {
					block.SetBase (true);
				}

				Blocks.Add(block.GetID (), block);
			}
		}
	}

	//
	// State machine
	//

	void State_Machine (Cursor.Key key) {

		if (key != Cursor.Key.Nothing) {
			keys_pressed.Enqueue (key);
		}

		switch (state) {
		case GameState.UpdateCursor:

			Cursor.Key input_key = (keys_pressed.Count > 0) ? keys_pressed.Dequeue () : Cursor.Key.Nothing;

			if (cursor.Process_Input ( input_key )) {
				SetState(GameState.LookingForBlocksToFall);
			}
			else {
				SetState(GameState.LookingForMatch);
			}

			break;

		case GameState.LookingForMatch:

			List<Block> blocksToDestroy = GetBlocksToDestroy ();
			
			if (blocksToDestroy.Count >= 3) {
				
				foreach (Block block in blocksToDestroy) {
					block.SetState (Block.BlockState.Dying);
				}
				
				SetState(GameState.AnimateDestroy);
			}
			else {
				SetState(GameState.GoingUp);
			}

			break;

		case GameState.GoingUp:
			// Move all blocks up
			foreach (Block block in Blocks.Values) {
				block.MoveUp (speedUp);
			}
			// Move cursor up
			cursor.MoveUp (speedUp);

			SetState(GameState.UpdateBaseBlocks);

			break;

		case GameState.UpdateBaseBlocks:
			// Are base blocks above the base line ?
			UpdateBaseBlocks ();

			SetState(GameState.UpdateCursor);
			break;

		case GameState.AnimateDestroy:
			bool all_blocks_destroyed = true;

			foreach (Block block in new List<Block>(Blocks.Values)) {
				if (block.state == Block.BlockState.Dying) {
					if (block.AnimateDying ()) {
						Blocks.Remove(block.GetID ());
						block.Destroy ();
					}
					else {
						all_blocks_destroyed = false;
					}
				}
			}

			if (all_blocks_destroyed)
				SetState(GameState.LookingForBlocksToFall);

			break;

		case GameState.LookingForBlocksToFall:
			// Any blocks floating ?
			bool any_block_to_fall = false;

			foreach (Block block in Blocks.Values) {

				// Base blocks should not fall
				if (block.is_base)
					continue;

				if (block.isSettled () == false) {
					any_block_to_fall = true;
					block.SetState(Block.BlockState.Falling);
				}
			}

			if (any_block_to_fall)
				SetState(GameState.BlocksFalling);
			else
				SetState(GameState.UpdateCursor);

			break;

		case GameState.BlocksFalling:
			bool all_blocks_settled = true;

			foreach (Block block in Blocks.Values) {
				if (block.state == Block.BlockState.Falling) {
					if (block.MoveDown (speedDown)) {
						block.SetState(Block.BlockState.Stopped);
					}
					else {
						all_blocks_settled = false;
					}
				}
			}

			if (all_blocks_settled) {
				SetState(GameState.LookingForBlocksToFall);
			}

			break;

		default:
			Debug.Log ("Default State " + state);
			break;
		}
	}

	void SetState (GameState new_state) {
//		Debug.Log ("Game state changed from " + state + " to " + new_state);
		state = new_state;
	}

	//
	// Blocks destruction management
	//

	List<Block> GetBlocksToDestroy () {
		List<Block> blocksToDestroy = new List<Block> ();
		
		foreach (Block block in Blocks.Values) {
			
			List<Block> horizontalBlocksList = block.GetHorizontalCollisions ();
			
			if (horizontalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (horizontalBlocksList);
			}
			
			List<Block> verticalBlocksList = block.GetVerticalCollisions ();
			
			if (verticalBlocksList.Count >= 3) {
				blocksToDestroy.AddRange (verticalBlocksList);
			}
		}
		
		return blocksToDestroy;
	}

	//
	// Base blocks management
	//

	void UpdateBaseBlocks () {
		float y = 0;

		// Get y value for base blocks
		foreach (Block block in Blocks.Values) {
			if (block.is_base) {
				y = block.gameObject.transform.position.y;
				// Clean flag if they are above bottom line
				if (y > 0) {
					block.SetBase (false);
				}
			}
		}

		// Create new base blocks
		if (y > 0) {
			GameObject rootObject = GameObject.Find ("Blocks");

			for (int x = -3; x <= 3; x++) {
				Block block = new Block (Block_Prefab, new Vector3 (x, y - 1, 0), rootObject);

				block.SetBase (true);
				
				Blocks.Add(block.GetID (), block);
			}
		}
	}
}

public class Block {

	public enum BlockState {
		Stopped,
		GoingUp,
		Falling,
		Dying,
	}

	public struct Block_Type {
		public Color color;
		public string name;
	}

	Block_Type[] block_types = new Block_Type []
	{
		new Block_Type { color = Color.blue, name = "Blue" },
		new Block_Type { color = Color.red, name = "Red" },
		new Block_Type { color = Color.green, name = "Green" },
		new Block_Type { color = Color.yellow, name = "Yellow" },
	};

	public BlockState state = BlockState.GoingUp;
	public GameObject gameObject;
	public bool is_base = false;

	private Tetris tetris;

	public Block (GameObject prefab, Vector3 position, GameObject parent)
	{
		int index = Random.Range (0, block_types.Length);

		gameObject = GameObject.Instantiate (prefab, position, Quaternion.identity) as GameObject;
		gameObject.transform.parent = parent.transform;
		gameObject.name = block_types[index].name;
		
		SetColor (block_types [index].color);

		tetris = GameObject.Find ("Main Camera").GetComponent<Tetris> ();
	}

	public void Destroy ()
	{
		GameObject.Destroy(gameObject);
	}

	public int GetID () {
		return gameObject.GetInstanceID ();
	}

	public void SetState (BlockState new_state) {
		Debug.Log ("Block state changed from " + state + " to " + new_state);
		state = new_state;
	}

	public void SetBase (bool isbase) {
		is_base = isbase;

		if (is_base) {
			SetColor ( GetColor () / 2 );
		} else {
			SetColor ( GetColor () * 2 );
		}
	}

	void SetColor (Color color) {
		Renderer renderer = gameObject.GetComponent<Renderer> ();
		renderer.material.color = color;
	}

	Color GetColor () {
		Renderer renderer = gameObject.GetComponent<Renderer> ();
		return renderer.material.color;
	}

	//
	// Collisions detection
	//

	public bool isSettled () {
		return isSettled (gameObject.transform);
	}

	public static bool isSettled (Transform transform) {
		return Physics.Raycast (transform.position, Vector3.down, 0.51f);
	}

	List<Block> GetCollisionsList (Vector3[] directions)
	{
		List<Block> collisions = new List<Block> ();
		RaycastHit hit;

		if (is_base == false) {
			collisions.Add (this);

			foreach (Vector3 dir in directions) {
				if (Physics.Raycast (gameObject.transform.position, dir, out hit, 0.5f)) {
					if (hit.transform.gameObject.name == gameObject.name) {
						Block block = GameObject_To_Block (hit.transform.gameObject);
						if (block.is_base == false) {
							collisions.Add (block);
						}
					}
				}
			}
		}

		return collisions;
	}

	public List<Block> GetHorizontalCollisions () {
		Vector3[] horizontal = { Vector3.left, Vector3.right };
		return GetCollisionsList (horizontal);
	}

	public List<Block> GetVerticalCollisions () {
		Vector3[] vertical = { Vector3.up, Vector3.down };
		return GetCollisionsList (vertical);
	}

	Block GameObject_To_Block (GameObject go) {
		return tetris.Blocks [ go.GetInstanceID () ];
	}

	//
	// Movement
	//

	public void MoveUp (float speed) {
		gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
	}

	// Returns true when object underneath is hitten
	public bool MoveDown (float speed) {

		Vector3 new_position = gameObject.transform.position + (Vector3.down * Time.deltaTime * speed);
		RaycastHit hit;

		if (Physics.Raycast (gameObject.transform.position, Vector3.down, out hit)) {

			if ((new_position.y - hit.transform.position.y) <= 1.0f) {
				new_position = hit.point;
				new_position.y += 0.5f;
			}
		}

		gameObject.transform.position = new_position;

		return isSettled ();
	}

	//
	// Animation
	//

	public bool AnimateDying () {
		gameObject.transform.localScale -= Vector3.one * Time.deltaTime;
		return gameObject.transform.localScale.x <= 0.0f;
	}
}

public class Cursor {

	public enum Key {
		Nothing,
		Swap,
		Left,
		Right,
		Up,
		Down,
	}

	public GameObject gameObject;

	private GameObject leftCursor, rightCursor;

	public Cursor (GameObject prefab)
	{
		gameObject = GameObject.Instantiate (prefab) as GameObject;
		gameObject.name = prefab.name;

		leftCursor = gameObject.transform.FindChild ("Left").gameObject;
		rightCursor = gameObject.transform.FindChild ("Right").gameObject;
	}
	
	public Key Capture_Input () {
		Key key = Key.Nothing;

		if (Input.anyKeyDown) {
			if (Input.GetButtonDown ("Swap")) {
				key = Key.Swap;
			} else if (Input.GetAxis ("Horizontal") > 0) {
				key = Key.Right;
			} else if (Input.GetAxis ("Horizontal") < 0) {
				key = Key.Left;
			} else if (Input.GetAxis ("Vertical") > 0) {
				key = Key.Up;
			} else if (Input.GetAxis ("Vertical") < 0) {
				key = Key.Down;
			}
		}

		return key;
	}

	public bool Process_Input (Key key) {
		bool swaped = false;

		switch (key) {
		case Key.Swap:

				GameObject leftBlock = GetBlock (leftCursor);
				GameObject rightBlock = GetBlock (rightCursor);

				swaped = true;

				if (leftBlock && rightBlock)
					SwapBlocks (leftBlock, rightBlock);
				else if (leftBlock)
					MoveBlock (leftBlock, Vector3.right);
				else if (rightBlock)
					MoveBlock (rightBlock, Vector3.left);
				else
					swaped = false;
			break;
		case Key.Right:
			MoveCursor (Vector3.right);
			break;
		case Key.Left:
			MoveCursor (Vector3.left);
			break;
		case Key.Up:
			MoveCursor (Vector3.up);
			break;
		case Key.Down:
			MoveCursor (Vector3.down);
			break;
		default:
			break;
		}

		return swaped;
	}

	GameObject GetBlock (GameObject cursor) {
		GameObject go = null;
		RaycastHit hit;
		
		if (Physics.Raycast (cursor.transform.position, Vector3.forward, out hit)) {
			go = hit.transform.gameObject;
		}
		
		return go;
	}
	
	void SwapBlocks (GameObject leftBlock, GameObject rightBlock) {
		Vector3 tempPosition = leftBlock.transform.position;
		leftBlock.transform.position = rightBlock.transform.position;
		rightBlock.transform.position = tempPosition;
	}
	
	void MoveBlock (GameObject block, Vector3 direction) {
		block.transform.position += direction;
	}
	
	void MoveCursor (Vector3 direction){
		gameObject.transform.position += direction;
	}

	public void MoveUp (float speed) {
		gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
	}
}
