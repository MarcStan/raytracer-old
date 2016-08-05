REM If msbuild is not in your path, add it or use full path such as C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe
msbuild.exe Raytracer.sln /t:Build /p:Configuration=Release /nr:false
mkdir "!Releases"

echo f|xcopy Raytracer\bin\Release\Raytracer.exe !Releases\Raytracer.exe /y
echo f|xcopy Raytracer\bin\Release\Raytracer.ini !Releases\Raytracer.ini /y