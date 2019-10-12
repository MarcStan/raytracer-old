# A C# 100% software based raytracer I wrote when I was in university

The implementation uses less rays while the player is moving to allow "realtime raytracing" at the cost of quality.

Once no more input is received, a background thread will compute a more detailed image of the scene and replace the backbuffer once it is ready.

![Raytraced scene](/raytraced.png?raw=true)

Uses monogame to render the raytraced scene.

Supports only spheres and planes with different surfaces as of now.

While the user is giving input (mouse or any of the bound keys) the realtime mode is used, which by default has a reduced raster size.

Once the user stops giving input, a background thread is started to raytrace the scene in higher detail. Once finished, the buffer is swapped and the scene is presented to the user with higher details.

![Realtime](/realtime.gif?raw=true)

In this case a scene with 400x400 pixels and a raster size of 4 is raytraced. Which means that actually a scene with 100x100 pixels is raytraced and then upscaled to 400x400. On modern CPUs this takes less than 16ms and can thus run at 60 fps (wheras raytracing a full 400x400 pixels requires 200ms+). When upscaling is needed the user can select point, linear or anisotropic sampling to smooth the scene.

## Features

* Realtime raytracing (with lower detail level)
* Moving camera through the scene (WASD + mouse)
* Accurate raytracing (executed in the background whenever the user stops moving)
* Phong-model based (ambient + diffuse + specular lighting)
* Objects can be added to the scene (as of now only spheres and planes supported)
* Object surfaces can be set per object (reflective, checkerboard, ..)
* Software rendering (entirely on the CPU)
* Soft shadows by using multiple samples per pixel with slightly randomized light positions and averaging the result

**Note on performance: Use the release build without a debugger attached. It will be many times faster!**

# Build

Use Visual Studio or run this build script in the project directory (which assumes MSBuild is in your path):

```
msbuild.exe Raytracer.sln /t:Build /p:Configuration=Release /nr:false
mkdir "!Releases"

echo f|xcopy Raytracer\bin\Release\Raytracer.exe !Releases\Raytracer.exe /y
echo f|xcopy Raytracer\bin\Release\Raytracer.ini !Releases\Raytracer.ini /y
```