using System;
using System.Collections.Generic;

namespace SuperSocketClient.Common
{
    internal class LogHelper
    {
        public static List<string> AllLines = new List<string>();
        public static int DisplayLength;

        public static string SetOnLog()
        {
            var count = AllLines.Count;
            if (DisplayLength == count) return "";
            //最多保留log行数
            if (AllLines.Count > 5000)
            {
                AllLines.RemoveRange(0, 20);
                DisplayLength = AllLines.Count;
            }
            return string.Join("\r\n", AllLines);
        }

        /// <summary>
        ///  普通的文件记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void WriteLog(string info)
        {
            Console.WriteLine(@"===>[{0}]<===", info);

            AllLines.Add(string.Join(" ", DateTime.Now.ToString("HH:mm:ss"), info));
        }
    }
}