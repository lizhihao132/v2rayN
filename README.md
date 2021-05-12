# v2rayN

### How to use
- If you are newbie please download v2rayN-Core.zip from releases
- Otherwise please download v2rayN.zip (Also need to download v2ray core in the same folder)
- Run v2rayN.exe

### Requirements  
- Microsoft [.NET Framework 4.6](https://docs.microsoft.com/zh-cn/dotnet/framework/install/guide-for-developers) or higher
- Project V core [https://github.com/v2fly/v2ray-core/releases](https://github.com/v2fly/v2ray-core/releases)


### 增加了 web-api, 可以通过 http 接口来控制:
启动 v2rayN 进程后, 做好相应配置(录入代理服务器, 或者订阅某个 vpn-url)
- 切换到下一个 proxy-ip. http://localhost:8888/?cmd=changeProxyServer
- 刷新 proxy-ip 列表(当配置了订阅链接来拉取 proxy-ip, 此选项很好用). http://localhost:8888/?cmd=flushProxyServerList
- 获得 proxy-ip 信息. http://localhost:8888/?cmd=getProxyServerList


### 后记:
- 实际上 https://github.com/2dust/v2rayN 的本质是:
    -- 解析各种 vpn 节点分发协议 (http、vmess 等) 形成配置文件放在 v2ray-Core 的目录下. 见此代码:
        https://github.com/lizhihao132/v2rayN/blob/master/v2rayN/v2rayN/Handler/ShareHandler.cs
    -- 通过重启 v2ray-Core 来实现新配置生效. 见此代码:
        https://github.com/lizhihao132/v2rayN/blob/master/v2rayN/v2rayN/Handler/V2rayHandler.cs