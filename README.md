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

## Users
The controls are based on popular screen readers like VoiceOver, NVDA and TalkBack, so that blind users will not 
have to relearn a new method of control.
The plugin will try to detect a screen reader running in the background and enable itself if it does. Otherwise 
it will sit dormant and not interfere with the app (making it usable for non-blind users).

## Developers
UI elements need to be marked up as accessible so that the UAP can recognize them, read them aloud to users, 
and allow interaction. This is done by adding accessibility-components to the GameObjects that are relevant.

# Documentation
The documentation, how-to guides and further examples can be found <a href="http://www.metalpopgames.com/assetstore/accessibility/doc/index.html">here</a>.
There's also a tutorial video demonstrating the basic setup: <a href="https://www.youtube.com/watch?v=SJuQWf7p9T4">Basic Tutorial Video</a>.

# FAQ
<b>Does this make VoiceOver recognize my Unity app?</b>
No. This plugin works and acts like a screen-reader - it does not expose Unity's UI to native screen readers.
It basically reimplements the functionality of VoiceOver (and then some). Exposing the UI tree to the native 
screen readers is impossible, because both UGUI and NGUI work in a different way.

<b>Can this plugin circumvent the problem with TalkBack blocking input?</b>
TalkBack blocks input to Unity on an OS level, and this plugin can not circumvent that. 
TalkBack can be detected if it is running, and the plugin can enable accessibility automatically, but the 
input will still need to be unblocked by pausing TalkBack while interacting with the app.
The plugin will prompt the user to pause TalkBack.

<b>On iOS 14 VoiceOver only says 'Direct Touch Area' - does the UAP not work in iOS 14?</b>
