using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Util;
using Android.Graphics;
using Android.Runtime;
using Android;
using Android.Content.PM;
using static Android.Gms.Vision.Detector;
using Android.Support.V4.App;
using System.Text;
using System.Text.RegularExpressions;

namespace TesteOCR
{
    [Activity(Label = "TesteOCR", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback, IProcessor
    {

        private SurfaceView cameraView;
        private TextView textView;
        private CameraSource cameraSource;
        private const int RequestCameraPermissionID = 1001;
        private static int cont = 0;
        private static int contTextCorrect = 0;



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCameraPermissionID:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(cameraView.Holder);
                        }
                    }
                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            cameraView = FindViewById<SurfaceView>(Resource.Id.surface_view);
            textView = FindViewById<TextView>(Resource.Id.text_view);


            TextRecognizer textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!textRecognizer.IsOperational)
            {

            }

            //Log.Error("Main Activity", "As dependencias ainda não estão prontas para serem carregadas!");
            else
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();
                cameraView.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);

                //var camera =  Android.Hardware.Camera.Open();
                //Parameters parameters = camera.GetParameters();
                //parameters.FlashMode = Parameters.FlashModeOn;
                //camera.SetParameters(parameters);
                //camera.StartPreview();


            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera
                }, RequestCameraPermissionID);
                return;
            }
            cameraSource.Start(cameraView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            cameraSource.Stop();
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            StringBuilder strBuilder = new StringBuilder();


            if (items.Size() != 0)
            {
                textView.Post(() =>
                {
                    for (int i = 0; i < items.Size(); i++)
                    {
                        strBuilder.Append(((TextBlock)items.ValueAt(i)).Value + " " + "\n");
                    }
                    //var textRemoved = Regex.Replace(strBuilder.ToString(), "[A-Za-z ]", " ");
                    string text = GetTextMatch(strBuilder.ToString());
                    if (!string.IsNullOrEmpty(text))
                    {
                        contTextCorrect++;
                        textView.Text = text + " Lendo o valor pela :" + contTextCorrect + " vez.";
                    }
                    else
                    {
                        contTextCorrect = 0;
                        cont = 0;
                        textView.Text = "No Price.";
                    }
                });
            }
            else
            {
                cont++;

                textView.Post(() =>
                {
                    textView.Text = "Não reconheceu nada: " + cont;
                });
            }
        }

        private string GetTextMatch(string text)
        {
            Match match = null;
            var regex_1 = new Regex(@"(R\$|RS|\$|S) ?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$*");

            //var regex_1  = new Regex(@"\$ ?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$*");
            //var regex_2 = new Regex(@"RS ?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?S*");
            //var regex_3 = new Regex(@"R\$ ?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$*");

            match = regex_1.Match(text);
            if (regex_1.Match(text).Success) return match.Value;

            return null;
        }

        public void Release()
        {

        }
    }
}

