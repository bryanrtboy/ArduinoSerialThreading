# ArduinoSerialThreading
Project that implements the [Touche](http://www.instructables.com/id/Touche-for-Arduino-Advanced-touch-sensing/) method based on the Disney labs project for detecting gestures with a single wire. This implentation is specifically for Unity, creating a threaded serial read for the incoming Arduino data and plotting graphs using particles, objects and lines. 

The lines are stored for comparing to the current curve reading to stored curves. To store curves, click the button when you are touching, grabbing, etc. 

![gif](https://github.com/bryanrtboy/ArduinoSerialThreading/blob/master/water.gif)
![gif](https://github.com/bryanrtboy/ArduinoSerialThreading/blob/master/orchid.gif)

Also includes a method for getting the name of the current serial port in the player build, allowing users to interactively switch to the correct port. All settings are stored for future launches of the application.

The original Processing sketch for Touche seemed glitchy, and appeared to only compare the highest y value of each curve, rather than the curve as a whole. This improves upon that by comparing the entire stored curve to the current curve read from the Arduino, always defaulting to the lowest total cumulative distance between each point of the curves. 
