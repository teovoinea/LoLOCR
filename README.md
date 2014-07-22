LoLOCR
======

League of Legends OCR runs on your computer taking screenshots of a league of legends stream on your screen. It will then use
OCR built by Google to parse information into a text file ready to be shipped to a server for app distribution.

How it works
=============
https://i.imgur.com/1AQVZfc.jpg   - Each rectangle will be an area the program will look at at least once to get a piece of
information

Red rectangles with yellow lines represent areas that only need to be looked at once per game
Red rectangles represent areas that need to be constantly checked but their location does not change
Green rectangles represent areas that that need to be constantly checked but their location is variable (depending on whether
the gold screen is showing or not)


By default it is set to refresh the information every minute but you can change it based on your build for faster information
updates or slower if your computer can't handle the stress.

Features to be added
====================
Different screen resolutions. 1920x1080 is next on the list.
