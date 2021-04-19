package com.metalpopgames.androidtts;

/**
 * Created by Michelle.Martin on 2/28/2017.
 * Copyright (c)2017 MetalPop Games
 */

import android.app.Activity;
import android.speech.tts.TextToSpeech;

import java.util.Locale;

public class AndroidTTS implements TextToSpeech.OnInitListener
{
    static private AndroidTTS androidTTS = null;

    private TextToSpeech textToSpeech = null;

    private boolean isInitialized = false;

    //////////////////////////////////////////////////////////////////////////

    public void InitTTS(Activity unityActivity)
    {
        isInitialized = false;
        textToSpeech = new TextToSpeech(unityActivity, this);
    }

    public void ShutDown()
    {
        if (textToSpeech == null)
        {
            return;
        }

        textToSpeech.shutdown();
    }

    //////////////////////////////////////////////////////////////////////////

    @Override
    public void onInit(int status)
    {
        if (status == TextToSpeech.SUCCESS)
        {
            isInitialized = true;
        }
        else
        {
            isInitialized = false;
        }
    }

    //////////////////////////////////////////////////////////////////////////

    public void Say(String textToSay)
    {
        if (textToSpeech == null)
        {
            return;
        }

        textToSpeech.speak(textToSay, TextToSpeech.QUEUE_FLUSH, null);
    }

    static public void Speak(String textToSay)
    {
        if (androidTTS == null)
        {
            return;
        }

        androidTTS.Say(textToSay);
    }

    //////////////////////////////////////////////////////////////////////////

    public boolean IsTalking()
    {
        if (textToSpeech == null)
        {
            return false;
        }

        return textToSpeech.isSpeaking();
    }

    static public boolean IsSpeaking()
    {
        if (androidTTS == null)
        {
            return false;
        }

        return androidTTS.IsTalking();
    }

    //////////////////////////////////////////////////////////////////////////

    public void Stop()
    {
        if (textToSpeech == null)
        {
            return;
        }

        textToSpeech.speak("", TextToSpeech.QUEUE_FLUSH, null);
        textToSpeech.stop();
    }

    static public void StopSpeaking()
    {
        if (androidTTS == null)
        {
            return;
        }

        androidTTS.Stop();
    }

    //////////////////////////////////////////////////////////////////////////

    public void SetSpeechRate(float rate)
    {
        if (textToSpeech == null)
        {
            return;
        }

        textToSpeech.setSpeechRate(rate);
    }

    static public void SetSpeakingRate(float rate)
    {
        if (androidTTS == null)
        {
            return;
        }

        androidTTS.SetSpeechRate(rate);
    }

    //////////////////////////////////////////////////////////////////////////

    static public String GetInitializationStatus()
    {
        if (androidTTS == null)
            return "Android TTS not instantiated";

        if (androidTTS.textToSpeech == null)
            return "Text To Speech Engine not instantiated";

        if (androidTTS.isInitialized == false)
            return "Initialization failed or isn't completed yet";

        return "Fully initialized";
    }

    //////////////////////////////////////////////////////////////////////////

    static public void InitializeTTS(Activity unityActivity)
    {
        androidTTS = new AndroidTTS();
        androidTTS.InitTTS(unityActivity);
    }

    //////////////////////////////////////////////////////////////////////////

    static public void ShutdownTTS()
    {
        if (androidTTS == null)
            return;

        androidTTS.ShutDown();
    }

}
