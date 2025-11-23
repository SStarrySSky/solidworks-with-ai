# 开始使用solidworks with ai

### **环境配置**

下载https://download.microsoft.com/download/6/4/2/642ec242-448b-49a1-8371-5d9c202eaa46/NDP48-DevPack-ENU.exe (安装.NET framework 4.8 developer pack)。

推荐使用 JetBrains Rider 作为IDE。

下载Rider (非商用免费) https://www.jetbrains.com.cn/rider/download/?section=windows。

下载JetBrains MSBuild 可再发行组件包 https://download.jetbrains.com/resharper/JetMSBuild.zip?_gl=1。

在rider的设置>构建，执行，部署>工具包和构建中将MSBuild版本路径替换为你下载的那个。

接下来在Rider中打开此项目。

若有关于框架的报错，将.NET framework替换为4.8即可，选择你刚刚的安装路径。

### AI与插件配置

打开https://platform.openai.com/api-keys，在manage选项卡中找到API keys, 创建你的api-key并复制。

在AiPlanner.cs中第38行找到“your api key”，将引号里的内容替换为你刚刚复制的api key。

右击AISW项目，选择添加，选择引用，将开头为solidworks的项改为你自己计算机solidworks目录下的dll的路径

SolidWorks.Interop.sldworks.dll

SolidWorks.Interop.swconst.dll

SolidWorks.Interop.swconst.dll

打开PowerShell输入

```
[guid]::NewGuid()
```

将得到的Guid替换swaddin.cs中第12行的Guid以及所有包含的项

点击上方锤头按钮构建。

构建完毕后，软件会在项目根目录的ALSW/bin/Debug下生成AISW.dll，复制该dll的路径。

接下来，打开有**管理员**权限的 PowerShell 或命令行。

执行命令（路径换成你刚刚复制的dll的路径）：

```
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
  "C:\YourPath\SwAiAddin\bin\x64\Debug\SwAiAddin.dll" `
  /codebase

```

若第一行命令不成功，请换成你找到的计算机的 `regasm`路径。

最后，在 SolidWorks 里启用 Add-in即可，启动SolidWorks可能会提示connecttoSW被调用,代表插件加载成功！

提醒：若使用插件跳出472报错则代表openai api用额不足，需要充值。