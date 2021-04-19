#ifdef DLL_EXPORTS
#define DLL_API __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif

#include <mutex>
#include <list>
#include <thread>

namespace WindowsVoice {
  extern "C" {
    DLL_API void __cdecl Initialize();
    DLL_API void __cdecl AddToSpeechQueue(const char* text);
		DLL_API void __cdecl StopSpeech();
		DLL_API void __cdecl DestroySpeech();
		DLL_API void __cdecl SetVolume(int volume);
		DLL_API void __cdecl SetRate(int rate);
		DLL_API bool __cdecl IsVoiceSpeaking();
		//DLL_API int  __cdecl GetVoiceCount();
		DLL_API void __cdecl SetVoiceSAPI(const char* voiceDescription);
  }

  std::mutex theMutex;
  std::list<wchar_t*> theSpeechQueue;
  std::thread* theSpeechThread;
	bool stopSpeech;
  bool shouldTerminate;
}