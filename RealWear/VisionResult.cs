using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace RealWear
{

    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public double Probability { get; set; }
    }

    public class RootObject
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public DateTime Created { get; set; }
        public List<Prediction> Predictions { get; set; }
    }
    [Activity(Label = "Vision Result", MainLauncher = false, ScreenOrientation = ScreenOrientation.Landscape)]
    class VisionResult : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VisionResult);
            Button backToMain = FindViewById<Button>(Resource.Id.backToMainButton);
            string visionData = Intent.GetStringExtra("visionData") ?? "NA";
            //string jsonString = @"{""movies"":[{""TagID"":""1"",""Tag"":""Sherlock"",""Probability"":""Sherlock""},{""TagID"":""2"",""Tag"":""The Matrix"",""Probability"":""The Matrix""}]}";
            var model = JsonConvert.DeserializeObject<RootObject>(visionData);
            List<Prediction> predictionList = model.Predictions.ToList();
            string result = "";
            int i = 0;
            foreach (var item in predictionList)
            {
                i++;
                result +=i.ToString()+". " +"Item : " + item.Tag + " ,Probability :" + item.Probability + "\n";
            }
            TextView textView= FindViewById<TextView>(Resource.Id.visionResultTextView);
            textView.Text=result;
            backToMain.Click += delegate
            {
                var backToMainActivity = new Intent(this, typeof(MainActivity));
                StartActivity(backToMainActivity);
            };
        }
    }
}