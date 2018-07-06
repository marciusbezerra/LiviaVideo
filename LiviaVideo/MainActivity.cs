using Android.App;
using Android.Widget;
using Android.OS;
using static Android.Media.MediaPlayer;
using Android.Media;
using Android.Content.PM;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using Android.Preferences;
using Android.Runtime;
using System.IO;
using System.Linq;

namespace LiviaVideo
{
    [Activity(Label = "Vídeos Lívia!", MainLauncher = true, Icon = "@mipmap/icon",
        ScreenOrientation = ScreenOrientation.Landscape, Theme = "@style/NoActionBar")]
    public class MainActivity : Activity, IOnCompletionListener, IOnPreparedListener, IOnErrorListener
    {
        private VideoView videoLivia;
        private int currentVideo = 0;
        private List<string> videoUrls = new List<string>();
        private const string PrefKioskMode = "pref_kiosk_mode";
        private ProgressDialog progress;

        private readonly string[] VideoDirs =
        {
            "/sdcard",
            "/sdcard/Videos",
            "/storage/extSdCard",
            "/storage/external_SD",
            "/storage/5800-E221",
        };

        public void OnCompletion(MediaPlayer mp)
        {
            currentVideo++;
            if (currentVideo >= videoUrls.Count)
                currentVideo = 0;
            PlayNextVideo();
        }

        private void PlayNextVideo()
        {
            try
            {
                progress = new ProgressDialog(this);
                //progress.Window.SetType(WindowManagerTypes.SystemAlert);
                progress.SetMessage("Aguarde Lívia, carregando dezeninho...");
                progress.SetCanceledOnTouchOutside(false);
                progress.Show();
                Android.Net.Uri uri = Android.Net.Uri.Parse(videoUrls[currentVideo]);
                videoLivia.SetVideoURI(uri);
                videoLivia.RequestFocus();
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }

        }

        public void OnPrepared(MediaPlayer mp)
        {
            videoLivia.Start();
            progress.Dismiss();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.DismissKeyguard);

            //var on = FindViewById<Button>(Resource.Id.turnOn);
            //var off = FindViewById<Button>(Resource.Id.turnOff);

            var sp = PreferenceManager.GetDefaultSharedPreferences(this);

            //on.Click += (_, __) => {
            var edit = sp.Edit();
            edit.PutBoolean(PrefKioskMode, true);
            edit.Commit();
            //};

            //off.Click += (_, __) => {
            //    var edit = sp.Edit();
            //    edit.PutBoolean(PrefKioskMode, false);
            //    edit.Commit();
            //};


            SetContentView(Resource.Layout.Main);

            videoLivia = FindViewById<VideoView>(Resource.Id.videoViewLivia);

            videoLivia.SetOnCompletionListener(this);
            videoLivia.SetOnErrorListener(this);
            videoLivia.SetOnPreparedListener(this);

            foreach (var videoDirectory in VideoDirs)
            {
                if (Directory.Exists(videoDirectory))
                {
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.mp4"));
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.MP4"));
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.avi"));
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.AVI"));
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.webm"));
                    videoUrls.AddRange(Directory.GetFiles(videoDirectory, "*.WEBM"));
                }
            }

            videoUrls.Shuffle();

            PlayNextVideo();
        }

        public override void OnBackPressed()
        {
            // Disable pressing back, yo!
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (!hasFocus)
            {
                var closeDialog = new Intent(Intent.ActionCloseSystemDialogs);
                SendBroadcast(closeDialog);
            }
        }

        //Optional: Disable buttons (i.e. volume buttons)
        private readonly IList<Keycode> _blockedKeys = new[] { Keycode.VolumeDown, Keycode.VolumeUp };

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (_blockedKeys.Contains(e.KeyCode))
                return true;

            return base.DispatchKeyEvent(e);
        }

        public bool OnError(MediaPlayer mp, [GeneratedEnum] MediaError what, int extra)
        {
            Toast.MakeText(this, what.ToString(), ToastLength.Long).Show();
            PlayNextVideo();
            return true;
        }
    }
}

