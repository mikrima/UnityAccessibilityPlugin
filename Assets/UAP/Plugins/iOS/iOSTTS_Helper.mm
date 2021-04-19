#import "iOSTTS_Helper.h"

@implementation iOSTTS_Helper
@synthesize speechSynthesizer;

//////////////////////////////////////////////////////////////////////////

-(bool) isSynthesizerSpeaking
{
	if(UIAccessibilityIsVoiceOverRunning())
	{
		if (isSpeaking == YES)
		{
			return true;
		}
	}

	if (speechSynthesizer == nil) 
	{
		return false;
	}

	if (self.speechSynthesizer.speaking == NO) 
	{
		isSpeaking = NO;
		return false;
	}

	return true;
}

//////////////////////////////////////////////////////////////////////////

-(void) stopSynthesizerSpeaking
{
	if (speechSynthesizer == nil) 
	{
		speechSynthesizer = [[AVSpeechSynthesizer alloc] init];
	}

	[self.speechSynthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
	isSpeaking = NO;

	if (isSpeaking)
		UIAccessibilityPostNotification(UIAccessibilityAnnouncementNotification, @"");
}

//////////////////////////////////////////////////////////////////////////

-(void) startSynthesizerSpeaking:(NSString*)textToSpeak withRate:(int)speechRate
{
	if (speechSynthesizer == nil) 
	{
		speechSynthesizer = [[AVSpeechSynthesizer alloc] init];
	}

	AVSpeechUtterance *utterance = [[AVSpeechUtterance alloc] initWithString:textToSpeak];

	// Set Rate
	if ([[[UIDevice currentDevice] systemVersion] floatValue] > 8.5) 
	{
		utterance.rate = 0.5 + ((speechRate - 50) / 100.0);
	}
	else
	{
		utterance.rate = 0.1f;

		if (speechRate < 50)
		{
			utterance.rate = 0.1 * (speechRate / 50.0);
		}
		else if (speechRate > 50)
		{
			utterance.rate = 0.1 + (0.9 * ((speechRate - 50) / 50.0));
		}

	}

	// Set Voice
	//NSString *voiceLangCode = [AVSpeechSynthesisVoice currentLanguageCode];
	//utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:voiceLangCode];
	
	// Default to English - if not set, then the text will be spoken with the default system language
	//utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:@"en-US"];
	
	//utterance.voice = nil; 

	mostRecentLine = textToSpeak;
	[self.speechSynthesizer speakUtterance:utterance];
	
	// Default settings
	//[self.speechSynthesizer speakUtterance:[AVSpeechUtterance speechUtteranceWithString:textToSpeak]];
}

//////////////////////////////////////////////////////////////////////////

-(void) startVoiceOverSpeaking:(NSString*)textToSpeak
{
	//if(UIAccessibilityIsVoiceOverRunning())
	{
		if (isRegistered == NO)
		{
			[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(voiceOverDidFinishAnnouncing:) name:UIAccessibilityAnnouncementDidFinishNotification object:nil];
			isRegistered = YES;
		}
		UIAccessibilityPostNotification(UIAccessibilityAnnouncementNotification, textToSpeak);
		mostRecentLine = textToSpeak;
		isSpeaking = YES;
	}
}

//////////////////////////////////////////////////////////////////////////

-(AVSpeechSynthesizer*) speechSynthesizer 
{
	if (speechSynthesizer == nil) 
	{
		speechSynthesizer = [[AVSpeechSynthesizer alloc] init];
	}
	
	return speechSynthesizer;
}

//////////////////////////////////////////////////////////////////////////

-(void) initTTS
{
	// Init VoiceOver hooks
	isSpeaking = NO;
	isRegistered = NO;

	//[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(voiceOverDidFinishAnnouncing:) name:UIAccessibilityAnnouncementDidFinishNotification object:nil];
}

//////////////////////////////////////////////////////////////////////////


-(void) shutdownTTS
{
	[[NSNotificationCenter defaultCenter] removeObserver:self];
}

//////////////////////////////////////////////////////////////////////////

-(void) voiceOverDidFinishAnnouncing:(NSNotification*) note
{
		if ([mostRecentLine isEqualToString:note.userInfo[UIAccessibilityAnnouncementKeyStringValue]])
		{
				isSpeaking = NO;
				mostRecentLine = nil;
		}
}

//////////////////////////////////////////////////////////////////////////

@end

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

extern "C" 
{
	bool IsVoiceOverRunning();
	bool IsVoiceSpeaking();
	void StopVoiceSpeaking();
	void StartSpeakingVoiceOver(char* textToSpeak);
	void StartSpeakingSynthesizer(char* textToSpeak, int speechRate);
	void InitializeVoice();
	void ShutdownVoice();
}

//////////////////////////////////////////////////////////////////////////

static iOSTTS_Helper *TTS_Helper = nil;

//////////////////////////////////////////////////////////////////////////

bool IsVoiceOverRunning()
{
	if(UIAccessibilityIsVoiceOverRunning())
	{
		return true;
	}
	return false;
}

//////////////////////////////////////////////////////////////////////////

bool IsVoiceSpeaking()
{
	return [TTS_Helper isSynthesizerSpeaking];
}

//////////////////////////////////////////////////////////////////////////

void StopVoiceSpeaking()
{
	if(TTS_Helper != nil)
	{
		[TTS_Helper stopSynthesizerSpeaking];
	}   
}

//////////////////////////////////////////////////////////////////////////

void StartSpeakingVoiceOver(char* textToSpeak)
{
	// Create the TTS Helper instance if none exists yet
	if(TTS_Helper == nil)
	{
		TTS_Helper = [iOSTTS_Helper alloc];
	}

	// No sense in going on if there was nothing passed in
	if (textToSpeak == nil)
		return;

	NSString *strTextToSpeak = [NSString stringWithUTF8String: textToSpeak];

	[TTS_Helper startVoiceOverSpeaking:strTextToSpeak];
}

//////////////////////////////////////////////////////////////////////////

void StartSpeakingSynthesizer(char* textToSpeak, int speechRate)
{
	// Create the TTS Helper instance if none exists yet
	if(TTS_Helper == nil)
	{
		TTS_Helper = [iOSTTS_Helper alloc];
	}

	// No sense in going on if there was nothing passed in
	if (textToSpeak == nil)
		return;

	NSString *strTextToSpeak = [NSString stringWithUTF8String: textToSpeak];

	[TTS_Helper startSynthesizerSpeaking:strTextToSpeak withRate:(int)speechRate];
}

//////////////////////////////////////////////////////////////////////////

void InitializeVoice()
{
	UIAccessibilityRegisterGestureConflictWithZoom();
	[TTS_Helper initTTS];
}

//////////////////////////////////////////////////////////////////////////

void ShutdownVoice()
{
	[TTS_Helper shutdownTTS];
}

///

// Get the device language and region code?
// Select voice based on user settings?

/*
NSString *language = [[[NSBundle mainBundle] preferredLocalizations] objectAtIndex:0];
NSString *voiceLangCode = [AVSpeechSynthesisVoice currentLanguageCode];
if (![voiceLangCode hasPrefix:language]) {
    // the default voice can't speak the language the text is localized to;
    // switch to a compatible voice:
    NSArray *speechVoices = [AVSpeechSynthesisVoice speechVoices];
    for (AVSpeechSynthesisVoice *speechVoice in speechVoices) {
        if ([speechVoice.language hasPrefix:language]) {
            self.voice = speechVoice;
            break;
        }
    }
}
*/

//////////////////////////////////////////////////////////////////////////

