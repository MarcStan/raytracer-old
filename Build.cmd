msbuild Raytracer.sln /t:Build /p:Configuration=Release /nr:false
mkdir "!Releases"

echo f|xcopy Raytracer\bin\Release\Raytracer.exe !Releases\Raytracer.exe /y
echo f|xcopy Raytracer\bin\Release\Raytracer.ini !Releases\Raytracer.ini /y