using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace WebBrowserApp
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class AnotherRemoteObject
    {

        // Sample property.
        public string Prop { get; set; } = "AnotherRemoteObject.Prop";
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class BridgeAddRemoteObject
    {
        // Sample function that takes no parameters.
        public string getVoices()
        {
            return "BridgeAddRemoteObject.Func2(),mock";
        }

    }
    public partial class Form1 : Form
    {
        TextPlayer player;
        public Form1()
        {
            InitializeComponent();
            this.Resize += this.onFormResize;
            initializeAsync();
            this.player = new TextPlayer();
            this.player.stateChangeHandler += onPlayStateChanged;
            this.player.voiceChangedHandler += onVoiceChanged;
        }
        private async void initializeAsync() {
            await webView.EnsureCoreWebView2Async(null);

            webView.CoreWebView2.WebMessageReceived += onWebMessageReceived;
            webView.CoreWebView2.AddHostObjectToScript("interop", player.interop);
        }

        private void onWebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            {
                var s = e.TryGetWebMessageAsString();
                try
                {
                    var msg = JsonSerializer.Deserialize<WebMessageEvent>(s);
                    if (msg.Event == "command")
                    {
                        var args = JsonSerializer.Deserialize<HostCommand>(msg.Arguments);
                        if (args.CommandName == "play")
                        {
                            this.onPlay(args.CommandArguments);
                        }
                        else if (args.CommandName == "pause")
                        {
                            this.onPause();
                        }
                        else if (args.CommandName == "stop")
                        {
                            this.onStop();
                        }
                        else if (args.CommandName == "changeVoice")
                        {
                            this.onChangeVoice(args.CommandArguments);
                        }
                    } 
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            {
                // foreach (var voice in synthesizer.GetInstalledVoices())
                // {
                //     var info = voice.VoiceInfo;
                //     var strVoice = ($"Id: {info.Id} | Name: {info.Name} | Age: { info.Age} | Gender: { info.Gender} | Culture: { info.Culture}");
                //     Console.WriteLine(strVoice);
                // }
            }

            {
                // var synthesizer = new SpeechSynthesizer();
                // var builder = new PromptBuilder();
                // builder.AppendTextWithHint("3rd", SayAs.NumberOrdinal);
                // builder.AppendBreak();
                // builder.AppendTextWithHint("3rd", SayAs.NumberCardinal);
                // synthesizer.Speak(builder);
            }
            
        }

        private void onFormResize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
        }

        #region command from Web
        private void onPlay(string txt)
        {
            try
            {
                if (this.player.State == PlayState.playing)
                {
                    player.Pause();
                    return;
                }
                else if (this.player.State == PlayState.paused)
                {
                    player.Play("");
                    return;
                }
                if (txt.Length > 0)
                {
                    this.player.Play(txt);
                }
                else
                {
                    using (var reader = new StreamReader(@"c:\temp\demo.txt"))
                    {
                        var str = reader.ReadToEnd();
                        this.player.Play(str);
                    }
                }
            }
            catch (IOException ec)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ec.Message);
            }
        }
        void onPause()
        {
            this.player.Pause();
        }
        void onStop()
        {
            this.player.Stop();
        }
        void onChangeVoice(string name)
        {
            this.player.changeVoice(name);
        }
        #endregion

        #region player state changed
        private void onPlayStateChanged(PlayState state)
        {
            var stateChangedEvent = new WebMessageEvent
            {
                Status = 0,
                Event = "playStateChanged",
                Arguments = state.ToString()
            };
            webView.CoreWebView2.PostWebMessageAsString(JsonSerializer.Serialize<WebMessageEvent>(stateChangedEvent));
        }
        private void onVoiceChanged(string name)
        {
            var voiceChangedEvent = new WebMessageEvent
            {
                Status = 0,
                Event = "voiceChanged",
                Arguments = name,
            };
            webView.CoreWebView2.PostWebMessageAsString(JsonSerializer.Serialize<WebMessageEvent>(voiceChangedEvent));
        }
        #endregion

    }

    internal class WebMessageEvent
    {
        public int Status { get; set; }
        public string Event { get; set; }
        public string Arguments { get; set; }
    }
    internal class HostCommand
    {
        public string CommandName { get; set; }
        public string CommandArguments { get; set; }
    }

}
