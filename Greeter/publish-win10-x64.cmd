cd Producer
dotnet publish --configuration Release --runtime win10-x64 --output ..\published-win10-x64 --self-contained False
cd ..\Consumer
dotnet publish --configuration Release --runtime win10-x64 --output ..\published-win10-x64 --self-contained False
cd ..\ConsumerHttp
dotnet publish --configuration Release --runtime win10-x64 --output ..\published-win10-x64 --self-contained False
cd ..
