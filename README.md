# v2rayN

### How to use
- If you are newbie please download v2rayN-Core.zip from releases
- Otherwise please download v2rayN.zip (Also need to download v2ray core in the same folder)
- Run v2rayN.exe

### Requirements  
- Microsoft [.NET Framework 4.6](https://docs.microsoft.com/zh-cn/dotnet/framework/install/guide-for-developers) or higher
- Project V core [https://github.com/v2fly/v2ray-core/releases](https://github.com/v2fly/v2ray-core/releases)


### 增加了 web-api, 可以通过 http 接口来控制:
- 切换到下一个 proxy-ip. http://localhost:8888/?cmd=changeProxyServer
- 刷新 proxy-ip 列表(当配置了订阅链接来拉取 proxy-ip, 此选项很好用). http://localhost:8888/?cmd=flushProxyServerList
- 获得 proxy-ip 信息. http://localhost:8888/?cmd=getProxyServerList