cd Producer
dotnet publish --configuration Release --runtime linux-x64 --output ..\published-linux-x64 --self-contained False
cd ..\Consumer
dotnet publish --configuration Release --runtime linux-x64 --output ..\published-linux-x64 --self-contained False
cd ..\ConsumerHttp
dotnet publish --configuration Release --runtime linux-x64 --output ..\published-linux-x64 --self-contained False
cd ..
