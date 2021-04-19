#import "iOSGestures_Helper.h"

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@implementation iOSGestures_Helper


UISwipeGestureRecognizer *swipeleft;
UISwipeGestureRecognizer *swiperight;
UISwipeGestureRecognizer *swipeup;
UISwipeGestureRecognizer *swipedown;
UISwipeGestureRecognizer *swipeupDouble;
UISwipeGestureRecognizer *swipedownDouble;
UISwipeGestureRecognizer *swipeupTriple;
UISwipeGestureRecognizer *swipedownTriple;

//////////////////////////////////////////////////////////////////////////

-(void) removeGestureListeners
{
	UIViewController* unityViewCtrl =  UnityGetGLViewController();
		
	swipeleft.delegate = nil;
	swiperight.delegate = nil;
	swipeup.delegate = nil;
	swipedown.delegate = nil;
	swipeupDouble.delegate = nil;
	swipedownDouble.delegate = nil;
	swipeupTriple.delegate = nil;
	swipedownTriple.delegate = nil;

	[unityViewCtrl.view removeGestureRecognizer:swipeleft];
	[unityViewCtrl.view removeGestureRecognizer:swiperight];
	[unityViewCtrl.view removeGestureRecognizer:swipeup];
	[unityViewCtrl.view removeGestureRecognizer:swipedown];
	[unityViewCtrl.view removeGestureRecognizer:swipeupDouble];
	[unityViewCtrl.view removeGestureRecognizer:swipedownDouble];
	[unityViewCtrl.view removeGestureRecognizer:swipeupTriple];
	[unityViewCtrl.view removeGestureRecognizer:swipedownTriple];
		
	swipeleft = nil;
	swiperight = nil;
	swipeup = nil;
	swipedown = nil;
	swipeupDouble = nil;
	swipedownDouble = nil;
	swipeupTriple = nil;
	swipedownTriple = nil;
}

-(void) initGestureListeners
{
	UIViewController* unityViewCtrl =  UnityGetGLViewController();
		
	swipeleft=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipeleft:)];
	swipeleft.direction=UISwipeGestureRecognizerDirectionLeft;
	[swipeleft setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipeleft];

	swiperight=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swiperight:)];
	swiperight.direction=UISwipeGestureRecognizerDirectionRight;
	[swiperight setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swiperight];

	swipeup=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipeup:)];
	swipeup.direction=UISwipeGestureRecognizerDirectionUp;
	[swipeup setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipeup];

	swipedown=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipedown:)];
	swipedown.direction=UISwipeGestureRecognizerDirectionDown;
	[swipedown setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipedown];

	swipeupDouble=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipeup:)];
	swipeupDouble.direction=UISwipeGestureRecognizerDirectionUp;
	swipeupDouble.numberOfTouchesRequired=2;
	[swipeupDouble setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipeupDouble];

	swipedownDouble=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipedown:)];
	swipedownDouble.direction=UISwipeGestureRecognizerDirectionDown;
	swipedownDouble.numberOfTouchesRequired=2;
	[swipedownDouble setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipedownDouble];

	swipeupTriple=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipeup:)];
	swipeupTriple.direction=UISwipeGestureRecognizerDirectionUp;
	swipeupTriple.numberOfTouchesRequired=3;
	[swipeupTriple setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipeupTriple];

	swipedownTriple=[[UISwipeGestureRecognizer alloc]initWithTarget:self action:@selector(swipedown:)];
	swipedownTriple.direction=UISwipeGestureRecognizerDirectionDown;
	swipedownTriple.numberOfTouchesRequired=3;
	[swipedownTriple setCancelsTouchesInView:NO];
	[unityViewCtrl.view addGestureRecognizer:swipedownTriple];

/*
	UITapGestureRecognizer * doubleTap=[[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(doubleTap)];
	doubleTap.numberOfTouchesRequired = 1;
	doubleTap.numberOfTapsRequired = 2;
	doubleTap.cancelsTouchesInView = NO;  // this prevents the gesture recognizers to 'block' touches
	[unityViewCtrl.view addGestureRecognizer:doubleTap];
	*/
}

//////////////////////////////////////////////////////////////////////////
/*
-(BOOL)accessibilityViewIsModal
{
	return YES;
}

-(BOOL)accessibilityPerformEscape
{
	UnitySendMessage("UAP_IOSGestures", "OnEscapeGesture", "escape");
}
*/
//////////////////////////////////////////////////////////////////////////
/*
-(void)doubleTap:(UITapGestureRecognizer*)gestureRecognizer 
{
	UnitySendMessage("UAP_IOSGestures", "OnDoubleTap", "tapped");
}
*/

-(void)swipeleft:(UISwipeGestureRecognizer*)gestureRecognizer 
{
	NSUInteger touches = gestureRecognizer.numberOfTouches;
	NSString *strValue = [@(touches) stringValue];
	UnitySendMessage("UAP_IOSGestures", "OnSwipeLeftCallback", [strValue UTF8String]);
}

-(void)swiperight:(UISwipeGestureRecognizer*)gestureRecognizer 
{
	NSUInteger touches = gestureRecognizer.numberOfTouches;
	NSString *strValue = [@(touches) stringValue];
	UnitySendMessage("UAP_IOSGestures", "OnSwipeRightCallback", [strValue UTF8String]);
}

-(void)swipeup:(UISwipeGestureRecognizer*)gestureRecognizer 
{
	NSUInteger touches = gestureRecognizer.numberOfTouches;
	NSString *strValue = [@(touches) stringValue];
	UnitySendMessage("UAP_IOSGestures", "OnSwipeUpCallback", [strValue UTF8String]);
}

-(void)swipedown:(UISwipeGestureRecognizer*)gestureRecognizer 
{
	NSUInteger touches = gestureRecognizer.numberOfTouches;
	NSString *strValue = [@(touches) stringValue];
	UnitySendMessage("UAP_IOSGestures", "OnSwipeDownCallback", [strValue UTF8String]);
}

//////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////

@end

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

extern "C" 
{
	void InitGestureRecognition();
	void RemoveGestureRecognition();
}

//////////////////////////////////////////////////////////////////////////

static iOSGestures_Helper* Gesture_Helper = nil;

//////////////////////////////////////////////////////////////////////////

void InitGestureRecognition()
{
	// Create the Gesture Helper instance if none exists yet
	if(Gesture_Helper == nil)
	{
		Gesture_Helper = [iOSGestures_Helper alloc];
	}

	[Gesture_Helper initGestureListeners];
}

//////////////////////////////////////////////////////////////////////////

void RemoveGestureRecognition()
{
	if(Gesture_Helper == nil)
		return;

	[Gesture_Helper removeGestureListeners];
}

//////////////////////////////////////////////////////////////////////////

