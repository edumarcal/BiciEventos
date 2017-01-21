﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teste_PAD
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Edit : Page
    {
        int _first = 0;
        Geopoint _startLocation = null;
        Geopoint _endLocation = null;
        private int _eventId;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var evento = (Event)e.Parameter;
            if (evento != null)
            {
                tb_Title.Text = evento.Title;
                cdp_StartDate.Date = evento.Start_Date;
                tp_Start_Time.Time = TimeSpan.Parse(evento.Start_Time);
                cdp_EndDate.Date = evento.End_Date;
                tp_End_Time.Time = TimeSpan.Parse(evento.End_Time);
                tb_Description.Text = evento.Description;
                _eventId = evento.Id;
                tblock_latitude.Text = evento.start_Latitude.ToString();
                tblock_longitude.Text = evento.start_Longitude.ToString();
            }
            base.OnNavigatedTo(e);


        }

        public Edit()
        {
            this.InitializeComponent();
        }

        private async void b_back_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var eventId = Convert.ToInt32(localSettings.Values["EventId"]);
            HttpClient client = new HttpClient();
            string getUri = "http://localhost:50859/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            var evento = listEvents.SingleOrDefault(a => a.Id == eventId);
            Frame?.Navigate(typeof(Details), evento);
        }

        private void b_Hamburger_Click(object sender, RoutedEventArgs e)
        {
            sv_Menu.IsPaneOpen = !sv_Menu.IsPaneOpen;
        }

        private async void lvi_Logout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["sessionUser"] = null;
            MessageDialog logoutMessage = new MessageDialog("Logout success");
            await logoutMessage.ShowAsync();
            Frame?.Navigate(typeof(MainPage));
        }

        private void lvi_Create_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Index));
        }

        private void lvi_myEvents_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(myEvents));
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["sessionUser"];
            var client = new HttpClient();
            string getUri = string.Format("http://localhost:50859/api/Events/{0}",localSettings.Values["Event_id"].ToString());
            var uri = new Uri(getUri);
            var evento = new Event()
            {
                Id = Convert.ToInt32(localSettings.Values["Event_id"]),
                Title = tb_Title.Text,
                Description = tb_Description.Text,
                Start_Date = cdp_StartDate.Date.Value.DateTime,
                End_Date = cdp_EndDate.Date.Value.DateTime,
                start_Latitude = Convert.ToDouble(localSettings.Values["Event_startLatitude"]),
                end_Latitude = Convert.ToDouble(localSettings.Values["Event_endLatitude"]),
                start_Longitude = Convert.ToDouble(localSettings.Values["Event_endLatitude"]),
                end_Longitude = Convert.ToDouble(localSettings.Values["Event_endLongitude"]),
                Start_Time = tp_Start_Time.Time.ToString(),
                End_Time = tp_End_Time.Time.ToString(),
                Username = value.ToString()
            };
            DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(Event));
            MemoryStream ms = new MemoryStream();
            jsonSer.WriteObject(ms, evento);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            StringContent theContent = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");
            var put_response = await client.PutAsync(uri, theContent);
            var editDialog = new MessageDialog("Changes are saved!");
            await editDialog.ShowAsync();
            Frame?.Navigate(typeof(Details), evento);
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Your changes won't be saved!");
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
            var res = await dialog.ShowAsync();
            if ((int)res.Id == 0)
            {
                Frame?.Navigate(typeof(Main));
            }
        }

        private async void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Geopoint start = new Geopoint(new BasicGeoposition()
            {
                Latitude = Convert.ToDouble(localSettings.Values["Event_startLatitude"]),
                Longitude = Convert.ToDouble(localSettings.Values["Event_startLongitude"]),
            });
            Geopoint end = new Geopoint(new BasicGeoposition()
            {
                Latitude = Convert.ToDouble(localSettings.Values["Event_endLatitude"]),
                Longitude = Convert.ToDouble(localSettings.Values["Event_endLongitude"]),
            });
            MapControl.Center = start;
            MapControl.LandmarksVisible = true;
            MapControl.ZoomLevel = 12;
            MapIcon startIcon = new MapIcon();
            startIcon.Location = start;
            startIcon.ZIndex = 0;
            MapControl.MapElements.Add(startIcon);

            MapIcon endIcon = new MapIcon();
            endIcon.Location = end;
            endIcon.ZIndex = 0;
            MapControl.MapElements.Add(endIcon);

            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(
                        startIcon.Location,
                        endIcon.Location,
                         MapRouteOptimization.Time,
                        MapRouteRestrictions.None
                        );
            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;
                MapControl.Routes.Add(viewOfRoute);
                await MapControl.TrySetViewBoundsAsync(
                    routeResult.Route.BoundingBox,
                    null,
                    Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
            }
        }

        private async void MapControl_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            int icons = Convert.ToInt32(localSettings.Values["icons"]);
            var tappedGeoPosition = args.Location.Position;
            tblock_latitude.Text = tappedGeoPosition.Latitude.ToString();
            tblock_longitude.Text = tappedGeoPosition.Longitude.ToString();
            if ((icons > 1) && (_first != 0))
            {
                tb_Description.Text = icons.ToString();
                MapControl.MapElements.Clear();
                MapControl.Routes.Clear();
                icons = 0;
            }
            else
            {
                MapIcon icon = new MapIcon();
                icon.Location = new Geopoint(tappedGeoPosition);
                icon.ZIndex = 0;
                MapControl.MapElements.Add(icon);
                icons++;
                _first++;
                if (icons == 1)
                    _startLocation = icon.Location;
                else
                {
                    _endLocation = icon.Location;
                    MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(
                    _startLocation,
                    _endLocation,
                     MapRouteOptimization.Time,
                    MapRouteRestrictions.None
                    );
                    if (routeResult.Status == MapRouteFinderStatus.Success)
                    {
                        MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                        viewOfRoute.RouteColor = Colors.Yellow;
                        viewOfRoute.OutlineColor = Colors.Black;
                        MapControl.Routes.Add(viewOfRoute);
                        await MapControl.TrySetViewBoundsAsync(
                            routeResult.Route.BoundingBox,
                            null,
                            Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
                    }
                }

            }
            localSettings.Values["icons"] = icons;
        }

        private void lvi_invite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Invites));
        }
    }
}
