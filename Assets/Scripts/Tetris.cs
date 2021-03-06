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
		GameOver,
	}

	public GameObject Block_Prefab;
	public GameObject Cursor_Prefab;
	public float speedUp = 0.5f;
	public float speedDown = 25.0f;
	public float speedUpAccelerated = 20.0f;
	public float min_x = -3, max_x = 3, min_y = -1, max_y = 9;
	public int inital_block_rows = 4;
	public GameState state = GameState.UpdateCursor;

	public Dictionary<int, Block> Blocks;
	public Cursor cursor;

	private string time = "0'00";
	private string speed = "1";
	private string level = "EASY";
	private Vector3 lGuiPos, rGuiPos;

	private bool cursor_updated = false;
	private float gameTime = 0.0f; // Seconds since game start
	private int gameScore = 0;

	// Use this for initialization
	void Start () {
		cursor = new Cursor (Cursor_Prefab);
		Create_Scene ();
		Create_Blocks ();
	}
	
	// Update is called once per frame
	void Update () {
		State_Machine ();
		UpdateTimer ();
	}

	void OnGUI () {

		GUI.Box (new Rect (Screen.width / 100, Screen.height / 10, 100, 40), "Time\n" + time);
		GUI.Box (new Rect (92 * (Screen.width / 100), Screen.height / 10, 100, 140), "Score\n" + gameScore + "\n\nSpeed\n" + speed + "\n\nLevel\n" + level);

		// This can be used to position the GUI elements next to the gameObjects
		//GUI.Box (new Rect (lGuiPos.x, lGuiPos.y, 100, 40), "Time\n" + time);
		//GUI.Box (new Rect (rGuiPos.x, rGuiPos.y, 100, 140), "Score\n" + score + "\n\nSpeed\n" + speed + "\n\nLevel\n" + level);

		if(state == GameState.GameOver)
			GUI.Box (new Rect (Screen.width * 0.5f - 100, Screen.height * 0.5f - 40, 200, 80), "Game Over!");
	}
	
	void Create_Blocks () {
		GameObject rootObject = new GameObject ("Blocks");
		Blocks = new Dictionary<int, Block> ();

		for (float x = min_x; x <= max_x; x++) {
			for (float y = min_y, rows = 0; y <= max_y && rows < inital_block_rows; y++, rows++) {
				Block block = new Block (Block_Prefab, new Vector3 (x, y, 0), rootObject);
				
				if (y == min_y) {
					block.SetBase (true);
				}

				Blocks.Add(block.GetID (), block);
			}
		}
	}

	public Block GetBlock (GameObject go) {
		return Blocks [ go.GetInstanceID () ];
	}

	//
	// Scene
	//

	void Create_Scene () {

		//
		// Frame
		//

		// frame for blocks
		Vector3 vertScale = new Vector3 (1, max_y - min_y, 1.3f);
		Vector3 horiScale = new Vector3 (3 + max_x - min_x, 1, 1.3f);
		GameObject cube;
		// left
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3(min_x - 1, 0.5f + (min_y + max_y) / 2, 0);
		cube.transform.localScale = vertScale;
		lGuiPos = Camera.main.WorldToScreenPoint(cube.transform.position);
		// right
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3(max_x + 1, 0.5f + (min_y + max_y) / 2, 0);
		cube.transform.localScale = vertScale;
		rGuiPos = Camera.main.WorldToScreenPoint(cube.transform.position);
		// up
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3((min_x + max_x) / 2, max_y + 1, 0);
		cube.transform.localScale = horiScale;
		// down
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3((min_x + max_x) / 2, min_y, 0);
		cube.transform.localScale = horiScale;
	}

	void UpdateTimer () {
		if (state != GameState.GameOver) {
			gameTime += 1 * Time.deltaTime;

			int minutes = (int)(gameTime / 60);
			int seconds = (int)(gameTime % 60);

			time = minutes + "'" + seconds.ToString ("D2");
		}
	}

	//
	// State machine
	//

	void State_Machine () {

		if ( cursor.Process_Input (cursor.Capture_Input ()) ) {
			cursor_updated = true;
		}

		switch (state) {
		case GameState.UpdateCursor:

			if (cursor_updated) {
				cursor_updated = false;
				SetState(GameState.LookingForBlocksToFall);
			}
			else {
				SetState(GameState.LookingForMatch);
			}

			break;

		case GameState.LookingForMatch:

			List<Block> blocksToDestroy = GetBlocksToDestroy ();
			
			if (blocksToDestroy.Count >= 3) {

				gameScore += 30;

				if (blocksToDestroy.Count > 3) {
					gameScore += (blocksToDestroy.Count - 3) * 20;
				}
				
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
			float speed = (Input.GetButton ("Accelerate")) ?
				speedUpAccelerated * Time.deltaTime : speedUp * Time.deltaTime;

			// Move all blocks up
			foreach (Block block in Blocks.Values) {
				block.MoveUp (speed);
			}
			// Move cursor up
			cursor.MoveUp (speed);

			// Adjusts cursor so it doesnt pass by the upper limit
			if (cursor.gameObject.transform.position.y > max_y) {
				Vector3 cursor_pos = cursor.gameObject.transform.position;
				cursor_pos.y--;
				cursor.gameObject.transform.position = cursor_pos;
			}

			SetState(GameState.UpdateBaseBlocks);

			// Checks for game over
			foreach (Block block in Blocks.Values) {
				if(block.gameObject.transform.position.y > max_y) {
					SetState(GameState.GameOver);
				}
			}

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
			float fallSpeed = speedDown * Time.deltaTime;

			foreach (Block block in Blocks.Values) {
				if (block.state == Block.BlockState.Falling) {
					if (block.MoveDown (fallSpeed)) {
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
		Dictionary<int,Block> blocksToDestroy = new Dictionary<int,Block> ();
		
		foreach (Block block in Blocks.Values) {
			
			List<Block> horizontalBlocksList = block.GetHorizontalCollisions ();
			
			if (horizontalBlocksList.Count >= 3) {
				foreach (Block b in horizontalBlocksList)  {
					if (blocksToDestroy.ContainsKey(b.GetID()) == false)
						blocksToDestroy.Add(b.GetID(), b);
				}
			}
			
			List<Block> verticalBlocksList = block.GetVerticalCollisions ();
			
			if (verticalBlocksList.Count >= 3) {
				foreach (Block b in verticalBlocksList) {
					if (blocksToDestroy.ContainsKey(b.GetID()) == false)
						blocksToDestroy.Add(b.GetID(), b);
				}	
			}
		}
		
		return new List<Block> (blocksToDestroy.Values);
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

			for (float x = min_x; x <= max_x; x++) {
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
		public string texture;
		public string name;
	}

	Block_Type[] block_types = new Block_Type []
	{
		new Block_Type { color = Color.blue, texture = "BlockCross", name = "Blue" },
		new Block_Type { color = Color.red, texture = "BlockCircle", name = "Red" },
		new Block_Type { color = Color.green, texture = "BlockTriangle", name = "Green" },
		new Block_Type { color = Color.yellow, texture = "BlockSquare", name = "Yellow" },
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

		Texture2D texture = Resources.Load (block_types [index].texture) as Texture2D;
		Renderer renderer = gameObject.GetComponent<Renderer> ();
		renderer.material.mainTexture = texture;
		renderer.material.SetTextureScale ("_MainTex", new Vector2 (1, -1)); // Flip texture

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

		if (new_state == BlockState.Dying) {
			Renderer renderer = gameObject.GetComponent<Renderer> ();
			Material mat = renderer.material;

			mat.SetColor ("_EmissionColor", GetColor () / 4);
			mat.EnableKeyword ("_EMISSION");
		}

		//Debug.Log ("Block state changed from " + state + " to " + new_state);
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
		Dictionary<int,Block> collisions = new Dictionary<int,Block> ();

		if (is_base == false) {
			collisions.Add (this.GetID(), this);

			foreach (Vector3 dir in directions) {
				RaycastHit hit;

				if (Physics.Raycast (gameObject.transform.position, dir, out hit, 0.51f)) {
					if (hit.transform.gameObject.name == gameObject.name) {
						Block block = tetris.GetBlock (hit.transform.gameObject);
						if (block.is_base == false) {
							collisions.Add (block.GetID(), block);
						}
					}
				}
			}
		}

		return new List<Block> (collisions.Values);
	}

	public List<Block> GetHorizontalCollisions () {
		Vector3[] horizontal = { Vector3.left, Vector3.right };
		return GetCollisionsList (horizontal);
	}

	public List<Block> GetVerticalCollisions () {
		Vector3[] vertical = { Vector3.up, Vector3.down };
		return GetCollisionsList (vertical);
	}

	//
	// Movement
	//

	public void MoveUp (float speed) {
		gameObject.transform.position += Vector3.up * speed;
	}

	// Returns true when object underneath is hitten
	public bool MoveDown (float speed) {

		Vector3 new_position = gameObject.transform.position + (Vector3.down * speed);
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
		gameObject.transform.Rotate (200 * Vector3.down * Time.deltaTime);
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

	private Tetris tetris;

	public Cursor (GameObject prefab)
	{
		gameObject = GameObject.Instantiate (prefab) as GameObject;
		gameObject.name = prefab.name;

		leftCursor = gameObject.transform.FindChild ("Left").gameObject;
		rightCursor = gameObject.transform.FindChild ("Right").gameObject;

		tetris = GameObject.Find ("Main Camera").GetComponent<Tetris> ();
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

			Block leftBlock = GetBlock (leftCursor);
			Block rightBlock = GetBlock (rightCursor);

			// Base blocks should not be swapped
			if ((leftBlock != null && leftBlock.is_base) || 
		        (rightBlock != null && rightBlock.is_base))
				break;

			swaped = true;

			if (leftBlock != null && rightBlock != null)
				SwapBlocks (leftBlock.gameObject, rightBlock.gameObject);
			else if (leftBlock != null)
				MoveBlock (leftBlock.gameObject, Vector3.right);
			else if (rightBlock != null)
				MoveBlock (rightBlock.gameObject, Vector3.left);
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

	Block GetBlock (GameObject cursor) {
		Block block = null;
		RaycastHit hit;
		
		if (Physics.Raycast (cursor.transform.position, Vector3.forward, out hit)) {
			block = tetris.GetBlock (hit.transform.gameObject);
		}
		
		return block;
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
		if (WithinBoundaries (direction)) {
			gameObject.transform.position += direction;
		}
	}

	bool WithinBoundaries (Vector3 direction)
	{
		Vector3 new_position = gameObject.transform.position + direction;

		return ((new_position.x >= tetris.min_x) && (new_position.x < tetris.max_x) &&
		        (new_position.y > tetris.min_y + 1) && (new_position.y < tetris.max_y));
	}

	public void MoveUp (float speed) {
		gameObject.transform.position += Vector3.up * speed;
	}
}
