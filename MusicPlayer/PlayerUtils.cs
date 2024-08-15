
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public static class PlayerUtils
    {
        public static string CurrentMusic;
        [DllImport("winmm.dll")]
        public static extern long mciSendString(string strCommand,StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        public static bool Play(string filePath, IntPtr intPtr)
        {
            int startIndex = filePath.LastIndexOf("\\") + 1;
            int endIndex = filePath.IndexOf(".")- startIndex;
            CurrentMusic = filePath.Substring(startIndex, endIndex);
            mciSendString("close music", null, 0, IntPtr.Zero);
            mciSendString($"open \"{filePath}\" alias music", null, 128, IntPtr.Zero);
            return mciSendString("play music notify", null, 128, intPtr) == 0;
        }

        public static void Stop()
        {
            //mciSendString("stop music", null, 0, IntPtr.Zero);
            mciSendString(@"close music", null, 0, IntPtr.Zero);
        }

        public static void Pause()
        {
            mciSendString("pause music", null, 0, IntPtr.Zero);
        }

        public static void Resume()
        {
            mciSendString("resume music", null, 0, IntPtr.Zero);
        }
        public static void GetMusicLength()
        {
            StringBuilder sb = new StringBuilder();
            mciSendString("status music length", sb, 1024, IntPtr.Zero);
            var length = sb.ToString();

        }
    }
}