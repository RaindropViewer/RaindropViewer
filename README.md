# LMV_unity / Raindrop viewer
## A viewer for secondlife and metaverse sims, target for android.

Use Unity engine as crossplatform framework and library galore

Use LibreMetaverse library for client APIs. 

Use FMOD for audio

## Key findings
### Imaging
- System.Bitmap and System.Drawing is tightly integrated into the OpenMetaverse/LibreMetaverse library. 
  - I have since changed all Bitmap references to unity's Texture2D.
- Bitmap reading/drawing seems to take 100x on android compared to laptop. 
  - slow GPU uploading? to profile.
- J2C is computationally expensive to decode (to profile; apparently in the order of 0.3s on PC), so we will have to get the J2C from tex pipeline, decode, then save as other raw/compressed textures (bitmap? GPU format?) to disk..
- turn off/ minimal texture fetching when in low-network more (metered internet).
  - fetchy-ness factor in settings?
### consistency of connection
- limitiation - activity lifecycle
  - android kills apps when switching due to memory pressure. 
    - user may answer a call, causing app to close.
      - when this happens, the connection state can be non-determinatistic
        - to avoid non-deterministic connection, our best bet is to force a logoff to the server, then reconnect once the app is switched back.
  - our aim is to:
    - preserve the illusion of connected-ness as long as the user did not log off.
      - switching-off the app ALWAYS triggers log-off in the client API.
        - upon returning to the app
          - app is still alive : internally re-connect to server and do *not* reload scene.
          - app is dead : internally re-connect to server. have to reload scene.
          
