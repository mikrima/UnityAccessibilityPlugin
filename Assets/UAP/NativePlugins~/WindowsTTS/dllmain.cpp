/**
 * Copyright (c)2017 MetalPop Games
 */

#include "pch.h"
#include "WindowsTTS.h"
#include <sapi.h>
#include <sphelper.h>
#include <atlbase.h>
#include <Windows.h>


namespace WindowsVoice 
{

	int Volume = 100;
	int Rate = 0;

	bool IsSpeaking = false;
	ISpVoice * pVoice = NULL;

	void SpeechThreadFunc()
	{
		//SpeechSynthesizer* synth = new SpeechSynthesizer();

		SPVOICESTATUS *pStatus = new SPVOICESTATUS();

		if (FAILED(::CoInitializeEx(NULL, COINITBASE_MULTITHREADED)))
		{
			//std::cout<<"Failed to initialize COM for Voice.\n";
			return;
		}

		HRESULT hr = CoCreateInstance(CLSID_SpVoice, NULL, CLSCTX_ALL, IID_ISpVoice, (void **)&pVoice);
		pVoice->SetRate(Rate);
		pVoice->SetVolume(Volume);
		if (!SUCCEEDED(hr))
		{
			LPVOID pText = 0;

			::FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL, hr, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&pText, 0, NULL);
			//std::cout<<"Failed to create voice instance. Error: "<<pText<<std::endl;
			LocalFree(pText);
			return;
		}


		// Enumerate available voices	
		//hr = SpEnumTokens(SPCAT_VOICES, NULL, NULL, &m_VoicesEnum);
		//if (SUCCEEDED(hr))
		//	hr = m_VoicesEnum->GetCount(&m_VoicesCount);


		//std::cout << "Speech ready.\n";
		wchar_t* priorText = nullptr;
		while (!shouldTerminate)
		{
			wchar_t* wText = NULL;
			if (stopSpeech)
			{
				ULONG pulNumSkipped;
				stopSpeech = false;
				pVoice->Skip(L"Sentence", 2147483647, &pulNumSkipped);
				pVoice->Pause();
				pVoice->Speak(NULL, SPF_PURGEBEFORESPEAK | SPF_ASYNC, NULL);
				pVoice->Resume();
				//pVoice->Release();
				//pVoice = NULL;
				//HRESULT hr = CoCreateInstance(CLSID_SpVoice, NULL, CLSCTX_ALL, IID_ISpVoice, (void **)&pVoice);
				//pVoice->SetRate(Rate);
				//pVoice->SetVolume(Volume);
			}

			if (!theSpeechQueue.empty())
			{
				theMutex.lock();
				wText = theSpeechQueue.front();
				theSpeechQueue.pop_front();
				theMutex.unlock();
			}

			HRESULT voiceHr = pVoice->GetStatus(pStatus, NULL);
			IsSpeaking = (pStatus->dwRunningState == SPRS_IS_SPEAKING);

			if (wText)
			{
				//SetNotifyCallbackFunction
					pVoice->Speak(wText, SPF_IS_NOT_XML | SPF_ASYNC, NULL);
					//          Sleep(250);
					delete[] priorText;
					priorText = wText;
			}
		}
		pVoice->Release();
		//std::cout << "Speech thread terminated.\n";
	}

	//////////////////////////////////////////////////////////////////////////

	void AddToSpeechQueue(const char* text)
	{
		if (text)
		{
			int len = strlen(text) + 1;
			wchar_t *wText = new wchar_t[len];

			memset(wText, 0, len);
			::MultiByteToWideChar(CP_ACP, NULL, text, -1, wText, len);

			theMutex.lock();
			theSpeechQueue.push_back(wText);
			theMutex.unlock();
		}
	}

	//////////////////////////////////////////////////////////////////////////

	void Initialize()
	{
		//std::printf("Starting Windows Voice.\n");
		shouldTerminate = false;
		stopSpeech = false;

		theSpeechThread = new std::thread(WindowsVoice::SpeechThreadFunc);
	}

	//////////////////////////////////////////////////////////////////////////

	void StopSpeech()
	{
		theMutex.lock();
		stopSpeech = true;
		theMutex.unlock();
	}

	//////////////////////////////////////////////////////////////////////////

	void DestroySpeech()
	{
		shouldTerminate = true;
		theSpeechThread->join();
		delete theSpeechThread;
		CoUninitialize();
	}

	//////////////////////////////////////////////////////////////////////////

	void SetVolume(int volume)
	{
		theMutex.lock();
		Volume = volume;
		theMutex.unlock();
	}

	//////////////////////////////////////////////////////////////////////////

	void SetRate(int rate)
	{
		theMutex.lock();
		Rate = rate;
		theMutex.unlock();
	}

	//////////////////////////////////////////////////////////////////////////

	bool IsVoiceSpeaking()
	{
		return IsSpeaking;
	}

	//////////////////////////////////////////////////////////////////////////

	//int GetVoiceCount()
	//{
	//	return m_VoicesCount;
	//}

	void SetVoiceSAPI(const char* voiceDescription)
	{
		if (voiceDescription)
		{
			// convert char to WCHAR
			int len = strlen(voiceDescription) + 1;
			wchar_t *wText = new wchar_t[len];
			memset(wText, 0, len);
			::MultiByteToWideChar(CP_ACP, NULL, voiceDescription, -1, wText, len);

			// Try to find a matching voice
			ISpObjectToken* cpToken(NULL);
			SpFindBestToken(SPCAT_VOICES, wText, L"", &cpToken);

			// Set the voice
			theMutex.lock();
			pVoice->SetVoice(cpToken);
			theMutex.unlock();

			cpToken->Release();
		}
	}

	//////////////////////////////////////////////////////////////////////////

}

//////////////////////////////////////////////////////////////////////////


BOOL APIENTRY DllMain(HMODULE, DWORD ul_reason_for_call, LPVOID)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}