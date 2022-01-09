## Recreating Snake II in Unity

### Motivation

This project was born from my desire to see a project through from its inception to a playable, and hopefully fun, finished product. Snake just happened to be the first project idea that came to mind which seemed in-scope. While doing research for this project I decided to reframe the challenge I had set for myself. Rather than create a game that is vaguely 'snake-like', I would instead make my best effort to recreate the visuals and gameplay of a specific entry in the long line of snake games: Snake II, which was bundled with the Nokia 3310 released in late 2000.

### Objectives

- See the project through to a playable, entertaining end product
- Mirror the original game's aesthetic, art, and gameplay.
- Use milanote to organize tasks and design docs
- Practice using the command pattern

### Recreating the Pixel Art

To recreate the pixel art, I took screenshots of the original gameplay, and sliced up the image (as seen below) to reveal how each segment was constructed. I then recreated each of these segment types as seperate prefab instances in unity using square sprites as pixels. I followed the same procedure to create the pixel art for the snake food and each of the bonus point critter food types. Each grid position in the original game is a 4x4 pixel area, so each prefab was carefully constructed so each pixel lines up properly in it's 4x4 grid area.
![Segment slicing](/images/segment_slicing.png/)
![Critter slicing](/images/critter_slicing.png/)


### [Return to Portfolio](https://2nplusone.github.io/)
