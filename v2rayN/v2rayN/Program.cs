using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using v2rayN.Forms;
using v2rayN.Properties;
using v2rayN.Tool;

namespace v2rayN
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static MainForm __appForm = null;
        private static int defaultServerIndex = 0;

        public static void WriteLog(Exception ex, string LogAddress = "")
        {
            //如果日志文件为空，则默认在Debug目录下新建 YYYY-mm-dd_Log.log文件
            if (LogAddress == "")
            {
                LogAddress = Environment.CurrentDirectory + '\\' +
                    DateTime.Now.Year + '-' +
                    DateTime.Now.Month + '-' +
                    DateTime.Now.Day + "_Log.log";
            }

            //把异常信息输出到文件
            StreamWriter sw = new StreamWriter(LogAddress, true);
            sw.WriteLine("当前时间：" + DateTime.Now.ToString());
            sw.WriteLine("异常信息：" + ex.Message);
            sw.WriteLine("异常对象：" + ex.Source);
            sw.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            sw.WriteLine("触发方法：" + ex.TargetSite);
            sw.WriteLine();
            sw.Close();
        }

        public static HttpListener listener;
        public static void startHttpListen()
        {
            System.Console.WriteLine("begin to listener ...");
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/"); //添加需要监听的url范围
            listener.Start(); //开始监听端口，接收客户端请求
            
            listener.BeginGetContext(ListenerHandle, listener);

            System.Console.WriteLine("finished listener ...");
        }

        private static string handleCmd(string cmd) {
            if(cmd == "changeProxyServer")
            {
                System.Console.WriteLine("now will change default server ...");
                try
                {
                    __appForm._SetDefaultServer(++ defaultServerIndex);
                    if (defaultServerIndex > 999999999)
                    {
                        defaultServerIndex = 0;
                    }
                    return "change success, use:" + defaultServerIndex;
                }
                catch (Exception ex) {
                    WriteLog(ex);
                    return ex.Message;
                }
            }
            else if(cmd == "flushProxyServerList")
            {
                try
                {
                    __appForm._UpdateSubscriptionProcess();
                    defaultServerIndex = 0;

                    Tuple<int> res = __appForm._GetProxyServerListInfo();
                    var total = res.Item1;
                    return "flush success, total:" + total + ", use:" + (defaultServerIndex % total);
                }catch(Exception ex)
                {
                    WriteLog(ex);
                    return ex.Message;
                }
            }
            else if(cmd == "getProxyServerList")
            {
                try
                {
                    Tuple<int> res = __appForm._GetProxyServerListInfo();
                    var total = res.Item1;
                    return "total:" + total + ", use:" + (defaultServerIndex % total);
                }
                catch(Exception ex)
                {
                    WriteLog(ex);
                    return ex.Message;
                }
                
            }
            return "";
        }

        private static void ListenerHandle(IAsyncResult result)
        {
            try
            {
                //listener = result.AsyncState as HttpListener;
                if (listener.IsListening)
                {
                    System.Console.WriteLine("accept request ...");

                    listener.BeginGetContext(ListenerHandle, result);
                    HttpListenerContext context = listener.EndGetContext(result);
                    //解析Request请求
                    HttpListenerRequest request = context.Request;
                    string cmd = "";
                    switch (request.HttpMethod)
                    {
                        case "POST":
                            {
                                System.Console.WriteLine("post ...");
                                Stream stream = context.Request.InputStream;
                                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                cmd = reader.ReadToEnd();
                            }
                            break;
                        case "GET":
                            {
                                System.Console.WriteLine("get ...");
                                var data = request.QueryString;
                                cmd = data.Get("cmd");
                            }
                            break;
                    }

                    string retStr = handleCmd(cmd);

                    //构造Response响应
                    HttpListenerResponse response = context.Response;
                    response.StatusCode = 200;
                    response.ContentType = "application/json;charset=UTF-8";
                    response.ContentEncoding = Encoding.UTF8;
                    response.AppendHeader("Content-Type", "application/json;charset=UTF-8");
                    response.AppendHeader("Access-Control-Allow-Origin", "*");  //允许跨域.

                    using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
                    {
                        string res = "{\"res\":\"" + retStr + "\"}";
                        System.Console.WriteLine("return response: " + res);
                        writer.Write(res);
                        writer.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                //ex.Message;
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            startHttpListen();

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);


            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!IsDuplicateInstance())
            {

                Utils.SaveLog("v2rayN start up " + Utils.GetVersion());

                //设置语言环境
                string lang = Utils.RegReadValue(Global.MyRegPath, Global.MyRegKeyLanguage, "zh-Hans");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                __appForm = new MainForm();
                Application.Run(__appForm);
            }
            else
            {
                UI.ShowWarning($"v2rayN is already running(v2rayN已经运行)");
            }
        }

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    try
        //    {
        //        string resourceName = "v2rayN.LIB." + new AssemblyName(args.Name).Name + ".dll";
        //        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        //        {
        //            if (stream == null)
        //            {
        //                return null;
        //            }
        //            byte[] assemblyData = new byte[stream.Length];
        //            stream.Read(assemblyData, 0, assemblyData.Length);
        //            return Assembly.Load(assemblyData);
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /// <summary> 
        /// 检查是否已在运行
        /// </summary> 
        public static bool IsDuplicateInstance()
        {
            //string name = "v2rayN";

            string name = Utils.GetExePath(); // Allow different locations to run
            name = name.Replace("\\", "/"); // https://stackoverflow.com/questions/20714120/could-not-find-a-part-of-the-path-error-while-creating-mutex
            
            Global.mutexObj = new Mutex(false, name, out bool bCreatedNew);
            return !bCreatedNew;
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Utils.SaveLog("Application_ThreadException", e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Utils.SaveLog("CurrentDomain_UnhandledException", (Exception)e.ExceptionObject);
        }
    }
}
