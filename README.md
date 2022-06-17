# Raindrop Viewer
An android client to connect to Secondlife and [OpenSimulator](http://opensimulator.org/wiki/Main_Page) grids, built on the Unity game engine.

This is experimental software.

## What works?
- Login to server/grid works.
- Grid selection from limited list of grids (you can connect to opensim server on localhost no problem. Metaversium grid works too.)
- Decoding of jp2 assets works but in a terrible, hacky way. (Involving modification of texture2D directly, nonetheless!)
- Map view is able to pan, zoom.
- Movement is synchronised with server, but there is no visual-smoothing at all. Janky.
- Object sounds work to a limited extent.

## How to build/contribute?
Please see [Raindrop Viewer Documentation](docs/Readme.md)

## Development Screenshots

![Game/3D Screen](docs/image/game.jpg "Game")

![Main Screen](docs/image/main.jpg "Main")

![Login Screen](docs/image/login.jpg "Login")

![Map Screen](docs/image/map.jpg "Map")

