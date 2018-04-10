using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Provider;
using Android.Runtime;
using Android.Graphics;
using Java.IO;
using Android.Content.PM;
using Android.Net;
using System.Collections.Generic;
using Android.Speech;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Android;

namespace RealWear
{
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }
    [Activity(Label = "RealWear", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : Activity
    {
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView textBox;
        private Button recButton;
        ImageView _imageView;

        public async Task CheckPermission()
        {
            bool arePermissionsGranted = false;
            
            //return arePermissionsGranted;
        }
       

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string permission = Manifest.Permission.ReadExternalStorage;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                string[] permissions = { Manifest.Permission.ReadExternalStorage };
                RequestPermissions(permissions, 0);
            }
            permission = Manifest.Permission.ReadExternalStorage;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                string[] permissions = { Manifest.Permission.WriteExternalStorage };
                RequestPermissions(permissions, 0);
            }
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var btnCamera = FindViewById<Button>(Resource.Id.btnCamera);
            if (IsThereAnAppToTakePictures())
            {
                
                CreateDirectoryForPictures();
                Button button = FindViewById<Button>(Resource.Id.btnCamera);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView);
                button.Click += TakeAPicture;
            }


            // set the isRecording flag to false (not recording)
            isRecording = false;

            // check to see if we can actually record - if we can, assign the event to the button
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    textBox.Text = "No microphone present";
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
            {
                isRecording = !isRecording;
                if (isRecording)
                {
                    // create the intent and start the activity
                    /*var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);*/

                    // put a message on the modal dialog
                    //voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                    // if there is more then 1.5s of silence, consider the speech over

                    /*
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);*/

                    // you can specify other languages recognised here, for example
                    // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                    // if you wish it to recognise the default Locale language and German
                    // if you do use another locale, regional dialects may not be recognised very well

                    /*voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                    StartActivityForResult(voiceIntent, VOICE);*/
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultCode == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                        {
                            textInput = textInput.Substring(0, 500);
                        }
                        if (textInput == "click picture" || textInput == "open camera")
                        {
                            StartActivity(typeof(CustomCamera));
                            //Intent intent = new Intent(MediaStore.ActionImageCapture);
                            //App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
                            //intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
                            //StartActivityForResult(intent, 0);
                            //StartActivityForResult(intent, 0);
                        }
                        //StartActivity(typeof(CustomCamera));

                    }
                }
            }
            else
            {
                // Make it available in the gallery
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);

                // Display in ImageView. We will resize the bitmap to fit the display.
                // Loading the full sized image will consume to much memory
                // and cause the application to crash.

                int height = Resources.DisplayMetrics.HeightPixels;
                int width = _imageView.Height;
                App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
                if (App.bitmap != null)
                {
                    _imageView.SetImageBitmap(App.bitmap);
                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    App.bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    byte[] imageByteArray = stream.ToArray();
                    stream.Dispose();
                    Task<string> visionAPIResult = getVisionAPIData(imageByteArray);
                    App.bitmap = null;
                }

                // Dispose of the Java side bitmap.
                GC.Collect();
            }
            base.OnActivityResult(requestCode, resultCode, data);
            //StartActivity(visionCall);
        }
        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "RealWearHMT");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }
        async Task getCameraPermission()
        {
            string[] permissions = { Manifest.Permission.Camera };
            RequestPermissions(permissions, 0);
            
        }
        private async void TakeAPicture(object sender, EventArgs eventArgs)
        {
            string permission = Manifest.Permission.Camera;
            if (CheckSelfPermission(permission) != (int)Permission.Granted)
            {
                await getCameraPermission();
            }
            else
            {
                Intent intent = new Intent(MediaStore.ActionImageCapture);
                App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
                StartActivityForResult(intent, 0);
            }
        }
        private async Task<string> getVisionAPIData(byte[] byteData)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["iterationId"] = "{string}";
            queryString["application"] = "{string}";
            var uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/c0ef94ad-d356-4566-b717-7a88fe0de68d/image?" + queryString;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", "9ba86598cacf4ec69d768ab91911fa89");
            client.BaseAddress = new System.Uri(uri);
            HttpResponseMessage response;
            string responseContent;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = client.PostAsync(uri, content).Result;
                responseContent = response.Content.ReadAsStringAsync().Result;
            }
            return responseContent.ToString();
        }
    }
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }


            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
            return resizeAndRotate(resizedBitmap, outWidth, outHeight);
        }
        public static Bitmap resizeAndRotate(Bitmap image, int width, int height)
        {
            var matrix = new Matrix();
            var scaleWidth = ((float)width) / image.Width;
            var scaleHeight = ((float)height) / image.Height;
            matrix.PostRotate(90);
            matrix.PreScale(scaleWidth, scaleHeight);
            return Bitmap.CreateBitmap(image, 0, 0, image.Width, image.Height, matrix, true);
        }
    }
}

