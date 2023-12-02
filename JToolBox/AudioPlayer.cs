using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace JToolBox
{
    public static class AudioPlayer
    {
        public static async Task PlayEmbeddedResourceAsync(string resourceName)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 构建资源的完整路径
            string resourcePath = $"{assembly.GetName().Name}.{resourceName}";

            // 从程序集中获取资源流
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (resourceStream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }

                // 使用NAudio播放MP3
                using (WaveStream waveStream = new Mp3FileReader(resourceStream))
                {
                    using (WaveOutEvent waveOutEvent = new WaveOutEvent())
                    {
                        // 异步初始化和播放
                        TaskCompletionSource<object> playbackTaskCompletionSource = new TaskCompletionSource<object>();

                        waveOutEvent.Init(waveStream);
                        waveOutEvent.PlaybackStopped += (sender, args) => playbackTaskCompletionSource.SetResult(null);

                        waveOutEvent.Play();

                        // 等待播放完成
                        await playbackTaskCompletionSource.Task;
                    }
                }
            }
        }
    }
}
