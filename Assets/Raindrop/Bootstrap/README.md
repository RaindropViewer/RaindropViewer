## Raindrop.Bootstrap

Contains code and scenes that boots up the app.

### Booting up flowchart: 

#### 1. Unity loads the default scene `BootstrapScene.unity`

This scene solely contains `RaindropBootstrapper.cs`, which calls methods to set up logger, static assets folder, etc. 

After all that, `RaindropBootstrapper.cs` starts the LibreMetaverse library, which is dependent on all the above-mentioned.

As a final step, `RaindropBootstrapper.cs` registers `RaindropInstance` to the `ServiceLocator` Singleton.


#### 2. Load Game Scene: `MainScene.unity`

When `RaindropBootstrapper.cs` finishes  setting up the environment, it loads the scene `MainScene.unity`. 

`MainScene.unity` contains most of the visible parts of Raindrop Viewer:
- entire UI 
- world-space rendered elements/ meshes (map, etc)
- the overall scene-hierachy for avatar, object, and sims.

Since these elements have dependency on the `LibreMetaverse` library, that's why they are solely in this scene. (I have suffered too many null reference exceptions)
