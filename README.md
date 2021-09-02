# winform-obs
winform-obs

# 環境建置
## 建立OBS環境
+ 安裝 [OBS軟體](https://obsproject.com/download) 目前使用版本27.0.1


+ 為本機OBS安裝 [websocket插件](https://github.com/Palakis/obs-websocket/releases/tag/4.9.1)，這樣WinForm才有辦法連接發指令。[obs-websocket API文件](https://github.com/Palakis/obs-websocket/blob/4.x-current/docs/generated/protocol.md)

+ 開啟obs設定port位及密碼
  
+ 設定輸出錄影格式
    + 類型：自訂輸出(FFmpeg)
    + 封裝格式：mpegts(目前使用jsmpeg播放)
    + 影像位元率(kbit/s)：10000kbps(越大越清楚) 
    + 影像編碼器：mpeg1video
    + 音效位元率(kbit/s)：128kbps
    + 音軌：勾選1(全勾選會有效能問題，影片播放變超慢)
    + 音效編碼器：mp2(要注意錄影時主介面下方輸出音效是否有反應，沒有表示沒錄到聲音)

## 建立winform專案
+ Windows Forms App(.NET Framework 4.6.1)
  + 設定Form1.cs寬高
  + 啟動OBS調整顯示器擷取範圍
  
+ 建立WebView
  + 下載 [EO.Browser](https://www.essentialobjects.com/Download.aspx) 相關檔案並安裝
  + 安裝完畢照[說明](https://www.essentialobjects.com/doc/webbrowser/start/winform.aspx)操作VisualStudio
  + 開啟VisualStudio後, 先確認工具箱是否有EO.WebBrowser.WebControl
  + 沒有就從"工具->選擇工具箱項目->瀏覽"，找到安裝目錄(C:\Program Files\Essential Objects\EO.Total 2021)內的dll套件
    + EO.WebBrowser.dll
    + EO.WebBrowser.WinForm.dll
  + 再看看專案"參考"是否存在EO.WebBrowser、EO.WebBrowser.WinForm
  + 若有缺漏，右鍵點擊"參考"加入參考，同樣瀏覽dll檔案並加入
  + 在工具箱EO.WebBrowser找到WebControl，拖到視窗內同時自動創建WebView，設定寬高、名稱(預設webView1)

+ obs-websocket
    + 直接使用NuGet安裝 [obs-weboscket-dotnet4.9.0套件](https://github.com/BarRaider/obs-websocket-dotnet)
    + 建立obs-webscoket實體

# 流程串接
+ 對Form1.cs介面雙擊生成基本代碼，在From1建構式內指定URL
   ```csharp
   webView1.Url = "https://google.com.tw";
   ```
+ 設定記憶體上限(否則會有記憶體不足問題)
   ```csharp
   EO.Base.Runtime.EnableEOWP = true;
   ```
+ 監聽OBS連接事件並測試連線
   ```csharp
   _obs = new OBSWebsocket();
   _obs.Connected += onConnect;
   _obs.Connect("ws://127.0.0.1:4444", "123456");
   ```
+ 連線成功開始取得OBSScene實體並設定
   ```csharp
   private void onConnect(object sender, EventArgs e)
    {
        //更換影片放置資料夾
        _obs.SetRecordingFolder("D:\\git\\VisualStudio\\WebViewTest\\WebViewTest\\video");
        string profile = _obs.GetCurrentProfile();
        OBSScene scene = _obs.GetCurrentScene();
        this.isConnect = true;
    }
   ```
+ WebView添加自定義function(Form1.Designer.cs)
    ```csharp
   //對webview註冊自定義function
   this.webView1.RegisterJSExtensionFunction("demoAbout", new JSExtInvokeHandler(WebView_JSDemoAbout));
   ```
+ 接著讓webview內的網頁在適當時機呼叫對外function並帶入參數
    ```typescript
    window['demoAbout']("start_record");
    ```
+ WinForm接收通知後開始/停止錄影
    ```csharp
    void WebView_JSDemoAbout(object sender, JSExtInvokeArgs e)
    {
        if (this.isConnect == false)
        {
            Console.WriteLine("尚未連線");
        }
        string command = e.Arguments[0] as string;
        if (command == "start_record")
        {
            _obs.StartRecording();
        }
        else
        {
            _obs.StopRecording();

            //等待5秒存檔，並上傳FTP
            wait(5000);
            this.UploadFtpFile("Client\\Test\\Ray", "D:\\git\\VisualStudio\\WebViewTest\\WebViewTest\\video\\aaa.ts");
        }
    }
    ```