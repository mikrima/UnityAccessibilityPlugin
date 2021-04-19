using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UAP_VirtualKeyboard : MonoBehaviour
{
	/// <summary>
	/// Path and name to the virtual keybaord, only change this if you move or rename the prefab
	/// </summary>
	const string PrefabPath = "UAP Virtual Keyboard";

	[System.Serializable]
	public class UAPKeyboardLayout
	{
		public SystemLanguage m_SystemLanguage = SystemLanguage.English;
		public string m_Letters =
			"QWERTYUIOP" +
			"ASDFGHJKL" +
			"ZXCVBNM";
		public string m_BottomKeys = ",.";
	}

	/// <summary>
	/// Default: Normal text keyboard, with a button to switch to numbers and symbols. Text is displayed as typed in the preview label.
	/// Password: Normal text keyboard, with a button to switch to numbers and symbols. Letters are displayed for a brief moment after they are typed, then they are replaced by a dot '•'
	/// </summary>
	public enum EKeyboardMode
	{
		Default = 0,
		Password = 1,
	}

	private enum EKeyboardPage
	{
		Letters,
		Numbers,
		Symbols
	}

	[Header("Layouts")]
	public UAPKeyboardLayout m_NumbersLayout = new UAPKeyboardLayout() { m_Letters = "1234567890@#$&*()'\"_ -/:;!?" };
	public UAPKeyboardLayout m_SymbolsLayout = new UAPKeyboardLayout() { m_Letters = "1234567890€£¥%^[]{}+=|\\©®™", m_BottomKeys = "<>" };
	public List<UAPKeyboardLayout> m_SupportedKeyboardLayouts = new List<UAPKeyboardLayout>();

	[Header("References")]
	public Transform m_SecondButtonRow = null;
	public List<Button> m_LetterButtons = new List<Button>();
	public Text m_PreviewText = null;
	public UAP_BaseElement m_PreviewTextAccessible = null;
	public Button m_LanguageButton = null;
	public Button m_EmailAtButton = null;
	public Button m_ShiftKey = null;
	public Image m_ShiftSymbol = null;
	public Button m_SwitchKey = null;
	public Button m_Done = null;
	public Button m_Cancel = null;
	public Button m_LeftOfSpace = null;
	public Button m_RightOfSpace = null;
	public Button m_ReturnKey = null;

	// Settings
	private string m_OriginalText = ""; //! The original text that was in the textbox when the keyboard was opened (will be restored if input is canceled)
	private bool m_AllowMutliLine = false; 
	private EKeyboardMode m_KeyboardMode = EKeyboardMode.Default;
	private bool m_StartCapitalized = true; //! Auto-enables Shift mode for the first letter
	private SystemLanguage m_PreferredLanguage = SystemLanguage.English; //! This is the language the keyboard is initialized in, but players can always switch to the English keyboard

	// Current keyboard state
	private EKeyboardPage m_CurrentKeyboardPage = EKeyboardPage.Letters;
	private UAPKeyboardLayout m_ActiveLetterLayout = null;
	private bool m_ShiftModeActive = false; //! Only affect letter keyboard page
	private string m_EditedText = ""; //! The working copy of the text, modified by the user's input
	private string m_PasswordedText = ""; //! The working copy of the text, with most or all letters replaced by a dot
	private SystemLanguage m_CurrentLanguage = SystemLanguage.English;

	// Cursor/Caret
	private float m_CursorBlinkDuration = 0.8f; //! Interval of blinking, in seconds
	private float m_CursorBlinkTimer = -1.0f;

	// Misc
	private static UAP_VirtualKeyboard Instance = null;
	private static UnityAction<string, bool> m_OnFinishListener = null;
	private static UnityAction<string> m_OnChangeListener = null;

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	private void Update()
	{
		m_CursorBlinkTimer -= Time.unscaledDeltaTime;
		bool showCursor = m_CursorBlinkTimer > 0;
		if (m_CursorBlinkTimer < -m_CursorBlinkDuration)
			m_CursorBlinkTimer = m_CursorBlinkDuration;

		// #UAP_Keyboard_Backlog Implement that passwords show their last letter for a moment before replacing it with a dot
		if (m_PasswordedText.Length != m_EditedText.Length)
		{
			m_PasswordedText = "";
			for (int i = 0; i < m_EditedText.Length; i++)
				m_PasswordedText += "•";
		}

		m_PreviewText.text = (m_KeyboardMode == EKeyboardMode.Password ? m_PasswordedText : m_EditedText) + (showCursor ? "<color='#777777'>|</color>" : "");
		m_PreviewTextAccessible.SetCustomText((m_KeyboardMode == EKeyboardMode.Password ? UAP_AccessibilityManager.Localize_Internal("Keyboard_PasswordHidden") : m_EditedText)); 

		// Support actual keyboard input coming from a Bluetooth keyboard or similar
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			// #UAP_Keyboard_Backlog Handle backspace key being held continuously (use a sensible repeat rate)
			OnBackSpacePressed();
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			// Return is needed to activate items - at least on Desktop. 
			// For Bluetooth keyboards on mobile Return will have the actual effect, since it can be assumed it is used
			// for text input and NOT to control the UI. To enter a new line on Desktop platform, the left shift key can be held
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android || Input.GetKey(KeyCode.LeftShift))
				OnReturnPressed();
		}
		else if (!string.IsNullOrEmpty(Input.inputString))
		{
			AddLetter(Input.inputString);
		}
	}

	//////////////////////////////////////////////////////////////////////////

	private void OnTextUpdated()
	{
		// Reset cursor blinking
		m_CursorBlinkTimer = m_CursorBlinkDuration;
		if (m_ShiftModeActive)
		{
			if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
				OnShiftKeyPressed();
			else
				m_ShiftModeActive = false;
		}

		// Notify listener
		if (m_OnChangeListener != null)
			m_OnChangeListener.Invoke(m_EditedText);

		// Repeat typed word if in default mode
		if (m_KeyboardMode == EKeyboardMode.Default)
			UAP_AccessibilityManager.Say(m_EditedText);
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetKeyboardLayoutForLanguage(SystemLanguage language)
	{
		m_CurrentLanguage = language;

		// Find the matching layout in the list
		m_ActiveLetterLayout = null;
		for (int i = 0; i < m_SupportedKeyboardLayouts.Count; i++)
		{
			if (m_SupportedKeyboardLayouts[i].m_SystemLanguage == language)
				m_ActiveLetterLayout = m_SupportedKeyboardLayouts[i];
		}
		if (m_ActiveLetterLayout == null)
			m_ActiveLetterLayout = new UAPKeyboardLayout();

		SetLettersLayout();

		// Set, show/hide the language toggle button
		if (m_PreferredLanguage == SystemLanguage.English)
		{
			m_LanguageButton.gameObject.SetActive(false);
			m_EmailAtButton.gameObject.SetActive(true); // Show the @ button instead of the language button
		}
		else
		{
			m_LanguageButton.gameObject.SetActive(true); // Show the language button instead of the @ button 
			m_EmailAtButton.gameObject.SetActive(false);

			// Language toggle button 
			if (m_CurrentLanguage == m_PreferredLanguage)
				m_LanguageButton.GetComponent<UAP_BaseElement>().SetCustomText("English");
			else
				m_LanguageButton.GetComponent<UAP_BaseElement>().SetCustomText(m_PreferredLanguage.ToString());
		}
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetLetterButtonsFromString(string letters, string bottomKeys)
	{
		// Some layout or pages need an extra button in the seconds row
		// Spanish for example has the Ñ character at the far right of that row
		bool isExtended = letters.Length > 26;
		m_LetterButtons[19].gameObject.SetActive(isExtended);

		// Move the entire second row depending on whether it has 9 or 10 keys
		if (isExtended)
		{
			// Offset to the left by half a button width and half the spacing
			float offsetX = m_LetterButtons[4].transform.position.x - m_LetterButtons[3].transform.position.x;
			m_SecondButtonRow.localPosition = new UnityEngine.Vector3(-0.5f * offsetX, 0, 0);
		}
		else
		{
			// Center the row
			m_SecondButtonRow.localPosition = new UnityEngine.Vector3(0, 0, 0);
		}

		int letterIndex = -1;
		for (int i = 0; i < m_LetterButtons.Count; i++)
		{
			if (!isExtended && i == 19)
				continue;
			++letterIndex;

			if (letterIndex < letters.Length)
			{
				if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
				{
					m_LetterButtons[i].GetComponent<UAP_BaseElement>().SetCustomText((m_ShiftModeActive ? UAP_AccessibilityManager.Localize_Internal("Keyboard_CapitalLetter").Replace("X", letters[letterIndex].ToString().ToLower()) : letters[letterIndex].ToString().ToLower()));
					m_LetterButtons[i].GetComponentInChildren<Text>().text = m_ShiftModeActive ? letters[letterIndex].ToString().ToUpper() : letters[letterIndex].ToString().ToLower();
				}
				else
				{
					m_LetterButtons[i].GetComponent<UAP_BaseElement>().SetCustomText(letters[letterIndex].ToString());
					m_LetterButtons[i].GetComponentInChildren<Text>().text = letters[letterIndex].ToString();
				}
			}
			else
			{
				// Just in case we have a layout with too few letters
				m_LetterButtons[i].gameObject.SetActive(false);
			}
		}

		m_LeftOfSpace.GetComponentInChildren<Text>().text = bottomKeys[0].ToString();
		m_RightOfSpace.GetComponentInChildren<Text>().text = bottomKeys[1].ToString();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnShiftKeyPressed()
	{
		// On the numbers page, this key leads to symbols
		// On the symbols page, this key leads to numbers
		// On the letters page, this key toggles upper case/lower case
		switch (m_CurrentKeyboardPage)
		{
			case EKeyboardPage.Numbers:
				SetSymbolsLayout();
				break;

			case EKeyboardPage.Symbols:
				SetNumbersLayout();
				break;

			case EKeyboardPage.Letters:
			default:
				// #UAP_Keyboard_Backlog Support hold mode (⇧ --> ⇪) - find out how this is activated in accessibility mode (double tap otherwise)
				m_ShiftModeActive = !m_ShiftModeActive;
				m_ShiftSymbol.color = m_ShiftModeActive ? Color.white : (Color)(new Color32(50, 50, 50, 255));
				UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal(m_ShiftModeActive ? "Keyboard_ShiftOn" : "Keyboard_ShiftOff"));
				SetLettersLayout();
				break;
		}
	}

	public void OnToggleKeyPressed()
	{
		// On the letters page, this key leads to numbers
		// On the numbers page, this key leads to letters
		// On the symbols page, this key leads to letters
		switch (m_CurrentKeyboardPage)
		{
			case EKeyboardPage.Letters:
				SetNumbersLayout();
				break;

			case EKeyboardPage.Numbers:
			case EKeyboardPage.Symbols:
			default:
				SetLettersLayout();
				break;
		}
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnLanguageKeyPressed()
	{
		// Toggle between English and the System language
		if (m_CurrentLanguage != m_PreferredLanguage)
			SetKeyboardLayoutForLanguage(m_PreferredLanguage);
		else
			SetKeyboardLayoutForLanguage(SystemLanguage.English);

		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingLanguage") + m_CurrentLanguage.ToString());
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetLettersLayout()
	{
		// Make accessibility announcement
		if (m_CurrentKeyboardPage != EKeyboardPage.Letters)
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingLetters")); 

		m_CurrentKeyboardPage = EKeyboardPage.Letters;
		m_ShiftKey.GetComponentInChildren<Text>().text = "";
		m_ShiftSymbol.gameObject.SetActive(true);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShiftKey")); 
		m_SwitchKey.GetComponentInChildren<Text>().text = "123";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_NumbersAndSymbols"));

		// Set the letter buttons
		SetLetterButtonsFromString(m_ActiveLetterLayout.m_Letters, m_ActiveLetterLayout.m_BottomKeys);
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetNumbersLayout()
	{
		// Make accessibility announcement
		if (m_CurrentKeyboardPage != EKeyboardPage.Numbers)
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingNumbers"));

		m_ShiftModeActive = false;
		m_CurrentKeyboardPage = EKeyboardPage.Numbers;
		m_ShiftKey.GetComponentInChildren<Text>().text = "%{<";
		m_ShiftSymbol.gameObject.SetActive(false);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Symbols"));
		m_SwitchKey.GetComponentInChildren<Text>().text = "abc";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Letters"));

		// Set the letter buttons
		SetLetterButtonsFromString(m_NumbersLayout.m_Letters, m_NumbersLayout.m_BottomKeys);
	}

	//////////////////////////////////////////////////////////////////////////

	private void SetSymbolsLayout()
	{
		// Make accessibility announcement
		if (m_CurrentKeyboardPage != EKeyboardPage.Symbols)
			UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_ShowingSymbols"));

		m_ShiftModeActive = false;
		m_CurrentKeyboardPage = EKeyboardPage.Symbols;
		m_ShiftKey.GetComponentInChildren<Text>().text = "123";
		m_ShiftSymbol.gameObject.SetActive(false);
		m_ShiftKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Numbers"));
		m_SwitchKey.GetComponentInChildren<Text>().text = "abc";
		m_SwitchKey.GetComponent<UAP_BaseElement>().SetCustomText(UAP_AccessibilityManager.Localize_Internal("Keyboard_Letters"));

		// Set the letter buttons
		SetLetterButtonsFromString(m_SymbolsLayout.m_Letters, m_SymbolsLayout.m_BottomKeys);
	}

	//////////////////////////////////////////////////////////////////////////

	private void AddLetter(string letter)
	{
		// Voice repeat typed letter
		UAP_AccessibilityManager.Say(letter);

		m_EditedText += letter;
		OnTextUpdated();
	}

	//////////////////////////////////////////////////////////////////////////

	public void OnLetterKeyPressed(Button button)
	{
		AddLetter(button.GetComponentInChildren<Text>().text);
	}

	public void OnSpacePressed()
	{
		// #UAP_Keyboard_Backlog Handle virtual key being held continuously (use a sensible repeat rate) (not in accessibility mode)

		AddLetter(" ");

		// Pressing space after a symbol or number switches back to letter keyboard
		if (m_CurrentKeyboardPage != EKeyboardPage.Letters)
			SetLettersLayout();
	}

	public void OnBackSpacePressed()
	{
		// #UAP_Keyboard_Backlog Handle virtual key being held continuously (use a sensible repeat rate) (not in accessibility mode)
		// #UAP_Keyboard_Backlog Long press also deletes entire words (not in accessibility mode)

		if (m_EditedText.Length == 0)
			return;
		m_EditedText = m_EditedText.Substring(0, m_EditedText.Length - 1);
		OnTextUpdated();

		AutoSetShiftMode();
	}

	public void OnReturnPressed()
	{
		// #UAP_Keyboard_Backlog Handle virtual key being held continuously (use a sensible repeat rate) (not in accessibility mode)

		if (m_AllowMutliLine)
		{
			AddLetter("\n");
		}
		else
		{
			// If multiline is not allowed, the return key ends the interaction and finalizes the input
			OnDonePressed();
		}
	}

	public void OnClearTextPressed()
	{
		m_EditedText = "";
		OnTextUpdated();

		AutoSetShiftMode();
	}

	private void AutoSetShiftMode()
	{
		if (m_EditedText.Length == 0 && m_StartCapitalized && !m_ShiftModeActive)
		{
			if (m_CurrentKeyboardPage == EKeyboardPage.Letters)
				OnShiftKeyPressed();
			else
				m_ShiftModeActive = true;
		}
	}

	public void OnDonePressed()
	{
		// Give the input text back to the input field
		if (m_OnFinishListener != null) m_OnFinishListener.Invoke(m_EditedText, true);
		ClearAllListeners();

		// Remove the keyboard
		CloseKeyboardOverlay();
	}

	public void OnCancelPressed()
	{
		// Restore the original value
		if (m_OnFinishListener != null) m_OnFinishListener.Invoke(m_OriginalText, false);
		ClearAllListeners();

		// Remove the keyboard
		CloseKeyboardOverlay();
	}

	//////////////////////////////////////////////////////////////////////////

	private void OnApplicationFocus(bool focus)
	{
		// Close keyboard with the cancel/abort action when focus is lost
		if (!focus)
			OnCancelPressed();
	}

	//////////////////////////////////////////////////////////////////////////

	private void InitializeKeyboard(string prefilledText, EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
		if (keyboardMode == EKeyboardMode.Password && startCapitalized)
		{
			Debug.LogWarning("[Accessibility] Password Input fields should not start capitalized. Ignoring parameter.");
			startCapitalized = false;
		}

		m_KeyboardMode = keyboardMode;
		m_StartCapitalized = startCapitalized;
		m_EditedText = prefilledText;
		m_OriginalText = prefilledText;
		m_CursorBlinkTimer = m_CursorBlinkDuration;
		m_AllowMutliLine = alllowMultiline;

		// Use system language if supported, or English as a fallback
		if (SupportsSystemLanguage())
			m_PreferredLanguage = Application.systemLanguage;
		else
			m_PreferredLanguage = SystemLanguage.English;
		SetKeyboardLayoutForLanguage(m_PreferredLanguage);

		// Depending on whether this is multiline or not, change the description of the Return key
		m_ReturnKey.GetComponent<UAP_BaseElement>().SetCustomText(alllowMultiline ? UAP_AccessibilityManager.Localize_Internal("Keyboard_Return") : UAP_AccessibilityManager.Localize_Internal("Keyboard_Done"));

		// Start with shift pressed, but only when the 
		AutoSetShiftMode();

		// Make voice announcement 'keyboard visible' (localized) and whether shift key is on
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_Showing"));
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal(m_ShiftModeActive ? "Keyboard_ShiftOn" : "Keyboard_ShiftOff"));

		// #UAP_Keyboard_Backlog Play proper incoming animation
	}

	//////////////////////////////////////////////////////////////////////////

	private void CloseKeyboardOverlay()
	{
		Instance = null;

		// #UAP_Keyboard_Backlog Close keyboard with an animation
		DestroyImmediate(gameObject);

		// Announce that the keyboard is now closed
		UAP_AccessibilityManager.Say(UAP_AccessibilityManager.Localize_Internal("Keyboard_Hidden")); 
	}

	//////////////////////////////////////////////////////////////////////////

	private void OnDestroy()
	{
		Instance = null;
	}

	//////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Calls back after the input was finished. 
	/// Parameters of the callback function: 
	/// string: text in the edit field (will be unchanged if text input was canceled)
	/// bool: true if 'Done' (or 'Return' in single line input) was pressed, false if the text input was canceled
	/// </summary>
	public static void SetOnFinishListener(UnityAction<string, bool> callback)
	{
		m_OnFinishListener = callback;
	}

	/// <summary>
	/// Optional, calls back every time the text is changed so it can be updated in the input text field (if desired).
	/// If the input is canceled, the OnFinish listener will be called with the original text.
	/// </summary>
	public static void SetOnChangeListener(UnityAction<string> callback)
	{
		m_OnChangeListener = callback;
	}

	/// <summary>
	/// Cancels the input and closes the keyboard. Text will be reset to what it was when the keyboard was opened.
	/// </summary>
	public static void CloseKeyboard()
	{
		if (Instance == null)
		{
			ClearAllListeners();
			return;
		}

		Instance.OnCancelPressed();
	}

	public static UAP_VirtualKeyboard ShowOnscreenKeyboard(string prefilledText = "", EKeyboardMode keyboardMode = EKeyboardMode.Default, bool startCapitalized = true, bool alllowMultiline = false)
	{
		// Force remove any previously existing keyboard
		if (Instance != null)
			DestroyImmediate(Instance.gameObject);

		// Clear any previous listeners
		ClearAllListeners();

		// Instantiate the keyboard
		var newKeyboard = (Instantiate(Resources.Load(PrefabPath)) as GameObject).GetComponent<UAP_VirtualKeyboard>();

		// Initialize with the given parameters
		newKeyboard.InitializeKeyboard(prefilledText, keyboardMode, startCapitalized, alllowMultiline);

		Instance = newKeyboard;
		return newKeyboard;
	}

	/// <summary>
	/// Remove all listeners to keyboard events
	/// Generally there is no need to remove listeners again, as they are automatically cleared when the keyboard is closed.
	/// </summary>
	public static void ClearAllListeners()
	{
		m_OnFinishListener = null;
		m_OnChangeListener = null;
	}

	/// <summary>
	/// Returns true if the virtual keyboard is currently open/visible on screen
	/// Will return false if the keyboard is in the process of closing
	/// </summary>
	/// <returns></returns>
	public static bool IsOpen()
	{
		return (Instance != null);
	}

	/// <summary>
	/// Returns true if a keyboard layout has been provided specifically for the system language.
	/// The keyboard will default to the English keyboard layout if the language is not supported.
	/// </summary>
	public static bool SupportsSystemLanguage()
	{
		var prefab = (Resources.Load(PrefabPath) as GameObject).GetComponent<UAP_VirtualKeyboard>();
		var supportedLayouts = prefab.m_SupportedKeyboardLayouts;
		for (int i = 0; i < supportedLayouts.Count; i++)
		{
			if (supportedLayouts[i].m_SystemLanguage == Application.systemLanguage)
				return true;
		}

		return false;
	}

	//////////////////////////////////////////////////////////////////////////

}
