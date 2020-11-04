using System;
using Android;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Android.Content;
using Android.Widget;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Portal;

namespace ShapeFile_Viewer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MapView _myMapView;
        public string filepath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature layer (shapefile)";
            //  ArcGISRuntimeEnvironment.SetLicense(licenseKey);
            //licence();
            // Register a portal that uses OAuth authentication with the AuthenticationManager 
            Esri.ArcGISRuntime.Security.ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri("https://www.arcgis.com/sharing/rest"),
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo { ClientId = "KCp9D7P2CjITzzlz", RedirectUri = new Uri("https://developers.arcgis.com") }
            };

            AuthenticationManager.Current.RegisterServer(serverInfo);
            CreateLayout();
            Initialize();
        }

        public async void licence()
        {

            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,

            };

            // Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri("http://www.arcgis.com/sharing/rest");

            try
            {
                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(loginInfo, false);

                // Connect to the portal (ArcGIS Online) using the credential
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(loginInfo.ServiceUri, cred);

                // Get LicenseInfo from the portal
                LicenseInfo licenseInfo = await arcgisPortal.GetLicenseInfoAsync();
                // ... code here to license the app immediately and/or save the license (JSON string) to take the app offline ...
                // License the app using the license info
                ArcGISRuntimeEnvironment.SetLicense(licenseInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to initialize the ArcGIS Runtime with the client ID provided: " + ex.Message);

                // TODO: handle exceptions
            }
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Get the path to the downloaded shapefile
            //  string filepath = GetShapefilePath();

            try
            {
                // Open the shapefile
                //ArcGISRuntimeEnvironment.SetLicense(licenseKey);
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);
                // ArcGISRuntimeEnvironment.SetLicense(licenseKey);

                // Create a feature layer to display the shapefile
                FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

                // Add the feature layer to the map
                _myMapView.Map.OperationalLayers.Add(newFeatureLayer);
                //ArcGISRuntimeEnvironment.SetLicense(licenseKey);

                // Zoom the map to the extent of the shapefile
                await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent, 50);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to initialize the ArcGIS Runtime with the client ID provided: " + ex.Message);
            }
        }

        // [assembly: Dependency(typeof(FileSystemImplementation))]
        private static string GetShapefilePath()
        {
            Context context = Android.App.Application.Context;
            var filePath = context.GetExternalFilesDir("");


            return filePath.Path;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout
            Button btnbutton = new Button(this);
            layout.AddView(btnbutton);
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            btnbutton.Click += myCustomClick;
            // FindViewById<Button>(Resource.Id.btnbutton).Click += (o, e) =>
            // GetShapefilePath();
            // Show the layout in the app
            SetContentView(layout);




            /*   SetContentView(Resource.Layout.activity_main);
            testLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            // Add a map view to the layout
            _myMapView = new MapView(this);
            testLayout.AddView(_myMapView);*/
        }

        public async void myCustomClick(object o, EventArgs e)
        {
            //handle click here
            //  string[] allowedTypes = { "application/shp" };

            FileData filedata = await CrossFilePicker.Current.PickFile();
            string filename = filedata.DataArray.ToString();
            filepath = filedata.FilePath.ToString();

            CreateLayout();
            Initialize();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}