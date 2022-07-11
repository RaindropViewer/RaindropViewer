Test the various decode and encode methods.

the primary formats we are concerned with are:

- JP2, the actual data from the server
- tga, the format typically used by the library to store images in disk.
- managedimage, the format the library uses to do baking (layering textures-with-alpha on top of another to create a combined texture). (appearance.cs, BakeLayer.cs)
- texture2D, the unity-side interface to upload textures onto the gpu, or just to modify it at runtime (main thread only)
  - Note that each Texture2D object will internally have different formats, such as plain/bitmap/no-encoding, ASTC, ETC1, etc.  
- 