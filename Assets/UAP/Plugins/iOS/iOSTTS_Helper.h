#import <AVFoundation/AVFoundation.h>

// This is a helper class containing the native iOS code for Text To Speech synthesis
@interface iOSTTS_Helper : UIViewController 
{
	AVSpeechSynthesizer *speechSynthesizer;
	BOOL isSpeaking;
	BOOL isRegistered;
	NSString *mostRecentLine;
}

//@property (nonatomic, assign) BOOL isSpeaking;

// AV synthesizer
@property (strong, nonatomic) AVSpeechSynthesizer *speechSynthesizer;

@end

