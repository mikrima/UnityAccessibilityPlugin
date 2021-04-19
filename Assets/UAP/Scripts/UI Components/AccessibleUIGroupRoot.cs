using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

[AddComponentMenu("Accessibility/UI/Accessible UI Group Root")]
public class AccessibleUIGroupRoot : MonoBehaviour
{
	/// <summary>
	/// Set this to true if this is a popup dialog or screen.
	/// This should be checked if this container is a popup that will partially overlay, but 
	/// not disable the current screen (i.e. not deactivated the GameObject). In other words, 
	/// the buttons and labels of the underlying screen might still be partially visible 
	/// while this is open.<br><br>
	/// Input to the other UI containers will be suspended while this is on the screen.<br><br>
	/// Leave at false for full screen menus that deactivate other screens.
	/// </summary>
	public bool m_PopUp = false;
	public bool m_AllowExternalJoining = false;

	/// <summary>
	/// Automatically read from top when this container is activated, useful for popups, cut-scenes etc
	/// </summary>
	public bool m_AutoRead = false;

	/// <summary>
	/// Higher numbers mean higher priority.
	/// </summary>
	public int m_Priority = 0;

	/// <summary>
	/// Optional: The name of this container.
	/// </summary>
	public string m_ContainerName = "";
	
	/// <summary>
	/// Uses the text inside m_ContainerName and requests translation.
	/// </summary>
	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey = false;

	/// <summary>
	/// If enabled, swiping up and down will select the UI element above or below - useful for grid based puzzles.
	/// The default behavior let's users jump between containers with up/down swipe as a means
	/// of quick navigation. DO NOT CHANGE this unless you app requires it, as you are taking away 
	/// an option for your users to navigate your app faster.<br><br>
	/// If you are making a grid based game, such as a puzzle, this is the option for you however. 
	/// Some card games might also need this type of navigation.
	/// </summary>
	public bool m_2DNavigation = false;

	/// <summary>
	/// User can not leave the current container with up/down/left/right swipes (2D Navigation only).
	/// Users can however still use Touch Explore (if enabled) to find other elements 
	/// on screen. This option should be used even more carefully than the 2D Navigation.<br><br>
	/// This option is useful for grid based puzzles to prevent the player from accidentally 
	/// swiping out of the playing area. A sound cue will play when the edge is reached.<br><br>
	/// Make sure to offer the player other means to leave the game, such as a Pause menu
	/// that can be opened by the Magic Tap etc. (See MagicGestures documentation on how to subscribe
	/// to gestures like that)
	/// </summary>
	public bool m_ConstrainToContainerUp = false;
	public bool m_ConstrainToContainerDown = false;
	public bool m_ConstrainToContainerLeft = false;
	public bool m_ConstrainToContainerRight = false;

	/// <summary>
	/// Touch Explore works on elements in this container - default is TRUE.
	/// You can optionally turn this off if you want to restrict the user to navigate 
	/// the elements in this container using swipes only.<br><br>
	/// This can be very helpful in grid based puzzles with 2D navigation, as 
	/// Touch Explore might inadvertently change a player's position in the grid
	/// when he touches the screen on accident.<br><br>
	/// The container itself can still be selected using Touch Explore.
	/// </summary>
	public bool m_AllowTouchExplore = true;

	// 	[Header("Optional")]
	// 	public AudioClip m_TextAsAudio = null;
	// 	public string m_ContainerName = "";
	// 	
	private bool m_HasStarted = false;
	private bool m_RefreshNextFrame = false;
	private bool m_ActivateContainerNextFrame = false;

	[Tooltip("This causes a 2 frame delay before the interface is accessible, but solves issues with screens that perform automatic UI elements ordering at start - such as any dynamically built UI, expanding scroll views, horizontal grids etc")]
	public bool m_DoubleCheckUIElementsPositions = true;
	private bool m_NeedsRefreshBeforeActivation = true; // helper variable to remember that we need to perform another refresh

	//////////////////////////////////////////////////////////////////////////

	public enum EUIElement
	{
		EUndefined = 0,
		EButton,
		ELabel,
		EToggle,
		ESlider,
		ETextEdit,
		EDropDown,
	};

	///

