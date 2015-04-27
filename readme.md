## YouTube/Twitch Buddy

Small WPF app for showing a combined list of all your favorite YouTube and Twitch channels.

I made this because I wanted to have a single list containing all the latest videos of the YouTube and Twitch channels I was following.

### Getting started

Insert YouTube and/or Twitch channel names in App.config. 


##### YouTube

Let's say you wish to view all videos by Microsoft (https://www.youtube.com/user/Microsoft) and Apple (https://www.youtube.com/user/Apple).
The channel name is the last part of the URL.

In this case, the YouTube settings in App.config (channel names are separated by '|'):

<add key="YouTubeChannels" value="Microsoft|Apple"/>

##### Twitch

The same goes for Twitch channels (e.g. http://www.twitch.tv/microsoft/):

<add key="TwitchChannels" value="Microsoft|Apple"/>

### Contact

Twitter: [@orjanhorpestad](https://twitter.com/orjanhorpestad)