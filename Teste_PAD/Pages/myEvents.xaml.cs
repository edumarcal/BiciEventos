﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Teste_PAD.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teste_PAD.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyEvents : Page
    {
        public MyEvents()
        {
            this.InitializeComponent();
        }

        private async void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["sessionUser"];

            HttpClient client = new HttpClient();
            string getUri = "http://localhost:5000/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            List<Event> myEvents = listEvents.FindAll(x => x.UserId == int.Parse(value.ToString()));
            foreach (Event item in myEvents)
            {
                ListBoxItem lb = new ListBoxItem {Content = item.Title};
                lb_Events.Items.Add(lb);
            }
        }
        private void lvi_Create_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Index));
        }
        private async void lb_Events_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HttpClient client = new HttpClient();
            string getUri = "http://localhost:5000/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var evento = listEvents.SingleOrDefault(x => x.Title == ((ListBoxItem)lb_Events.SelectedValue).Content.ToString());
            tblock_Title.Text = evento.Title;
            localSettings.Values["start_latitude"] = evento.StartLatitude;
            localSettings.Values["start_longitude"] = evento.StartLongitude;
            localSettings.Values["end_latitude"] = evento.EndLatitude;
            localSettings.Values["end_longitude"] = evento.EndLongitude;
            if (evento.User.Id == int.Parse(localSettings.Values["sessionUser"].ToString()))
            {
                localSettings.Values["Allowed_to_Edit"] = true;
            }
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
        private async void b_Search_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["sessionUser"];
            HttpClient client = new HttpClient();
            string query = tb_Search.Text;
            string getUri = "http://localhost:5000/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            List<Event> events = listEvents.FindAll(x => x.Title.Contains(query) || x.User.Id == int.Parse(value.ToString()));
            lb_Events.Items.Clear();
            foreach (Event item in events)
            {
                ListBoxItem lb = new ListBoxItem {Content = item.Title};
                lb_Events.Items.Add(lb);
            }
        }

        private void lvi_Main_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Main));
        }

        private void lvi_invite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Invites));
        }
    }
}
