using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class gameplay : MonoBehaviour
{
	public GridLayoutGroup grid = null;
	public AccessibleUIGroupRoot container = null;
	public Text m_MovesLabel;
	public UAP_BaseElement m_MovesLabel_Access = null;
	public Text[] m_GoalsLabel = null;
	public Image[] m_GoalsImages = null;
	public GameObject[] m_GoalsCheckmarks = null;
	public GameObject[] m_GoalsHighlightPos = null;
	public GameObject m_SelectionFrame = null;
	//	public UAP_BaseElement m_GoalsLabel_Access = null;
	public AudioSource m_SFXPlayer = null;
	public Image m_SoundToggle = null;
	public Sprite m_SoundOn = null;
	public Sprite m_SoundOff = null;
	public AudioSource m_MusicPlayer = null;

	public AudioClip m_ActiveTile = null;
	public AudioClip m_SwapAborted = null;
	public AudioClip m_NoMatch3 = null;
	public AudioClip m_Match3 = null;
	public AudioClip m_GoalsMatch3 = null;
	public AudioClip m_FallingPieces = null;

	public GameObject m_LevelGoalHighlightEffect = null;

	public Sprite[] m_GemTextures = new Sprite[gridpanel.tileTypeCount];

	private int m_CellCountX = -1;
	private int m_CellCountY = -1;
	private List<gridpanel> m_GridTiles = new List<gridpanel>();

	// Base Data for size calculations
	public bool m_MakeSquares = true;
	private int m_BaseCellSize = 85;
	private int m_BaseCellCountX = 7;
	private int m_BaseCellCountY = 11;

	static public gameplay Instance = null;

	// gameplay
	private int m_MovesLeft = 15;
	private int m_MoveCount = 0;
	private float m_GameDuration = 0.0f;
	private List<int> m_Cleared = new List<int>();
	private List<int> m_LevelGoals = new List<int>();
	private int m_TileTypeCount = 0;
	private bool m_Paused = false;
	private string m_LevelGoalsString = "";

	private int m_MovesGained = 0;
	private gridpanel m_SelectedTile = null;
	private bool m_levelGoalUpdatedWithMove = false;

	static public int DifficultyLevel = 0;

	// swap previewing
	private bool m_IsPreviewingSwap = false;
	private float m_SwapPreviewTimer = -1.0f;
	private float m_SwapPreviewDuration = 0.1f;
	private int m_PreviewIndex1 = -1;
	private int m_PreviewIndex2 = -1;
	private Vector3 m_Previewposition1 = new Vector3();
	private Vector3 m_Previewposition2 = new Vector3();

	//////////////////////////////////////////////////////////////////////////

	gameplay()
	{
		Instance = this;
	}

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		EnableMusic(PlayerPrefs.GetInt("Music_Enabled", 1) == 1);
	}

	//////////////////////////////////////////////////////////////////////////

	public Sprite GetTileTypeSprite(int tileType)
	{
		return m_GemTextures[tileType];
	}


	//////////////////////////////////////////////////////////////////////////

	public void InitBoard(int countX, int countY, int tiletypeCount, int moveCount, List<int> levelGoals)
	{
		UAP_AccessibilityManager.BlockInput(true);
		m_SelectionFrame.SetActive(false);

		m_CellCountX = countX;
		m_CellCountY = countY;
		m_MoveCount = 0;
		m_GameDuration = 0.0f;
		m_TileTypeCount = tiletypeCount;

		for (int i = 0; i < m_GoalsLabel.Length; ++i)
			m_GoalsLabel[i].gameObject.SetActive(false);
		for (int i = 0; i < m_GoalsImages.Length; ++i)
			m_GoalsImages[i].gameObject.SetActive(false);
		for (int i = 0; i < m_GoalsImages.Length; ++i)
			m_GoalsCheckmarks[i].SetActive(false);

		m_LevelGoals.Clear();
		m_LevelGoals = levelGoals;
		m_Cleared.Clear();
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
			m_Cleared.Add(0);

		// Remove all that is on the board now
		while (transform.childCount > 0)
		{
			Transform go = transform.GetChild(0);
			go.SetParent(null);
			DestroyImmediate(go.gameObject);
		}
		m_GridTiles.Clear();

		// Calculate grid element sizes
		Vector2 newCellSize = new Vector2(0, 0);
		newCellSize.x = (m_BaseCellSize / (float)countX) * m_BaseCellCountX;
		newCellSize.y = (m_BaseCellSize / (float)countY) * m_BaseCellCountY;

		// Ensure an equal tile size by choosing the smaller one for both axes
		if (m_MakeSquares)
		{
			if (newCellSize.x < newCellSize.y)
				newCellSize.y = newCellSize.x;
			else
				newCellSize.x = newCellSize.y;
		}

		grid.cellSize = newCellSize;
		grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		grid.constraintCount = countX;

		// Instantiate correct number of elements
		GameObject prefab = Resources.Load("Panel") as GameObject;
		int count = countX * countY;
		for (int i = 0; i < count; ++i)
		{
			GameObject newPanel = Instantiate(prefab);
			newPanel.transform.SetParent(transform, false);
			newPanel.GetComponent<gridpanel>().SetIndex(i, countX);
			m_GridTiles.Add(newPanel.GetComponent<gridpanel>());
		}

		m_SelectedTile = null;

		m_MovesLeft = moveCount;
		UpdateMoveLabel();
		UpdateLevelGoalsLabels();

		RandomizeTiles();

		SaveGameState();

		// Insert level goal
		int goalCount = 0;
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
		{
			if (m_LevelGoals[i] > 0)
			{
				m_GoalsImages[goalCount].gameObject.SetActive(true);
				m_GoalsImages[goalCount].sprite = GetTileTypeSprite(i);
				++goalCount;
			}
		}
		string levelGoalsString = "Level Goals: "/*m_GoalsLabel_Access.m_Text*/;
		int counter = 0;
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
		{
			if (m_LevelGoals[i] < 0)
				continue;

			int cleared = m_Cleared[i];
			if (cleared > m_LevelGoals[i])
				cleared = m_LevelGoals[i];

			++counter;
			if (m_LevelGoalsString.Length > 0)
				levelGoalsString += "\n ";
			if (counter == goalCount && goalCount > 1)
				levelGoalsString += "And ";
			levelGoalsString += m_LevelGoals[i].ToString("0") + " " + gridpanel.GetTileTypeName(i) + " gems, ";
		}
		levelGoalsString += "need to be destroyed.";


		UAP_AccessibilityManager.Say("Game Started. Double Tap with two fingers to pause the game.", false);
		UAP_AccessibilityManager.Say(levelGoalsString + " \nYou have " + m_MovesLeft.ToString("0") + " moves remaining.");
		UAP_AccessibilityManager.Say("Tap once with two fingers to repeat the level goals.");
		UAP_AccessibilityManager.BlockInput(false);
	}

	//////////////////////////////////////////////////////////////////////////

	void Start()
	{
		UAP_AccessibilityManager.RegisterOnPauseToggledCallback(OnUserPause);
		UAP_AccessibilityManager.RegisterOnTwoFingerSingleTapCallback(OnRepeatLevelGoals);

		// TODO: This would come from the main menu or somewhere
		// There should be three levels of difficulty
		// With 5, 6 and 7 tile types
		// And different start move amounts

		// EASY SETTINGS
		// DifficultyLevel == 0
		int tileTypeCount = 5;
		int startMoveCount = 10;
		int goalCount = 1;
		int upperGoalRange = 15;

		if (DifficultyLevel == 1)
		{
			tileTypeCount = 6;
			startMoveCount = 15;
			goalCount = 2;
			upperGoalRange = 20;
		}
		else if (DifficultyLevel == 2)
		{
			tileTypeCount = 7;
			startMoveCount = 20;
			goalCount = 3;
			upperGoalRange = 30;
		}

		List<int> levelGoals = new List<int>();
		bool levelGoalsValid = false;

		levelGoals.Clear();
		for (int c = 0; c < gridpanel.tileTypeCount; ++c)
			levelGoals.Add(-1);

		while (!levelGoalsValid)
		{
			int goalCounter = 0;

			for (int c = 0; c < tileTypeCount; ++c)
			{
				if (levelGoals[c] < 0 && Random.Range(0, 5) < 3)
				{
					int val = Random.Range(5, upperGoalRange);
					levelGoals[c] = val;
					//Debug.Log("Adding Level Goal " + val + " " + gridpanel.GetTileTypeName(c));
					++goalCounter;
					if (goalCounter == goalCount)
						break;
				}
			}

			levelGoalsValid = (goalCounter == goalCount);
		}
		InitBoard(6, 6, tileTypeCount, startMoveCount, levelGoals);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnRepeatLevelGoals()
	{
		UAP_AccessibilityManager.Say(m_LevelGoalsString);
		UAP_AccessibilityManager.Say(m_MovesLabel_Access.m_Text);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnUserPause()
	{
		// Toggle Pause
		if (!m_Paused)
		{
			m_Paused = true;
			m_MusicPlayer.Pause();
			transform.parent.parent.gameObject.SetActive(false);
			Instantiate(Resources.Load("Pause Menu"));
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void Update()
	{
		m_GameDuration += Time.unscaledDeltaTime;

		// Update swap preview, if any
		if (m_IsPreviewingSwap)
		{
			m_SwapPreviewTimer -= Time.unscaledDeltaTime;
			float scale = 1.0f - (m_SwapPreviewTimer / m_SwapPreviewDuration);
			gridpanel tile1 = GetGridTile(m_PreviewIndex1);
			gridpanel tile2 = GetGridTile(m_PreviewIndex2);
			tile1.m_GemImage.transform.position = Vector3.Lerp(m_Previewposition1, m_Previewposition2, scale);
			tile2.m_GemImage.transform.position = Vector3.Lerp(m_Previewposition2, m_Previewposition1, scale);
		}
	}

	private int GetRandomTile()
	{
		return Random.Range(0, m_TileTypeCount);
	}

	//////////////////////////////////////////////////////////////////////////

	void RandomizeTiles()
	{
		for (int i = 0; i < m_GridTiles.Count; ++i)
		{
			m_GridTiles[i].SetTileType(GetRandomTile());
		}

		// Make sure that there are no immediate match3s
		List<int> matchIndices = null;
		while (null != (matchIndices = FindMatch3()))
		{
			// Randomize the tiles in the indices
			foreach (int index in matchIndices)
				m_GridTiles[index].SetTileType(GetRandomTile());
		}

		// Save current state when all is done
		SaveGameState();
	}

	//////////////////////////////////////////////////////////////////////////

	Vector2 ConvertIndexToXYCoordinates(int index)
	{
		Vector2 xyCoords = new Vector2();
		xyCoords.y = Mathf.FloorToInt(index / (float)m_CellCountX);
		xyCoords.x = Mathf.FloorToInt(index - (xyCoords.y * m_CellCountX));
		return xyCoords;
	}

	private void AbortSelection()
	{
		// Deselect, stop sound
		m_SFXPlayer.Stop();
		m_SFXPlayer.PlayOneShot(m_SwapAborted);

		CancelPreview();

		UAP_AccessibilityManager.Say("Swap canceled.", false, false, UAP_AudioQueue.EInterrupt.All);

		m_SelectionFrame.SetActive(false);
		m_SelectedTile = null;

		return;
	}

	//////////////////////////////////////////////////////////////////////////

	public void ActivateTile(int index)
	{
		UAP_AccessibilityManager.Say("", false, false, UAP_AudioQueue.EInterrupt.All);

		Vector2 xyCoord = ConvertIndexToXYCoordinates(index);
		gridpanel tile = GetGridTile(index);

		// Check whether this tile is already selected
		if (tile == null || m_SelectedTile == tile)
		{
			AbortSelection();
			return;
		}

		// Is this the first tile to be activated or the second?
		if (m_SelectedTile == null)
		{
			// Select, start sound
			m_SFXPlayer.clip = m_ActiveTile;
			m_SFXPlayer.loop = true;
			m_SFXPlayer.Play();

			m_SelectedTile = tile;
			m_SelectionFrame.SetActive(true);
			m_SelectionFrame.transform.position = m_SelectedTile.transform.position;

			return;
		}

		// If second, check whether this tile is a neighbor to the first
		Vector2 selectedXY = ConvertIndexToXYCoordinates(m_SelectedTile.GetIndex());
		Vector2 diff = selectedXY - xyCoord;
		// Is it too far away?
		if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) > 1.0f)
		{
			AbortSelection();
			return;
		}

		// Temporarily block input in the AM
		UAP_AccessibilityManager.BlockInput(true);

		// Swap the tiles
		CancelPreview(true);
		int tileTypeSelected = m_SelectedTile.GetTileType();
		m_SelectedTile.SetTileType(tile.GetTileType());
		tile.SetTileType(tileTypeSelected);
		m_SelectedTile = null;
		m_SelectionFrame.SetActive(false);

		// Stop looping sound
		m_SFXPlayer.Stop();

		m_MovesGained = 0;
		++m_MoveCount;
		m_levelGoalUpdatedWithMove = false;
		EvaluateBoard();
	}

	//////////////////////////////////////////////////////////////////////////

	private gridpanel GetGridTile(int index)
	{
		if (index < 0 || index >= m_GridTiles.Count)
			return null;

		return m_GridTiles[index];
	}

	//////////////////////////////////////////////////////////////////////////

	void UpdateMoveLabel()
	{
		m_MovesLabel.text = "Moves: " + m_MovesLeft.ToString("0");
		m_MovesLabel_Access.m_Text = m_MovesLeft.ToString("0") + " moves left.";
	}

	//////////////////////////////////////////////////////////////////////////

	void UpdateLevelGoalsLabels()
	{
		int goalCounter = 0;

		m_LevelGoalsString = "Level Goals: \n";
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
		{
			if (m_LevelGoals[i] < 0)
				continue;

			m_GoalsLabel[goalCounter].gameObject.SetActive(true);
			m_GoalsImages[goalCounter].gameObject.SetActive(true);

			int cleared = m_Cleared[i];
			if (cleared > m_LevelGoals[i])
				cleared = m_LevelGoals[i];

			m_GoalsLabel[goalCounter].text = cleared.ToString("0") + "/" + m_LevelGoals[i].ToString("0");
			m_GoalsLabel[goalCounter].GetComponent<UAP_BaseElement>().m_Text = gridpanel.GetTileTypeName(i) + ": " + cleared.ToString("0") + " of " + m_LevelGoals[i].ToString("0");

			if (cleared == m_LevelGoals[i])
				m_GoalsCheckmarks[goalCounter].SetActive(true);

			if (m_LevelGoalsString.Length > 0)
				m_LevelGoalsString += "\n";
			m_LevelGoalsString += gridpanel.GetTileTypeName(i) + ": " + cleared.ToString("0") + " of " + m_LevelGoals[i].ToString("0") + ". \n";

			++goalCounter;

		}

		//m_GoalsLabel.text = "Moves: " + m_MovesLeft.ToString("0");
		//m_GoalsLabel_Access.m_Text = m_MovesLeft.ToString("0") + " moves left.";
	}

	//////////////////////////////////////////////////////////////////////////

	private int GetLevelGoalIndex(int tileType)
	{
		int counter = 0;
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
		{
			if (m_LevelGoals[i] < 0)
				continue;

			if (i == tileType)
				return counter;

			++counter;
		}

		return -1;
	}

	//////////////////////////////////////////////////////////////////////////

	private void EvaluateBoard()
	{
		// Evaluate board for possible match3s
		List<int> match = FindMatch3();
		if (match != null)
		{
			int matchTileType = m_GridTiles[match[0]].GetTileType();

			bool wasLevelGoal = false;
			if (m_LevelGoals[matchTileType] > 0 && m_LevelGoals[matchTileType] > m_Cleared[matchTileType])
			{
				m_levelGoalUpdatedWithMove = true;
				wasLevelGoal = true;
				m_SFXPlayer.PlayOneShot(m_GoalsMatch3);

				// Trigger Highlight effect
				int goalIndex = GetLevelGoalIndex(matchTileType);
				if (goalIndex >= 0)
				{
					GameObject highlight = Instantiate(m_LevelGoalHighlightEffect) as GameObject;
					highlight.transform.SetParent(m_GoalsHighlightPos[goalIndex].transform, false);
					Destroy(highlight, 1.0f);
				}
				else
					Debug.LogWarning("Couldn't find level goal index");
				UpdateLevelGoalsLabels();
			}
			else
				m_SFXPlayer.PlayOneShot(m_Match3);

			m_Cleared[matchTileType] += match.Count;

			UAP_AccessibilityManager.Say("Matched " + match.Count + " " + m_GridTiles[match[0]].GetTileTypeName() + " gems.");
			if (wasLevelGoal)
			{
				int left = m_LevelGoals[matchTileType] - m_Cleared[matchTileType];
				if (left > 0)
					UAP_AccessibilityManager.Say("Need " + left.ToString("0") + " more.");
				else
					UAP_AccessibilityManager.Say(m_GridTiles[match[0]].GetTileTypeName() + " level goal completed.");
			}
			if (match.Count > 3)
			{
				m_MovesGained += match.Count - 1;
			}

			// Remove matching tiles
			GameObject burstPrefab = Resources.Load("Burst") as GameObject;
			foreach (int index in match)
			{
				m_GridTiles[index].SetTileType(-1);
				GameObject newBurst = Instantiate(burstPrefab);
				newBurst.transform.SetParent(transform.parent, true);
				newBurst.transform.position = m_GridTiles[index].transform.position;
				Destroy(newBurst, 0.45f);
			}

			// Find more matches
			float waitTime = 0.25f; // change to sounds and voice duration
			Invoke("EvaluateBoard", waitTime);
		}
		else // no match found
		{
			// Let remaining tiles drop down
			//Debug.LogError("Starting DropDown");
			Invoke("DropDownTiles", 0.05f);

			// NOTE: drop down could cause more than one new matches to appear.
			// Before dropping down, the board should be cleared of ALL matches.
			// Everything else would be unfair.
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void DropDownTiles()
	{
		// Move tiles down.
		bool movedTiles = false;
		for (int x = 0; x < m_CellCountX; ++x)
		{
			for (int y = m_CellCountY - 1; y >= 0; --y)
			{
				int index = GetIndex(x, y);
				if (m_GridTiles[index].GetTileType() == -1)
				{
					movedTiles = true;

					// Top Row gets a new random tile
					if (y == 0)
					{
						m_GridTiles[index].SetTileType(GetRandomTile());
						continue;
					}

					// Move tiles from above until the cell is no longer -1
					for (int jY = y - 1; jY >= 0; --jY)
					{
						int jIndex = GetIndex(x, jY);
						int newTileType = m_GridTiles[jIndex].GetTileType();
						if (newTileType < 0 && jY == 0)
						{
							m_GridTiles[index].SetTileType(GetRandomTile());
							break;
						}
						else if (newTileType >= 0)
						{
							m_GridTiles[index].SetTileType(newTileType);
							m_GridTiles[jIndex].SetTileType(-1);
							break;
						}

						// continue searching upwards otherwise
					}

				}
			}
		}

		// If ANY tiles were moved, call EvaluateBoard again
		// If not, finish up (see below)
		if (movedTiles)
		{
			// Play sound(s) for moving
			m_SFXPlayer.PlayOneShot(m_FallingPieces);

			float waitTime = 0.25f; // change to sounds and voice duration
			//Debug.LogError("DropDown Done");
			Invoke("EvaluateBoard", waitTime);
		}
		else
		{
			// Do this only if the dropdown found no tiles with -1
			FinishBoardEvaluation();
		}

	}

	//////////////////////////////////////////////////////////////////////////

	int GetIndex(int x, int y)
	{
		return x + y * m_CellCountX;
	}

	//////////////////////////////////////////////////////////////////////////

	private void FinishBoardEvaluation()
	{
		// Only deduct a move if no level goals gems were matched
		if (!m_levelGoalUpdatedWithMove)
			--m_MovesLeft;
		m_levelGoalUpdatedWithMove = false;

		// Award moves
		m_MovesLeft += m_MovesGained;
		if (m_MovesGained > 0)
			UAP_AccessibilityManager.Say("Gained " + m_MovesGained.ToString("0") + " move" + ((m_MovesGained > 1) ? "s." : "."));
		UAP_AccessibilityManager.Say(m_MovesLeft.ToString("0") + " moves left.");
		m_MovesGained = 0;
		UpdateMoveLabel();
		UpdateLevelGoalsLabels();

		// Check whether player has any moves left and end level if not
		if (m_MovesLeft <= 0)
		{
			gameover.GameWon = false;
			gameover.GameDuration = m_GameDuration;
			gameover.MoveCount = m_MoveCount;
			DestroyMyself();
			Instantiate(Resources.Load("Game Over Screen"));

			return;
		}

		// Check whether the level goals are completed
		bool levelWon = true;
		for (int i = 0; i < gridpanel.tileTypeCount; ++i)
		{
			if (m_LevelGoals[i] < 0)
				continue;

			if (m_Cleared[i] < m_LevelGoals[i])
			{
				levelWon = false;
				break;
			}
		}
		if (levelWon)
		{
			gameover.GameWon = true;
			gameover.GameDuration = m_GameDuration;
			gameover.MoveCount = m_MoveCount;
			DestroyMyself();
			Instantiate(Resources.Load("Game Over Screen"));

			return;
		}

		// Save current state when all is done
		SaveGameState();

		// Unblock input in the AM
		UAP_AccessibilityManager.BlockInput(false);
	}

	//////////////////////////////////////////////////////////////////////////

	void SaveGameState()
	{
		// TODO: Save current board and progress
	}

	//////////////////////////////////////////////////////////////////////////

	private List<int> FindMatch3()
	{
		// Evaluate the board for Match3s
		// Terminate on the first match found
		List<int> indices = new List<int>();

		// Go through each piece and check whether it is part of a contiguous group of 3 or more
		for (int i = 0; i < m_GridTiles.Count; ++i)
		{
			// Matches can be vertical or horizontal
			Vector2 xyCoords = ConvertIndexToXYCoordinates(i);
			int tileType = m_GridTiles[i].GetTileType();
			if (tileType < 0)
				continue;

			indices.Clear();
			indices.Add(i);
			int counter = 1;

			// Test horizontally to the right
			for (int jX = (int)xyCoords.x + 1; jX < m_CellCountX; ++jX)
			{
				int jIndex = jX + (int)xyCoords.y * m_CellCountX;
				int type = m_GridTiles[jIndex].GetTileType();
				if (type == tileType)
				{
					indices.Add(jIndex);
					++counter;
				}
				else
					break;
			}
			if (counter >= 3)
			{
				// Found match, but now we need to vertically for each tile in the match
				int indexCount = indices.Count;
				for (int idx = 0; idx < indexCount; ++idx)
				{
					int index = indices[idx];
					Vector2 xyC = ConvertIndexToXYCoordinates(index);
					if (GetTileType(xyC.x, xyC.y + 1) == tileType && GetTileType(xyC.x, xyC.y + 2) == tileType)
					{
						indices.Add(index + m_CellCountX);
						indices.Add(index + m_CellCountX + m_CellCountX);
					}
				}

				// Found Match, terminate here
				string matchString = "";
				foreach (int m in indices)
				{
					matchString += ConvertIndexToXYCoordinates(m) + " ";
				}
				//Debug.Log("Found horizontal Match3 : " + matchString);
				return indices;
			}

			indices.Clear();
			indices.Add(i);
			counter = 1;
			// Test vertically downwards
			for (int jY = (int)xyCoords.y + 1; jY < m_CellCountY; ++jY)
			{
				int jIndex = (int)xyCoords.x + jY * m_CellCountX;
				int type = m_GridTiles[jIndex].GetTileType();
				if (type == tileType)
				{
					indices.Add(jIndex);
					++counter;
				}
				else
					break;

			}
			if (counter >= 3)
			{
				// Found match, but now we need to horizontally for each tile in the match
				int indexCount = indices.Count;
				for (int idx = 0; idx < indexCount; ++idx)
				{
					int index = indices[idx];

					// test left, right, and left-right
					bool added1 = false;
					bool added2 = false;
					Vector2 xyC = ConvertIndexToXYCoordinates(index);
					if (GetTileType(xyC.x - 2, xyC.y) == tileType && GetTileType(xyC.x - 1, xyC.y) == tileType)
					{
						indices.Add(index - 2);
						indices.Add(index - 1);
					}
					if (GetTileType(xyC.x + 2, xyC.y) == tileType && GetTileType(xyC.x + 1, xyC.y) == tileType)
					{
						indices.Add(index + 2);
						indices.Add(index + 1);
					}
					if (!added1 || !added2)
					{
						if (GetTileType(xyC.x - 1, xyC.y) == tileType && GetTileType(xyC.x + 1, xyC.y) == tileType)
						{
							if (!added1)
								indices.Add(index - 1);
							if (!added2)
								indices.Add(index + 1);
						}
					}
				} // end of for loop

				// Found Match, terminate here
				string matchString = "";
				foreach (int m in indices)
				{
					matchString += ConvertIndexToXYCoordinates(m) + " ";
				}
				//Debug.Log("Found vertical Match3 : " + matchString);
				return indices;
			}
		}

		return null;
	}

	//////////////////////////////////////////////////////////////////////////

	int GetTileType(float xCoord, float yCoord)
	{
		if (xCoord < 0 || xCoord >= m_CellCountX)
			return -1;
		if (yCoord < 0 || yCoord >= m_CellCountY)
			return -1;

		int index = (int)xCoord + (int)yCoord * m_CellCountX;
		return m_GridTiles[index].GetTileType();
	}

	//////////////////////////////////////////////////////////////////////////

	public void AbortGame()
	{
		DestroyMyself();
		Instantiate(Resources.Load("Main Menu"));
	}

	//////////////////////////////////////////////////////////////////////////

	private void DestroyMyself()
	{
		DestroyImmediate(transform.parent.parent.gameObject);
	}

	//////////////////////////////////////////////////////////////////////////

	public void ResumeGame()
	{
		m_Paused = false;
		m_MusicPlayer.UnPause();
		transform.parent.parent.gameObject.SetActive(true);
	}

	//////////////////////////////////////////////////////////////////////////

	void OnDestroy()
	{
		UAP_AccessibilityManager.UnregisterOnPauseToggledCallback(OnUserPause);
		UAP_AccessibilityManager.UnregisterOnTwoFingerSingleTapCallback(OnRepeatLevelGoals);
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnSoundToggle()
	{
		EnableMusic(!m_MusicPlayer.enabled);
	}

	void EnableMusic(bool enable)
	{
		m_MusicPlayer.enabled = enable;
		m_SoundToggle.sprite = enable ? m_SoundOn : m_SoundOff;
		PlayerPrefs.SetInt("Music_Enabled", enable ? 1 : 0);
		PlayerPrefs.Save();
	}

	//////////////////////////////////////////////////////////////////////////

	public void PreviewDrag(int index1, int index2)
	{
		//Debug.Log("Starting preview");

		CancelPreview();

		gridpanel tile1 = GetGridTile(index1);
		gridpanel tile2 = GetGridTile(index2);
		if (tile1 == null || tile2 == null)
			return;

		m_IsPreviewingSwap = true;
		m_SwapPreviewTimer = m_SwapPreviewDuration;
		m_PreviewIndex1 = index1;
		m_PreviewIndex2 = index2;

		m_Previewposition1 = tile1.transform.position;
		m_Previewposition2 = tile2.transform.position;
	}

	//////////////////////////////////////////////////////////////////////////

	public void CancelPreview(bool swapSuccessful = false)
	{
		//Debug.Log("Cancelling preview");

		if (!m_IsPreviewingSwap)
			return;

		m_IsPreviewingSwap = false;

		//if (swapSuccessful)
		//  return;

		// Reset positions
		gridpanel tile1 = GetGridTile(m_PreviewIndex1);
		gridpanel tile2 = GetGridTile(m_PreviewIndex2);
		tile1.m_GemImage.transform.position = m_Previewposition1;
		tile2.m_GemImage.transform.position = m_Previewposition2;
	}
}
