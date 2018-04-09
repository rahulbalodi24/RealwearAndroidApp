using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using Android.Views;
using Android.Hardware;
using Java.IO;
using System;
using Android.Content.PM;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace RealWear
{
    [Activity(Label = "Custom Camera", MainLauncher = false, ScreenOrientation = ScreenOrientation.Landscape)]
    class CustomCamera : Activity, Android.Hardware.Camera.IPictureCallback, Android.Hardware.Camera.IPreviewCallback, Android.Hardware.Camera.IShutterCallback, ISurfaceHolderCallback
    {
        Camera camera;
        String PICTURE_FILENAME = String.Format("myPhoto_{0}.jpg", Guid.NewGuid());
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CustomCamera);

            SurfaceView surface = (SurfaceView)FindViewById(Resource.Id.customCameraSurface);
            var holder = surface.Holder;
            holder.AddCallback(this);
            holder.SetType(Android.Views.SurfaceType.PushBuffers);

            FindViewById(Resource.Id.customCameraClick).Click += delegate
            {

                Android.Hardware.Camera.Parameters p = camera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                camera.SetParameters(p);
                camera.TakePicture(this, this, this);
            };
        }

        void Camera.IPictureCallback.OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            FileOutputStream outStream = null;
            Java.IO.File dataDir = Android.OS.Environment.ExternalStorageDirectory;
            if (data != null)
            {
                try
                {
                    outStream = new FileOutputStream(dataDir + "/" + String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
                    outStream.Write(data);
                    outStream.Close();
                    Task<string> visionAPIResult =  getVisionAPIData(data);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    System.Console.Out.WriteLine(e.Message);
                }
                catch (System.IO.IOException ie)
                {
                    System.Console.Out.WriteLine(ie.Message);
                }
            }
        }

        void Camera.IPreviewCallback.OnPreviewFrame(byte[] b, Android.Hardware.Camera c)
        {

        }

        void Camera.IShutterCallback.OnShutter()
        {

        }


        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                camera = Android.Hardware.Camera.Open();
                Android.Hardware.Camera.Parameters p = camera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                camera.SetParameters(p);
                camera.SetPreviewCallback(this);
                camera.Lock();
                camera.SetPreviewDisplay(holder);
                camera.StartPreview();
                Thread.Sleep(2000);
                FindViewById(Resource.Id.customCameraClick).PerformClick();
            }
            catch (System.IO.IOException e)
            {
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

            camera.Unlock();
            camera.StopPreview();
            camera.SetPreviewCallback(null);
            camera.Release();
            camera = null;
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format f, int i, int j)
        {


        }

        private async Task<string> getVisionAPIData(byte[] byteData)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["iterationId"] = "{string}";
            queryString["application"] = "{string}";
            var uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/c0ef94ad-d356-4566-b717-7a88fe0de68d/image?" + queryString;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", "9ba86598cacf4ec69d768ab91911fa89");
            client.BaseAddress = new Uri(uri);
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
}