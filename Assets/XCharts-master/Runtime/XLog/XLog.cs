using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace XCharts.Runtime
{
    /// <summary>
    ///     Log system. Used to output logs with date and log type, support output to file, support custom output log type.
    ///     ||日志系统。用于输出带日期和日志类型的日志，支持输出到文件，支持自定义输出的日志类型。
    /// </summary>
    public class XLog : MonoBehaviour
    {
        public const int ALL = 0;
        public const int WARNING = 1;
        public const int DEBUG = 2;
        public const int INFO = 3;
        public const int PROTO = 4;
        public const int VITAL = 5;
        public const int ERROR = 6;
        public const int EXCEPTION = 7;

        private const int MAX_ERROR_LOG = 20;

        public static bool isReportBug;
        public static bool isOutputLog = false;
        public static bool isUploadLog;
        public static bool isCloseOutLog = false;

        public static int errorCount;
        public static int exceptCount;
        public static int uploadTick = 20;
        public static int reportTick = 10;

        private static bool initFileSuccess;
        private static readonly bool[] levelList = { true, true, true, true, true, true, true, true };
        private static readonly List<string> writeList = new();
        private static float uploadTime;
        private static float reportTime;
        public static List<string> errorList = new();
        private static readonly object m_Lock = new();

        public int logCount;

        private string outpath;
        private string[] temp;
        private StreamWriter writer;

        public static XLog Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitLogFile();
            // Application.logMessageReceived += HandleLog;
            Application.logMessageReceivedThreaded += HandleLog;
        }

        private void Update()
        {
            uploadTime += Time.deltaTime;
            reportTime += Time.deltaTime;
            lock (m_Lock)
            {
                if (writeList.Count > 0)
                {
                    logCount = writeList.Count;
                    if (!initFileSuccess)
                    {
                        writeList.Clear();
                        return;
                    }

                    try
                    {
                        temp = writeList.ToArray();
                        var count = 0;
                        foreach (var str in temp)
                        {
                            count++;
                            writer.WriteLine(str);
                            writeList.Remove(str);
                            if (count > 10) break;
                        }

                        writer.Flush();
                    }
                    catch (Exception e)
                    {
                        initFileSuccess = false;
                        //Application.logMessageReceived -= HandleLog;
                        Application.logMessageReceivedThreaded -= HandleLog;
                        UnityEngine.Debug.LogError("write outlog.txt error:" + e.Message);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }

            // Application.logMessageReceived -= HandleLog;
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void InitLogFile()
        {
            ClearAllLog();
            EnableLog(ALL);
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ClearAllLog();
                EnableLog(VITAL);
                EnableLog(ERROR);
                isReportBug = true;
                isUploadLog = true;
            }
            else
            {
                isUploadLog = false;
                isReportBug = false;
            }

            outpath = GetLogOutputPath();
            try
            {
                if (File.Exists(outpath)) File.Delete(outpath);
                writer = new StreamWriter(outpath, false, Encoding.UTF8);
                writer.WriteLine(GetNowTime() + "init file success!!");
                UnityEngine.Debug.Log(GetNowTime() + "init file success:" + outpath);
                writer.Flush();
                initFileSuccess = true;
            }
            catch (Exception e)
            {
                initFileSuccess = false;
                Application.logMessageReceived -= HandleLog;
                UnityEngine.Debug.LogError("write outlog.txt error:" + e.Message);
            }
        }

        private static string GetLogOutputPath()
        {
#if UNITY_EDITOR
            var path = Application.dataPath + "/../outlog.txt";
#else
            string path = Application.persistentDataPath + "/outlog.txt";
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.persistentDataPath + "/outlog.txt";
            }
            else
            {
                path = Application.dataPath + "/../outlog.txt";
            }
#endif
            return path;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            lock (m_Lock)
            {
                if (!initFileSuccess) return;
                var index = logString.IndexOf("stack traceback");
                if (index > 0)
                {
                    var log = logString.Substring(0, index);
                    var trace = logString.Substring(index, logString.Length - index);
                    logString = log;
                    stackTrace = trace;
                }

                if (type == LogType.Log)
                {
                }
                else if (type == LogType.Error)
                {
                    if (logString.IndexOf("LUA ERROR") > 0 || logString.IndexOf("stack traceback") > 0) exceptCount++;
                    else errorCount++;

                    writeList.Add(logString);
                    //writeList.Add(stackTrace + "\n");

                    if (errorList.Count >= MAX_ERROR_LOG) errorList.RemoveAt(1);

                    if (errorList.Count < MAX_ERROR_LOG) errorList.Add(logString);
                    // errorList.Add(stackTrace + "\n");
                }
                else if (type == LogType.Exception)
                {
                    exceptCount++;

                    writeList.Add(logString);
                    writeList.Add(stackTrace + "\n");

                    if (errorList.Count >= MAX_ERROR_LOG) errorList.RemoveAt(1);

                    if (errorList.Count < MAX_ERROR_LOG)
                    {
                        errorList.Add(logString);
                        errorList.Add(stackTrace + "\n");
                    }
                }
            }
        }

        public static void FlushLog()
        {
            var instance = Instance;
            if (instance != null && instance.writer != null)
            {
                for (var i = 0; i < writeList.Count; i++) instance.writer.WriteLine(writeList[i]);
                instance.writer.Flush();
                writeList.Clear();
            }
        }

        public static void EnableLog(int logType)
        {
            if (logType < 0 || logType >= levelList.Length) return;
            levelList[logType] = true;
        }

        public static void ClearAllLog()
        {
            for (var i = 0; i < levelList.Length; i++) levelList[i] = false;
        }

        public static bool CanLog(int level)
        {
            if (level < 0 || level >= levelList.Length) return false;
            return levelList[level] || levelList[0];
        }

        public static void Log(string log)
        {
            Debug(log);
        }

        public static void LogError(string log)
        {
            Error(log);
        }

        public static void LogWarning(string log)
        {
            Warning(log);
        }

        public static void Debug(string log)
        {
            if (!CanLog(DEBUG)) return;
            UnityEngine.Debug.Log(GetNowTime() + "[DEBUG]\t" + log);
        }

        public static void Vital(string log)
        {
            if (!CanLog(INFO)) return;
            UnityEngine.Debug.Log(GetNowTime() + "[VITAL]\t" + log);
        }

        public static void Info(string log)
        {
            if (!CanLog(INFO)) return;
            UnityEngine.Debug.Log(GetNowTime() + "[INFO]\t" + log);
        }

        public static void Proto(string log)
        {
            if (!CanLog(PROTO)) return;
            UnityEngine.Debug.Log(GetNowTime() + "[PROTO]\t" + log);
        }

        public static void Warning(string log)
        {
            if (!CanLog(WARNING)) return;
            UnityEngine.Debug.LogWarning(GetNowTime() + "[WARN]\t" + log);
        }

        public static void Error(string log)
        {
            if (!CanLog(ERROR)) return;
            UnityEngine.Debug.LogError(GetNowTime() + "[ERROR]\t" + log);
        }

        public static string GetNowTime(string formatter = null)
        {
            var now = DateTime.Now;
            if (formatter == null)
                return now.ToString("[HH:mm:ss fff]", DateTimeFormatInfo.InvariantInfo);
            return now.ToString(formatter, DateTimeFormatInfo.InvariantInfo);
        }

        public static ulong GetTimestamp()
        {
            return (ulong)(DateTime.Now - new DateTime(190, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        }
    }
}