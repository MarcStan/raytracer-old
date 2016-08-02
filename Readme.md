# A C# based raytracer

![Raytraced scene](/raytraced.png?raw=true)

Uses monogame to render the raytraced scene.

Supports only spheres and planes with different surfaces as of now.

The raytracer has two modes: realtime and background.

While the user is giving input (mouse or any of the bound keys) the realtime mode is used, which by default has a reduced raster size. Once the user stops giving input, a background thread is started to raytrace the scene in higher detail. Once finished, the buffer is swapped and the scene is presented to the user with higher details.

![Realtime](/realtime.gif?raw=true)

In this case a scene with 400x400 pixels and a raster size of 4 is raytraced. The raster size of 4 means that 4x4 pixel blocks always receive the same value. Essentially a 100x100 scene is raytraced and upscaled to 400x400. On modern CPUs this takes less than 16ms and can thus run at 60 fps (wheras raytracing a full 400x400 pixels requires 200ms+).