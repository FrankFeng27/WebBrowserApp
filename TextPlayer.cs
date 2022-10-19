using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Text.Json;

namespace WebBrowserApp
{
    internal delegate void PlayStateChangedHandler(PlayState state);
    internal delegate void VoiceChangedHandler(string name);
    public enum PlayState
    {
        unknown = 0,
        playing = 1,
        paused = 2,
        completed = 3,
    }
    class TextPlayer
    {
        public TextPlayer()
        {
            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            State = PlayState.unknown;
            this.initialize();
        }
        public void Play(string txt)
        {
            if (this.State == PlayState.paused)
            {
                this.synthesizer.Resume();
            }
            else
            {
                synthesizer.SpeakAsync(txt);
            }
        }
        public void Pause()
        {
            if (this.State == PlayState.playing)
            {
                synthesizer.Pause();
            }
        }
        public void Stop()
        {
            var prevState = this.State;
            this.State = PlayState.completed;
            if (prevState == PlayState.paused)
            {
                synthesizer.Resume();
            }
            synthesizer.SpeakAsyncCancelAll();
        }
        public void changeVoice(string name)
        {
            try
            {
                synthesizer.SelectVoice(name);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void initialize()
        {
            synthesizer.StateChanged += onPlayStateChanged;
            synthesizer.SpeakCompleted += onPlayCompleted;
            synthesizer.VoiceChange += onVoiceChanged;
            this.interop = new PlayerInterop(this.synthesizer);
        }

        private void onVoiceChanged(object sender, VoiceChangeEventArgs e)
        {
            notifyVoiceChanged(e.Voice.Name);
        }

        private void onPlayCompleted(object sender, SpeakCompletedEventArgs e)
        {
            this.State = PlayState.completed;
            this.notifyStateChanged(this.State);
        }

        private void onPlayStateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.State == SynthesizerState.Paused)
            {
                this.State = PlayState.paused;
            }
            if (e.State == SynthesizerState.Speaking)
            {
                this.State = PlayState.playing;
            }
            this.notifyStateChanged(this.State);
        }
        private void notifyStateChanged(PlayState state)
        {
            if (stateChangeHandler != null)
            {
                stateChangeHandler(state);
            }
        }
        private void notifyVoiceChanged(string name)
        {
            if (voiceChangedHandler != null)
            {
                voiceChangedHandler(name);
            }
        }

        SpeechSynthesizer synthesizer = null;
        public PlayerInterop interop { get; private set; } = null;
        public PlayState State { get; private set; }
        public PlayStateChangedHandler stateChangeHandler;
        public VoiceChangedHandler voiceChangedHandler;

    }
}
