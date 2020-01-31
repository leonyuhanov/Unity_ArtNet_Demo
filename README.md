# Unity_ArtNet_Demo
Virtual LED Wall demo in Unity utilizing a simple ArtNet Implementation

Works on UDP port 6454 and expects a single 530byte ArtNet packet per frame. LEDs are organized starting at TOP LEFT than right, then next row starting from the LEFT etc..

This demo is set up for a 13 x 13 px grid that renders a total of 169px. DMX CHanels 0-507 inclusive are used. 508-511 are not used

Example video here https://www.instagram.com/p/B77X5hInP5d/
