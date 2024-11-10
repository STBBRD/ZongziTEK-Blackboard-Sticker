using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.IO;
using Edge_tts_sharp;
using System.Net.NetworkInformation;
using Edge_tts_sharp.Model;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public static class TTSHelper
    {
        public static void PlayText(string text)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                EdgeTTSPlayText(text);
            }
            else
            {
                SysTTSPlayText(text);
            }
        }

        private static void EdgeTTSPlayText(string text)
        {
            Task.Run(() =>
                {
                    var voice = Edge_tts.GetVoice()[MainWindow.Settings.TimetableSettings.Voice];

                    PlayOption option = new()
                    {
                        Rate = 1,
                        Text = text
                    };

                    Edge_tts.PlayText(option, voice);
                });
        }

        private static void SysTTSPlayText(string text)
        {
            SpeechSynthesizer synthesizer = null;

            try
            {
                synthesizer = new SpeechSynthesizer();
            }
            catch
            {
                // 系统 TTS 不存在
            }

            if (synthesizer != null)
            {
                synthesizer.SpeakAsync(text);
            }
        }
    }
}
