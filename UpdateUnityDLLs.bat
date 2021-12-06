del ".\MUCOUnityClient\Assets\MUCONet.dll"
del ".\MUCOUnityClient\Assets\MUCONet.dll.meta"
echo F|xcopy ".\MUCONet\bin\Debug\netcoreapp3.1\MUCONet.dll" ".\MUCOUnityClient\Assets\MUCONet.dll"

del ".\MUCOUnityServer\Assets\MUCONet.dll"
del ".\MUCOUnityServer\Assets\MUCONet.dll.meta"
echo F|xcopy ".\MUCONet\bin\Debug\netcoreapp3.1\MUCONet.dll" ".\MUCOUnityServer\Assets\MUCONet.dll"

pause