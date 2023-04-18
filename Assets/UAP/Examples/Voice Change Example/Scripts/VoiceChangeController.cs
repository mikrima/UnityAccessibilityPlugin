using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System;

public class VoiceChangeController : MonoBehaviour
{
    /// <summary>
    /// Enum with all available languages
    /// </summary>
    public enum languages { English, Spanish }

    [Tooltip("The voice to use for English")]
    public string englishVoice = "Microsoft David Desktop";
    [Tooltip("The voice to use for Spanish")]
    public string spanishVoice = "Microsoft Sabina Desktop";
    //SAPI voice to use for each language
    private Dictionary<languages, string> voices = new Dictionary<languages, string>();
    //the currently active voice
    private string activeVoice = "";
    [Tooltip("The TextMesh component where current voice will be displayed")]
    public TextMeshProUGUI voiceDisplay;

    /* Called once each frame */
    private void Update()
    {
        //Escape key to toggle UAP on and off
        if (Input.GetKeyDown(KeyCode.Escape))EnableUAP();    

    }

    private static VoiceChangeController _instance = null;
    public static VoiceChangeController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VoiceChangeController>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(VoiceChangeController).ToString());
                    _instance = go.AddComponent<VoiceChangeController>();
                }
            }
            return _instance;
        }
    }

    /* when the scene first runs, make sure that there is only one  */
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(gameObject);

            //initialize localization
            LocalizationSettings.InitializationOperation.WaitForCompletion();

            UAP_AccessibilityManager.SetLanguage("English");
            //prepare the voices dictionary
            voices[languages.English] = englishVoice;
            voices[languages.Spanish] = spanishVoice;
        }
    }


    #region Change Language
    /// <summary>
    /// Changes the language
    /// </summary>
    /// <param name="newLanguage">Which language to change to</param>
    public void ChangeLanguage(string newLanguage)
    {
        //switch the UAP language
        UAP_AccessibilityManager.SetLanguage(newLanguage);
        languages newLangEnum;
        //convert the language string to the languages enum
        bool validLang = Enum.TryParse<languages>(newLanguage, out newLangEnum);
        //if the language string was valid
        if (validLang) {
            //change the voice
            activeVoice = UAP_AccessibilityManager.SetVoice(voices[newLangEnum]);
            //Say what language this is
            UAP_AccessibilityManager.Say(newLanguage, true);
            //display the current voice
            voiceDisplay.text = activeVoice;
        }
        else {
            //create a list of languages
            string[] langArr= Enum.GetNames(typeof(languages));
            string langList = langArr[0];
            for (int i = 1; i < langArr.Length; i++) langList += $" {langArr[i]}";

            throw new Exception($"Language string {newLanguage} is not a valid language. Valid languages are {langList}");
        }
        
    }
    #endregion

    

    /// <summary>
    /// Enables UAP
    /// </summary>
    public void EnableUAP()
    {
        //if accessibility not already enabled enable it
        if (!UAP_AccessibilityManager.IsEnabled()) UAP_AccessibilityManager.EnableAccessibility(true);
        //otherwise disable it
        else  UAP_AccessibilityManager.EnableAccessibility(false);
        
    }
}