	public bool IsConstrainedToContainer(UAP_AccessibilityManager.ESDirection direction)
	{
		switch (direction)
		{
			case UAP_AccessibilityManager.ESDirection.ELeft:
				return m_ConstrainToContainerLeft;
			case UAP_AccessibilityManager.ESDirection.EUp:
				return m_ConstrainToContainerUp;
			case UAP_AccessibilityManager.ESDirection.ERight:
				return m_ConstrainToContainerRight;
			case UAP_AccessibilityManager.ESDirection.EDown:
				return m_ConstrainToContainerDown;
		}

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

	public static void GetAbsoluteAnchors(RectTransform t, out Vector2 anchorMin, out Vector2 anchorMax, out Vector2 centerPos, bool stopAtScrollView = false)
	{
		centerPos = t.TransformPoint(0.5f, 0.5f, 0.0f);

		// #UAP: Take care of offsets
		anchorMin = t.anchorMin;
		anchorMax = t.anchorMax;

		// #UAP: Abstract this into a GUI system independent function that will get the position and min/max corner points of the UI element frame.

		// #UAP: Transform points into world coordinate system, and afterwards transform it back into local space of the object with the
		// Accessiblity_Container component.
		// #UAP: If stopAtScrollView is active, use that RectTransform instead of the UIContainer transform.

		Vector2 scale = new Vector3(1.0f / (float)Screen.width, 1.0f / (float)Screen.height, 0.0f);
		Vector2 tanchorMin = t.TransformPoint(0.0f, 0.0f, 0.0f);
		Vector2 tanchorMax = t.TransformPoint(1.0f, 1.0f, 0.0f);
		tanchorMin.Scale(scale);
		tanchorMax.Scale(scale);
		//string output = "min max of " + t.gameObject.name + " is MIN: " + tanchorMin.ToString() + " and MAX: " + tanchorMax.ToString();

		//anchorMin = t.TransformPoint(0.0f, 0.0f, 0.0f);
		//anchorMax = t.TransformPoint(1.0f, 1.0f, 0.0f);
		//return;

		Transform parent = t.parent;
		while (parent != null)
		{
			if (parent.gameObject.GetComponent<Canvas>() && parent.parent == null)
				break;

			if (stopAtScrollView)
			{
				if (parent.gameObject.GetComponent<ScrollRect>() != null)
				{
					// Return early
					return;
				}
			}

			// Add up the positions
			RectTransform pTrans = parent.gameObject.GetComponent<RectTransform>();
			parent = parent.parent;
			if (pTrans == null)
				continue;

			//centerPos = pTrans.TransformPoint(centerPos);

			anchorMin.x = pTrans.anchorMin.x + anchorMin.x * (pTrans.anchorMax.x - pTrans.anchorMin.x);
			anchorMin.y = pTrans.anchorMin.y + anchorMin.y * (pTrans.anchorMax.y - pTrans.anchorMin.y);
			anchorMax.x = pTrans.anchorMin.x + anchorMax.x * (pTrans.anchorMax.x - pTrans.anchorMin.x);
			anchorMax.y = pTrans.anchorMin.y + anchorMax.y * (pTrans.anchorMax.y - pTrans.anchorMin.y);
		}

		//output += " and MIN: " + anchorMin.ToString() + " and MAX: " + anchorMax.ToString(); 
		//Debug.Log(output);
	}

	//////////////////////////////////////////////////////////////////////////

	public class Accessible_UIElement
	{
		public bool AllowsVoiceOver()
		{
			return m_Object.m_AllowVoiceOver;
		}

		//////////////////////////////////////////////////////////////////////////

		public bool ReadType()
		{
			return m_Object.m_ReadType;
		}

		//////////////////////////////////////////////////////////////////////////

		public void CalculatePositionOrder(UAP_BaseElement uiElement, int backupIndex)
		{
			GameObject obj = uiElement.gameObject;

			// If no parent transform is set, use the next higher up group root if possible
			if (uiElement.m_ManualPositionParent == null)
			{
				AccessibleUIGroupRoot groupRoot = uiElement.GetUIGroupContainer();
				uiElement.m_ManualPositionParent = (groupRoot != null ? groupRoot.gameObject : null);
			}

			bool hasManualOrder = uiElement.m_ManualPositionOrder >= 0 && uiElement.m_ManualPositionParent != null;
			if (hasManualOrder)
				obj = uiElement.m_ManualPositionParent;

			Vector2 anchorMin = new Vector2();
			Vector2 anchorMax = new Vector2();
			Vector2 centerPos = new Vector2();
			bool gotValidValues = false;
			bool secondaryOrderNeeded = false;

			// Calculation for Unity UI (uUI)
			RectTransform t = obj.GetComponent<RectTransform>();
			if (t != null)
			{
				// Does this ui element lie inside a scroll rect?
				Transform pt = (Transform)t;
				RectTransform primaryOrderBase = t;
				while (pt.parent != null)
				{
					if (pt.parent.gameObject.GetComponent<ScrollRect>() != null)
					{
						primaryOrderBase = pt.parent.gameObject.GetComponent<RectTransform>();
						secondaryOrderNeeded = true;
						break;
					}
					pt = pt.parent;
				}

				// Calculate up until the Canvas is found. Otherwise nested objects won't be correctly sorted
				GetAbsoluteAnchors(primaryOrderBase, out anchorMin, out anchorMax, out centerPos);
				gotValidValues = true;
			}

#if ACCESS_NGUI
			UIWidget widget = obj.GetComponent<UIWidget>();
			if (widget == null)
				widget = obj.GetComponentInChildren<UIWidget>();

			Camera uiCam = (widget != null) ? NGUITools.FindCameraForLayer(widget.gameObject.layer) : null;
			if (widget != null && uiCam != null)
			{
				// Transform from world to screen coordinates, if there is a main camera?
				centerPos = uiCam.WorldToScreenPoint(widget.transform.position);
				anchorMin = uiCam.WorldToScreenPoint(widget.worldCorners[0]);
				anchorMax = uiCam.WorldToScreenPoint(widget.worldCorners[1]);


				gotValidValues = true;
			}
#endif


			if (gotValidValues)
			{
				/*
				m_Pos = anchorMin + 0.5f * (anchorMax - anchorMin);
				m_PositionOrder = (int)(10.0f * (m_Pos.x + (1000.0f * (1.0f - m_Pos.y))));
				if (hasManualOrder)
					m_PositionOrder += uiElement.m_ManualPositionOrder;
				*/

				Vector3 wPos = centerPos;
				m_Pos.x = wPos.x;
				m_Pos.y = wPos.y;
				m_Pos.Scale(new Vector2(1.0f / (float)Screen.width, 1.0f / (float)Screen.height));
				m_PositionOrder = (int)(10.0f * (m_Pos.x + (1000.0f * (1.0f - m_Pos.y))));
				if (hasManualOrder)
					m_PositionOrder += uiElement.m_ManualPositionOrder;

				// For 2D Navigation this is needed to adjust for the aspect ratio!
				m_Pos.Scale(new Vector2(Screen.width, Screen.height));

				// For the secondary order always use the actual game object (regardless of manual order parent reference)
				obj = uiElement.gameObject;

				// Do we need a secondary order?
				if (secondaryOrderNeeded)
				{
					// TODO: This might need a different calculation in NGUI

					Vector2 pos = obj.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.0f));
					m_SecondaryOrder = (int)(10.0f * (pos.x + (1000.0f * (1.0f - pos.y))));
				}
			}
			else
			{
				// 3D Objects obviously wouldn't have any UI transforms
				if (uiElement is UAP_BaseElement_3D)
				{
					if (uiElement.m_ManualPositionOrder >= 0)
						m_PositionOrder = uiElement.m_ManualPositionOrder;
					else
						m_PositionOrder = backupIndex;
					m_Pos = obj.transform.position;
				}
				else
				{
					Debug.LogWarning("[Accessibility] Could not find any UI transforms on UI element " + obj.name + " that the accessibility plugin can understand - ordering UI elements by manual position index (if present) or order of initialization.");
					if (uiElement.m_ManualPositionOrder >= 0)
						m_PositionOrder = uiElement.m_ManualPositionOrder;
					else
						m_PositionOrder = backupIndex;
					m_Pos = obj.transform.position;
				}
			}

		}

