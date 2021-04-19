# UnityAccessibilityPlugin
The UI Accessibility Plugin (UAP) allows you to make your UI accessible to blind and visually impaired players on Windows, Android, iOS, Mac and WebGL. 
Tested in Unity 5.6 to Unity 2020.1.

# Feature Overview
- Full source code, including native code for Windows and Android libraries
- UGUI & NGUI compatible
- Supports multiple languages (and more can be added)
- Works like VoiceOver & TalkBack

# Supported Platforms
- Android
- iOS
- Windows (Windwos SAPI and NVDA)
- Mac
- WebGL (some limitations apply)

# Basic Functionality
This plugin brings screen reader functionality to Unity apps, making them usable for blind users (without making 
them unusable for everyone else).

## Developers
UI elements need to be marked up as accessible so that the UAP can recognize them, read them aloud to users, 
and allow interaction. This is done by adding accessibility-components to the GameObjects that are relevant.

## Blind Users
The controls are based on popular screen readers like VoiceOver, NVDA and TalkBack, so that blind users will not 
have to relearn a new method of control.
The plugin will try to detect a screen reader running in the background and enable itself if it does. Otherwise 
it will sit dormant and not interfere with the app (making it usable for non-blind users).

# Documentation
The documentation, how-to guides and further examples can be found <a href="http://www.metalpopgames.com/assetstore/accessibility/doc/index.html">here</a>.<br>
There's also a tutorial video demonstrating the basic setup: <a href="https://www.youtube.com/watch?v=SJuQWf7p9T4">Basic Tutorial Video</a>.

# FAQ
<u><b>Does this make VoiceOver recognize my Unity app?</b></u><br>
No. This plugin works and acts like a screen-reader - it does not expose Unity's UI to native screen readers.
It basically reimplements the functionality of VoiceOver (and then some). Exposing the UI tree to the native 
screen readers is not feasible, because UGUI and NGUI don't work this way.
<br><br>
<u><b>Can this plugin circumvent the problem with TalkBack blocking input?</b></u><br>
TalkBack blocks input to Unity on an OS level, and this plugin can not circumvent that. 
TalkBack can be detected if it is running, and the plugin can enable accessibility automatically, but the 
input will still need to be unblocked by pausing TalkBack while interacting with the app.
The plugin will prompt the user to pause TalkBack.
<br><br>
<u><b>On iOS 14 VoiceOver only says 'Direct Touch Area' - does the UAP not work in iOS 14?</b></u><br>
UAP works fine in iOS 14. However, Apple changed the default behaviour for non-native UI views in this version of iOS. 
As a result, many accessible apps now 'appear' to no longer work. But all the user has to do is reenable direct touch 
using the rotor gesture. Blind users are familiar with this process (and have likely already changed the 
OS default behaviour in their device settings). This topic has been discussed in several forum threads.<br>
As this is caused by the OS, it isn't something that this plugin can automatically circumvent. It is possible 
that future versions of Unity will register the correct view parameters and solve this issue.<br>
More on the topic can be found here: <a href="https://developer.apple.com/forums/thread/663529">Direct Touch Interaction Broken in iOS 14?</a>
