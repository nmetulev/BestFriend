# BestFriend

Bot implementaion for [PersonalityForge](http://www.personalityforge.com/), integrated with Cortana and Voice services on the Universal Windows Platform

## Setup

1. Ensure Cortana is signed in with an MSA account. This can be achieved by opening Cortana once and following the sign-in process. 
2. Run the application normally once (eg, via F5 debug or deploy/launch). This installs the voice command definitions.
3. Close the app.
4. Click on the microphone icon in Cortana's search bar. 
5. Say one of the supported voice commands (see below)

(Note: it may take a small amount of time for Cortana to refresh its installed voice commands.)

## Usage

When Cortana is listening, any of the following voice commands are supported.

- "Best Friend, I want to talk" - opens the app and starts conversation with voice
- "Best Friend, let me tell you, {message}" - sends a message to the bot without opening the app. The response is returned in canvas

There are more commands supported, but you should find them youself.

## Related topics

-  [Cortana design guidelines](https://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn974233.aspx)
-  [Cortana interactions (XAML)](https://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn974230.aspx)
-  [Cortana interactions (HTML)](https://msdn.microsoft.com/en-us/library/windows/apps/dn974231.aspx)

## System requirements

* Cortana requires an appropriate recording device, and the system must be associated with a Microsoft Account in order for Cortana to function.
* Windows 10 RTM
* [Visual Studio 2015 Community or higher](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx)
