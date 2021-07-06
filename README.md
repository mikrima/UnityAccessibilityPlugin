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

# Installation
1. Download or clone this repo.
2. Copy the UAP folder into your project's asset folder.
3. Follow the Quick Start Guide either using the included PDF or the online guide <a href="http://www.metalpopgames.com/assetstore/accessibility/doc/QuickStart.html">here</a>.
4. Watch the basic tutorial video on <a href="https://www.youtube.com/watch?v=SJuQWf7p9T4&ab_channel=MetalPopGames">Youtube</a>.

# Basic Functionality
This plugin brings screen reader functionality to Unity apps, making them usable for blind users (without making 
them unusable for everyone else).

## Developers
UI elements need to be marked up as accessible so that the UAP can recognize them, read them aloud to users, 
and allow interaction. This is done by adding accessibility-components to the GameObjects that are relevant.<br>

## Blind Users
The controls are based on popular screen readers like VoiceOver, NVDA and TalkBack, so that blind users will not 
have to relearn a new method of control.<br>
The plugin will try to detect a screen reader running in the background and enable itself if it does. Otherwise 
it will sit dormant and not interfere with the app (making it usable for non-blind users).

# Documentation
The documentation, how-to guides and further examples can be found <a href="http://www.metalpopgames.com/assetstore/accessibility/doc/index.html">here</a>.<br>
There's also a tutorial video demonstrating the basic setup: <a href="https://www.youtube.com/watch?v=SJuQWf7p9T4">Basic Tutorial Video</a>.<br>
Here's a forum thread for discussion and support on the Unity forums: <a href="https://forum.unity.com/threads/released-ui-accessibility-plugin-uap-v1-0.469298/?_ga=2.92342237.1961910733.1618848783-1844297938.1510951995">UI Accessibility Plugin</a>

# License & Asset Usage Rights
The UAP plugin itself is made available under the Apache 2.0 license. See included license file for details.<br>
This license does **not** apply to the demo and example content.

## Example Scene
All assets inside the Example folder (or its subfolders) of this plugin are for sample purposes only and cannot be redistributed, sold or used in your products (free or commercial).<br>
The UI for the Match 3 game example was created by Vasili Tkach.<br>
It is available for free here: <a href="https://dribbble.com/shots/2261532--Funtique-game-UI-kit-free-PSD">Funtique UI by Vasili Tkach</a>

## Third Party Licensing Terms
Windows only:<br>
UAP includes the shared NVDA screen reader controller client library for 64 and 32 Bit.
The library can be found in the folders UAP/Plugins/x86 and UAP/Plugins/x86_64 respectively.
The NVDA Controller Client API is licensed under the GNU Lesser General Public License (LGPL), version 2.1 (Source)<br>
In simple terms, this library can be used in any application, but cannot be modified.<br>
Link to full license text: <a href="https://github.com/nvaccess/nvda/blob/master/extras/controllerClient/license.txt">https://github.com/nvaccess/nvda/blob/master/extras/controllerClient/license.txt</a>

# FAQ
<b>*Does this make VoiceOver recognize my Unity app?*</b><br>
No. This plugin works and acts like a screen-reader - it does not expose Unity's UI to native screen readers.
It basically reimplements the functionality of VoiceOver (and then some). Exposing the UI tree to the native 
screen readers is not feasible, because UGUI and NGUI don't work this way.
<br><br>
<b>*Can this plugin circumvent the problem with TalkBack blocking input?*</b><br>
TalkBack blocks input to Unity on an OS level, and this plugin can not circumvent that. 
TalkBack can be detected if it is running, and the plugin can enable accessibility automatically, but the 
input will still need to be unblocked by pausing TalkBack while interacting with the app.
The plugin will prompt the user to pause TalkBack.
<br><br>
<b>*On iOS 14 VoiceOver only says 'Direct Touch Area' - does the UAP not work in iOS 14?*</b><br>
UAP works fine in iOS 14. However, Apple changed the default behaviour for non-native UI views in this version of iOS. 
As a result, many accessible apps now 'appear' to no longer work. But all the user has to do is reenable direct touch 
using the rotor gesture. Blind users are familiar with this process (and have likely already changed the 
OS default behaviour in their device settings). This topic has been discussed in several forum threads.<br>
As this is caused by the OS, it isn't something that this plugin can automatically circumvent. It is possible 
that future versions of Unity will register the correct view parameters and solve this issue.<br>
More on the topic can be found here: <a href="https://developer.apple.com/forums/thread/663529">Direct Touch Interaction Broken in iOS 14?</a>
<br><br>
<b>*Can I delete the Examples folder?*</b><br>
Yes. It is safe to delete the entire "Examples" folder from the plugin if it isn't needed.<br>
Also keep in mind that the assets used in the examples do not fall under the Apache 2.0 license, and cannot be 
used in other projects. See section **License & Asset Usage Rights** further up on this page.
<br><br>
<b>*Is this plugin free to use in commercial projects?*</b><br>
Yes.<br>
Please note that the assets used in the Examples folder do not fall under the Apache 2.0 license, and may not be 
used in such projects. See section **License & Asset Usage Rights** further up on this page.
