using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowserApp
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class PlayerInterop
    {
        private SpeechSynthesizer synthesizer = null;
        public PlayerInterop(SpeechSynthesizer synth)
        {
            this.synthesizer = synth;
        }
        public string getVoices()
        {
            var voices = synthesizer.GetInstalledVoices();
            var list = new List<string>();
            foreach(var v in voices)
            {
                list.Add(v.VoiceInfo.Name);
            }
            return string.Join(",", list);
        }
        public string getCurrentVoice()
        {
            return synthesizer.Voice.Name;
        }
        // Get type of an object.
        public string GetObjectType(object obj)
        {
            return obj.GetType().Name;
        }

        // Sample property.
        public string Prop { get; set; } = "PlayerInterop.Prop";
    }
}
