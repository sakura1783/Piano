using System.Runtime.InteropServices;
using UnityEngine;

namespace iOSNative
{
    public static class EnableBackgroundAudioBridge
    {
        [DllImport("__Internal")]
        static extern void __enableBackgroundAudio();

        /// <summary>
        /// バックグラウンドでのオーディオ再生を有効化
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_IOS")]
        public static void EnableBackgroundAudio()
        {
            __enableBackgroundAudio();
        }

        // NOTE: 自動有効化が不要なら切ること
        [RuntimeInitializeOnLoadMethod]
        static void AutoEnabling()
        {
            EnableBackgroundAudio();
        }
    }
}