		//////////////////////////////////////////////////////////////////////////

		public Accessible_UIElement(UAP_BaseElement item, EUIElement type, int index)
		{
			m_Type = type;
			m_Object = item;
			GameObject targetGO = item.GetTargetGameObject();

			// Is this item inside a scroll view?
			if (targetGO.GetComponentInParent<ScrollRect>() != null)
				item.m_IsInsideScrollView = true;
#if ACCESS_NGUI
			if (targetGO.GetComponentInParent<UIScrollView>() != null)
				item.m_IsInsideScrollView = true;
#endif

			CalculatePositionOrder(item, index);
			m_Object.m_PositionOrder = m_PositionOrder;
			m_Object.m_SecondaryOrder = m_SecondaryOrder;
			m_Object.m_Pos = m_Pos;
		}

		public EUIElement m_Type = EUIElement.EUndefined;
		public UAP_BaseElement m_Object = null;
		public Vector2 m_Pos = new Vector2(0, 0);
		public int m_PositionOrder = -1;
		public int m_SecondaryOrder = 0;
	}

	//////////////////////////////////////////////////////////////////////////

	List<Accessible_UIElement> m_AllElements = new List<Accessible_UIElement>();
	UAP_BaseElement m_CurrentStartItem = null;
	int m_CurrentItemIndex = 0;

	//////////////////////////////////////////////////////////////////////////

	public void CheckForRegister(UAP_BaseElement item)
	{
		if (!m_HasStarted)
			return;

		// Is this item not yet known? Refresh List or ignore?
		bool refreshNeeded = true;
		foreach (Accessible_UIElement element in m_AllElements)
		{
			if (element.m_Object == item)
			{
				refreshNeeded = false;

				// But this item might want to be the first start item
				if (item.m_ForceStartHere && item.IsElementActive())
					m_CurrentStartItem = item;

				break;
			}
		}
		if (refreshNeeded)
			RefreshContainer();
		// 		else
		// 			Debug.Log("Item " + item.name + " already known. Refresh not needed.");
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetAsStartItem(UAP_BaseElement item)
	{
		foreach (Accessible_UIElement element in m_AllElements)
		{
			if (element.m_Object == item)
			{
				// But this item might want to be the first start item
				if (item.m_ForceStartHere && item.IsElementActive())
				{
					m_CurrentStartItem = item;
					m_RefreshNextFrame = true;
				}

				return;
			}
		}

		// Item not yet known, so let's register it
		Register_Item(item);
	}

	//////////////////////////////////////////////////////////////////////////

	public void UnRegister(UAP_BaseElement item)
	{
		foreach (Accessible_UIElement element in m_AllElements)
		{
			if (element.m_Object == item)
			{
				//Debug.Log("Unregistering " + item);
				m_AllElements.Remove(element);
				UAP_AccessibilityManager.ElementRemoved(element);
				return;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void RefreshContainer()
	{
		bool needsResetToStart = false;
		if (m_CurrentStartItem != null && m_CurrentItemIndex >= 0 && m_AllElements[m_CurrentItemIndex].m_Object == m_CurrentStartItem)
			needsResetToStart = true;

		int prevCount = m_AllElements.Count;
		m_AllElements.Clear();

		// Search through all children AND objects on the same GameObject
		List<UAP_BaseElement> elements = new List<UAP_BaseElement>();
		//if (gameObject.GetComponent<UAP_BaseElement>() != null)
		//	elements.Add(gameObject.GetComponent<UAP_BaseElement>());
		elements.AddRange(gameObject.GetComponentsInChildren<UAP_BaseElement>());

		foreach (UAP_BaseElement element in elements)
		{
			Register_Item(element);
		}

		// Don't activate an empty container
		if (prevCount == 0 && m_AllElements.Count > 0)
		{
			ResetToStart();
			m_ActivateContainerNextFrame = true;
		}
		else
		{
			if (needsResetToStart)
				ResetToStart();
		}
	}

	private void ActivateContainer_Internal()
	{
		UAP_AccessibilityManager.ActivateContainer(this, true);
		m_ActivateContainerNextFrame = false;
	}

	//////////////////////////////////////////////////////////////////////////

	void Register_Item(UAP_BaseElement item)
	{
		EUIElement type = item.m_Type;

		// Is this the correct container for this element?
		AccessibleUIGroupRoot container = null;
		Transform tr = item.transform;
		container = tr.gameObject.GetComponent<AccessibleUIGroupRoot>();
		while (container == null && tr.parent != null)
		{
			tr = tr.parent;
			container = tr.gameObject.GetComponent<AccessibleUIGroupRoot>();
		}
		if (container != this)
			return;

		// sanity check
		if (container == null)
		{
			Debug.LogError("[Accessibility] A UI element tried to register with " + gameObject.name + " but not only am I not the right container, there seems to be no container in it's parent hierarchy.");
			return;
		}

		// sorted by position, top to bottom, left to right
		// sort it into the list of UI items
		Accessible_UIElement newElement = new Accessible_UIElement(item, type, m_AllElements.Count);
		int order = newElement.m_PositionOrder;
		int secondaryOrder = newElement.m_SecondaryOrder;
		int count = m_AllElements.Count;
		bool added = false;
		for (int i = 0; i < count; ++i)
		{
			if (order < m_AllElements[i].m_PositionOrder)
			{
				m_AllElements.Insert(i, newElement);
				added = true;
				break;
			}
			else if (order == m_AllElements[i].m_PositionOrder)
			{
				// Take Secondary Order into account (for scroll views an the like)
				int difference = m_AllElements[i].m_SecondaryOrder - secondaryOrder;
				//int testVal = m_AllElements[i].m_SecondaryOrder;
				//int currentVal = secondaryOrder;
				if (difference > 0)
				{
					m_AllElements.Insert(i, newElement);
					added = true;
					break;
				}
			}
		}
		if (!added)
		{
			m_AllElements.Add(newElement);
		}

		// Save it if this element is to be the first one to use
		if (item.m_ForceStartHere)
			m_CurrentStartItem = item;
	}

	//////////////////////////////////////////////////////////////////////////

	void OnEnable()
	{
		if (!m_HasStarted)
			return;

		RefreshContainer();

		ResetToStart();

		// Let the Manager know this UI is now active
		if (m_AllElements.Count > 0)
			m_ActivateContainerNextFrame = true;
	}

	//////////////////////////////////////////////////////////////////////////

	void Start()
	{
		m_HasStarted = true;

		RefreshContainer();

		ResetToStart();

		// Let the Manager know this UI is now active
		if (m_AllElements.Count > 0)
			m_ActivateContainerNextFrame = true;
	}

	//////////////////////////////////////////////////////////////////////////

	public void ResetToStart()
	{
		m_CurrentItemIndex = 0;

		// Start at the element that should be started at
		if (m_CurrentStartItem != null)
		{
			for (int i = 0; i < m_AllElements.Count; ++i)
			{
				if (m_AllElements[i].m_Object == m_CurrentStartItem)
				{
					m_CurrentItemIndex = i;
					break;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void OnDisable()
	{
		// Let the manager know that this UI is now inactive
		UAP_AccessibilityManager.ActivateContainer(this, false);
	}

	void OnDestroy()
	{
		// Let the manager know that this UI is now inactive
		UAP_AccessibilityManager.ActivateContainer(this, false);
	}

	//////////////////////////////////////////////////////////////////////////

	public Accessible_UIElement GetCurrentElement(bool rollOverAllowed)
	{
		// Only return ACTIVE/ENABLED elements
		m_CurrentItemIndex = FindFirstActiveItemIndex(m_CurrentItemIndex, rollOverAllowed);

		if (m_CurrentItemIndex < 0 || m_AllElements.Count == 0)
			return null;

		return m_AllElements[m_CurrentItemIndex];
	}

	//////////////////////////////////////////////////////////////////////////

	int FindFirstActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		if (m_RefreshNextFrame)
			RefreshContainer();

		if (m_AllElements.Count == 0)
			return -1;

		if (startIndex < 0)
			startIndex = 0;
		if (startIndex >= m_AllElements.Count)
		{
			if (rollOverAllowed)
				startIndex = 0;
			else
				return -1;
		}

		for (int i = 0; i < m_AllElements.Count; ++i)
		{
			// Roll over index
			int index = i + startIndex;
			if (index >= m_AllElements.Count)
			{
				if (rollOverAllowed)
					index = index - m_AllElements.Count;
				else
					return -1;
			}

			// Safety Check
			if (m_AllElements[index].m_Object == null)
			{
				RefreshContainer();
				return FindFirstActiveItemIndex(startIndex, rollOverAllowed);
			}

			if (m_AllElements[index].m_Object.IsElementActive())
				return index;
		}

		return -1;
	}

	int FindPreviousActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		if (m_RefreshNextFrame)
			RefreshContainer();

		if (m_AllElements.Count == 0)
			return -1;

		if (startIndex < 0)
		{
			if (rollOverAllowed)
				startIndex = m_AllElements.Count - 1;
			else
				return -1;
		}
		if (startIndex >= m_AllElements.Count)
			startIndex = m_AllElements.Count - 1;

		for (int i = m_AllElements.Count; i > 0; --i)
		{
			// Roll over index
			int index = i + startIndex - m_AllElements.Count;
			if (index < 0)
			{
				if (rollOverAllowed)
					index += m_AllElements.Count;
				else
					return -1;
			}

			// Safety Check
			if (m_AllElements[index].m_Object == null)
			{
				RefreshContainer();
				return FindFirstActiveItemIndex(startIndex, rollOverAllowed);
			}

			if (m_AllElements[index].m_Object.IsElementActive())
				return index;
		}

		return -1;
	}

	//////////////////////////////////////////////////////////////////////////

	//! Function returns true if the next item is further into the screen,
	//! and false if a rollover happened (or there are no active items)
	public bool IncrementCurrentItem(bool rollOverAllowed)
	{
		int nextIndex = FindFirstActiveItemIndex(m_CurrentItemIndex + 1, rollOverAllowed);

		// Was there even an active element found?
		if (nextIndex < 0)
		{
			return false;
		}

		// Did a rollover happen?
		if (nextIndex <= m_CurrentItemIndex)
		{
			if (rollOverAllowed)
				m_CurrentItemIndex = nextIndex;
			return false;
		}

		m_CurrentItemIndex = nextIndex;
		return true;
	}

	public bool DecrementCurrentItem(bool rollOverAllowed)
	{
		int nextIndex = FindPreviousActiveItemIndex(m_CurrentItemIndex - 1, rollOverAllowed);

		// Was there even an active element found?
		if (nextIndex < 0)
		{
			return false;
		}

		// Did a rollover happen?
		if (nextIndex >= m_CurrentItemIndex)
		{
			if (rollOverAllowed)
				m_CurrentItemIndex = nextIndex;
			return false;
		}

		m_CurrentItemIndex = nextIndex;
		return true;
	}

	public bool MoveFocus2D(UAP_AccessibilityManager.ESDirection direction)
	{
		// Select the closest item in the direction (quarter) 
		// that's also active.

		//Debug.Log("[Container] Moving focus " + direction);

		Vector2 activeItemPos = m_AllElements[m_CurrentItemIndex].m_Pos;

		// Go through all items (skip current one)
		Vector2 deltaPos;
		int nextElementIndex = -1;
		float closestDistanceSquared = -1.0f;
		for (int i = 0; i < m_AllElements.Count; ++i)
		{
			if (i == m_CurrentItemIndex)
				continue;

			if (!m_AllElements[i].m_Object.IsElementActive())
				continue;

			deltaPos = m_AllElements[i].m_Pos - activeItemPos;

			// Add in a delta scale by which one axis must be larger than the other
			// to avoid going diagonal at the edges of grids with uneven numbers of elements 
			// in a row or column (think of a diamond shaped grid - the focus should not jump 
			// up or down a row when the edge is hit and the user tries to keep going.
			float deltaScale = 1.1f; // this means the axis needs to be 10% larger

			// Check if the position of the item is in the right quarter
			bool isInQuarter = false;
			switch (direction)
			{
				case UAP_AccessibilityManager.ESDirection.ELeft:
					if (deltaPos.x < 0 && (-deltaPos.x) > Mathf.Abs(deltaPos.y) * deltaScale)
						isInQuarter = true;
					break;

				case UAP_AccessibilityManager.ESDirection.ERight:
					if (deltaPos.x > 0 && deltaPos.x > Mathf.Abs(deltaPos.y) * deltaScale)
						isInQuarter = true;
					break;

				case UAP_AccessibilityManager.ESDirection.EUp:
					if (deltaPos.y > 0 && deltaPos.y > Mathf.Abs(deltaPos.x) * deltaScale)
						isInQuarter = true;
					break;

				case UAP_AccessibilityManager.ESDirection.EDown:
					if (deltaPos.y < 0 && (-deltaPos.y) > Mathf.Abs(deltaPos.x) * deltaScale)
						isInQuarter = true;
					break;
			}

			if (!isInQuarter)
				continue;

			// Check distance (squared) if larger then current smallest, continue
			float squaredDistance = deltaPos.SqrMagnitude();
			if (nextElementIndex < 0 || squaredDistance < closestDistanceSquared)
			{
				nextElementIndex = i;
				closestDistanceSquared = squaredDistance;
			}
		}

		// Did the element change?
		if (nextElementIndex < 0)
		{
			//Debug.Log("[Container] Found no element to go to.");
			return false;
		}

		//Debug.Log("[Container] Found a new element");
		m_CurrentItemIndex = nextElementIndex;
		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	void Update()
	{
		if (m_ActivateContainerNextFrame)
		{
			if (m_DoubleCheckUIElementsPositions && m_NeedsRefreshBeforeActivation)
			{
				m_RefreshNextFrame = true;
				m_NeedsRefreshBeforeActivation = false;
			}
			else
			{
				ActivateContainer_Internal();
				m_NeedsRefreshBeforeActivation = m_DoubleCheckUIElementsPositions; // move into activate internal?
			}
		}

		if (m_RefreshNextFrame)
		{
			m_RefreshNextFrame = false;
			RefreshContainer();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void RefreshNextUpdate()
	{
		m_RefreshNextFrame = true;
	}

	//////////////////////////////////////////////////////////////////////////

	public void JumpToFirst()
	{
		m_CurrentItemIndex = 0;
		FindFirstActiveItemIndex(m_CurrentItemIndex, false);
	}

	//////////////////////////////////////////////////////////////////////////

	public void JumpToLast()
	{
		m_CurrentItemIndex = m_AllElements.Count - 1;
		FindPreviousActiveItemIndex(m_CurrentItemIndex, false);
	}

	//////////////////////////////////////////////////////////////////////////

	public void SetActiveElementIndex(int index, bool rollOverAllowed)
	{
		if (index < 0 || index >= m_AllElements.Count)
		{
			Debug.LogError("[Accessibility] UI Group: Trying to set an out of bounds index for current item. Setting index to first valid and active item to prevent crash.");
			index = 0;
		}

		if (!m_AllElements[index].m_Object.IsElementActive())
		{
			Debug.LogWarning("[Accessibility] UI Group: Trying to set a current item index to an inactive element. Setting index to next valid and active item to prevent issues.");
		}

		m_CurrentItemIndex = FindFirstActiveItemIndex(index, rollOverAllowed);
	}

	//////////////////////////////////////////////////////////////////////////

	public int GetCurrentElementIndex()
	{
		return m_CurrentItemIndex;
	}

	//////////////////////////////////////////////////////////////////////////

	//! Returns all elements in this container, including inactive ones.
	//! When using keep in mind that the first element might not be the one 
	//! that the navigation should start at (see UAP_BaseElement::m_ForceStartHere).
	//! Also keep in mind that elements might be inactive. 
	//! Use UAP_BaseElement::IsElementActive() to check before using.
	public List<Accessible_UIElement> GetElements()
	{
		return m_AllElements;
	}

	//////////////////////////////////////////////////////////////////////////

	public bool SelectItem(UAP_BaseElement element, bool forceRepeatItem = false)
	{
		for (int i = 0; i < m_AllElements.Count; ++i)
		{
			Accessible_UIElement accessElement = m_AllElements[i];
			if (accessElement.m_Object == element)
			{
				m_CurrentItemIndex = i;
				// Notify the Manager that this should be the active container
				return UAP_AccessibilityManager.MakeActiveContainer(this, forceRepeatItem);
			}
		}
		return false;
	}

	public string GetContainerName(bool useGameObjectNameIfNone = false)
	{
		if (m_ContainerName.Length > 0)
		{
			if (IsNameLocalizationKey())
				return UAP_AccessibilityManager.Localize(m_ContainerName);
			return m_ContainerName;
		}

		if (useGameObjectNameIfNone)
			return gameObject.name;
		return "";
	}

	//////////////////////////////////////////////////////////////////////////

	public bool IsNameLocalizationKey()
	{
		return (m_IsLocalizationKey);
	}
}
