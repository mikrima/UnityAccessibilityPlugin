using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class gridpanel : MonoBehaviour
{
	public Image m_GemImage = null;
	public Sprite[] m_BGTexture = new Sprite[2];

	GameObject m_Tile = null;
	UAP_BaseElement m_AccessibilityHelper = null;
	//Text m_ButtonLabel = null;
	Button m_Button = null;
	
	int m_Index = -1;
	int m_xWidth = -1;
	//int m_X = -1;
	//int m_Y = -1;

	int m_TileType = -1;
	static string[] typeList = { "Ruby", "Blue", "Emerald", "Purple", "Gold", "Orange", "Crystal" };
	static public int tileTypeCount = 7;
	public Sprite[] m_GemTextures = new Sprite[tileTypeCount];
	//Color[] colorList = { Color.red, Color.blue, Color.green, Color.magenta, Color.yellow, new Color(1, 0.5f, 0), Color.gray };
	int m_BGType = 0;

	// Previewing
	EVecDir m_LastPreviewDir = EVecDir.None;

	//////////////////////////////////////////////////////////////////////////

	enum EVecDir
	{
		None,
		Up,
		Left,
		Right,
		Down
	}

	//////////////////////////////////////////////////////////////////////////

	public string GetTileTypeName()
	{
		if (m_TileType >= 0 && m_TileType < typeList.Length)
			return typeList[m_TileType];

		return "invalid";
	}

	//////////////////////////////////////////////////////////////////////////

	static public string GetTileTypeName(int tileType)
	{
		if (tileType >= 0 && tileType < typeList.Length)
			return typeList[tileType];

		return "invalid";
	}

	//////////////////////////////////////////////////////////////////////////

	void SetBGType(int bgIndex = 0)
	{
		m_BGType = bgIndex;
		if (m_Button != null)
			m_Button.GetComponent<Image>().sprite = m_BGTexture[bgIndex];
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetTileType(int tileType)
	{
		m_TileType = tileType;

		// Create new tile if needed
		if (m_Tile == null)
		{
			m_Tile = Instantiate(Resources.Load("Button")) as GameObject;
			m_Tile.transform.SetParent(transform, false);
			m_Tile.transform.SetAsFirstSibling();
			m_AccessibilityHelper = m_Tile.GetComponentInChildren<UAP_BaseElement>();
			m_AccessibilityHelper.m_TryToReadLabel = false;
			//m_ButtonLabel = m_Tile.GetComponentInChildren<Text>();
			m_Button = m_Tile.GetComponentInChildren<Button>();

			// React to both - button presses and pointer events
			m_Button.onClick.AddListener(OnButtonPress);
			EventTrigger trigger = m_Button.gameObject.AddComponent<EventTrigger>();
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.EndDrag;
				entry.callback.AddListener((data) => { OnDragEndDelegate((PointerEventData)data); });
				trigger.triggers.Add(entry);
			}
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Drag;
				entry.callback.AddListener((data) => { OnDragUpdateDelegate((PointerEventData)data); });
				trigger.triggers.Add(entry);
			}
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerDown;
				entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
				trigger.triggers.Add(entry);
			}
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerUp;
				entry.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
				trigger.triggers.Add(entry);
			}

			SetBGType(m_BGType);
		}

		if (tileType < 0)
		{
			m_AccessibilityHelper.m_Text = "none";
			//m_ButtonLabel.text = " ";
			//m_ButtonLabel.color = Color.black;
			m_GemImage.sprite = null;
			m_GemImage.gameObject.SetActive(false);
			//m_Button.gameObject.SetActive(false);
		}
		else
		{
			// Set info
			m_AccessibilityHelper.m_Text = GetTileTypeName();
			//m_ButtonLabel.text = " ";// GetTileTypeName().Substring(0, 1);
			//m_ButtonLabel.color = colorList[tileType];
			m_GemImage.sprite = m_GemTextures[tileType];
			m_GemImage.gameObject.SetActive(true);
			//m_Button.gameObject.SetActive(true);

			m_Button.gameObject.name = GetTileTypeName();
			gameObject.name = GetTileTypeName();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public int GetTileType()
	{
		return m_TileType;
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetIndex(int index, int xWidth)
	{
		m_Index = index;

		m_xWidth = xWidth;
		//m_Y = index / xWidth;
		//m_X = index % xWidth;

		if (xWidth % 2 == 1)
			SetBGType(index % 2);
		else
		{
			int temp = index;
			bool plusOne = true;
			while (temp >= xWidth)
			{
				plusOne = !plusOne;
				temp -= xWidth;
			}
			if (plusOne)
				++index;
			SetBGType(index % 2);
		}
	}

	public int GetIndex()
	{
		return m_Index;
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnPointerDownDelegate(PointerEventData data)
	{
		//Debug.Log("Down Received on " + GetTileTypeName() + " on " + gameObject.name);

		// Ignore these in accessibility mode
		if (UAP_AccessibilityManager.IsEnabled())
			return;

		// Tell the game grid that this tile was activated
		// The grid will take it from there
		gameplay.Instance.ActivateTile(m_Index);
	}

	public void OnPointerUpDelegate(PointerEventData data)
	{
		m_LastPreviewDir = EVecDir.None;

		// Ignore these in accessibility mode
		if (UAP_AccessibilityManager.IsEnabled())
			return;

		gridpanel panel = (data.pointerEnter != null ? gameObject.GetComponent<gridpanel>() : null);
		if (panel == this && !data.dragging)
		{
			//Debug.Log("Pointer Up, not dragging - on same tile " + GetTileTypeName());
			gameplay.Instance.ActivateTile(m_Index);
		}
	}

	public void OnDragUpdateDelegate(PointerEventData data)
	{
		// Ignore these in accessibility mode
		if (UAP_AccessibilityManager.IsEnabled())
			return;

		Vector2 direction = (data.position - data.pressPosition).normalized;
		EVecDir dir = GetVectorDirection(direction);

		if (IsSameTile(data.pointerEnter))
			dir = EVecDir.None;

		if (dir == m_LastPreviewDir)
			return;

		m_LastPreviewDir = dir;

		// Preview drag
		if (dir != EVecDir.None)
			gameplay.Instance.PreviewDrag(m_Index, GetNeighbourIndex(dir));
		else
			gameplay.Instance.CancelPreview();
	}

	public void OnDragEndDelegate(PointerEventData data)
	{
		// Ignore these in accessibility mode
		if (UAP_AccessibilityManager.IsEnabled())
			return;

		Vector2 direction = (data.position - data.pressPosition).normalized;
		EVecDir dir = GetVectorDirection(direction);

		//Debug.Log("End Drag - Press Position: " + data.pressPosition + " Position: " + data.position + " Direction: " + dir); 

		if (IsSameTile(data.pointerEnter))
		{
			gameplay.Instance.ActivateTile(m_Index);
			return;
		}

		gameplay.Instance.ActivateTile(GetNeighbourIndex(dir));
	}

	//////////////////////////////////////////////////////////////////////////

	private bool IsSameTile(GameObject pointerEnter)
	{
		bool wasDroppedOnTile = (pointerEnter != null && pointerEnter.transform.parent != null);

		//Debug.Log("Dropped on " + (data.pointerEnter != null ? data.pointerEnter.name : "") + " and " + ((data.pointerEnter != null && data.pointerEnter.transform.parent != null) ? data.pointerEnter.transform.parent.name : ""));

		if (wasDroppedOnTile)
		{
			gridpanel droppedOn = pointerEnter.transform.parent.gameObject.GetComponent<gridpanel>();
			if (droppedOn != null)
			{
				if (droppedOn == this)
				{
					return true;
				}
			}
		}

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	private int GetNeighbourIndex(EVecDir dir)
	{
		switch (dir)
		{
			case EVecDir.Up:
				return m_Index - m_xWidth;

			case EVecDir.Down:
				return m_Index + m_xWidth;

			case EVecDir.Left:
				return  m_Index - 1;

			case EVecDir.Right:
				return m_Index + 1;
		}

		return m_Index;
	}

	//////////////////////////////////////////////////////////////////////////
	
	EVecDir GetVectorDirection(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			if (vector.x > 0)
				return EVecDir.Right;
			else
				return EVecDir.Left;
		}
		else
		{
			if (vector.y > 0)
				return EVecDir.Up;
			else
				return EVecDir.Down;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	
	public void OnButtonPress()
	{
		// This is only relevant for Accessibility Mode
		if (!UAP_AccessibilityManager.IsEnabled())
			return;

		// Tell the game grid that this tile was activated
		// The grid will take it from there
		gameplay.Instance.ActivateTile(m_Index);
	}

}
