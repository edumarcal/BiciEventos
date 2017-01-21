﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teste_PAD
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Main : Page
    {
        public Main()
        {
            this.InitializeComponent();
        }

        private void lb_Icons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void b_Hamburger_Click(object sender, RoutedEventArgs e)
        {
            sv_Menu.IsPaneOpen = !sv_Menu.IsPaneOpen;
        }

        private async void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            string getUri = "http://localhost:50859/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            foreach (Event item in listEvents)
            {
                ListBoxItem lb = new ListBoxItem();
                lb.Content = item.Title;
                lb_Events.Items.Add(lb);
            }
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Allowed_to_Edit"] = false;
        }

        private async void lvi_Logout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["sessionUser"] = null;
            MessageDialog logoutMessage = new MessageDialog("Logout success");
            await logoutMessage.ShowAsync();
            Frame?.Navigate(typeof(MainPage));
        }

        private async void lb_Events_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            HttpClient client = new HttpClient();
            string getUri = "http://localhost:50859/api/Events";
            string getUriEG = "http://localhost:50859/api/Event_Going";
            Uri uri = new Uri(getUri);
            Uri uriEG = new Uri(getUriEG);
            var response = await client.GetStringAsync(uri);
            var responseEG = await client.GetStringAsync(uriEG);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            List<Event_Going> events = JsonConvert.DeserializeObject<List<Event_Going>>(response);
            var evento = listEvents.FirstOrDefault(x => x.Title == ((ListBoxItem)lb_Events.SelectedValue).Content.ToString());
            tblock_Title.Text = evento.Title;
            var usersParticipations = events.FindAll(x => x.EventId.Equals(evento.Id));
            localSettings.Values["Users_Participating"] = usersParticipations.Count.ToString();
            localSettings.Values["start_latitude"] = evento.start_Latitude;
            localSettings.Values["start_longitude"] = evento.start_Longitude;
            localSettings.Values["end_latitude"] = evento.end_Latitude;
            localSettings.Values["end_longitude"] = evento.end_Longitude;
            localSettings.Values["EventId"] = evento.Id;
            if (evento.Username == localSettings.Values["sessionUser"].ToString())
            {
                localSettings.Values["Allowed_to_Edit"] = true;
            }
            Frame?.Navigate(typeof(Details), evento);
        }

        private void lvi_Create_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Index));
        }

        private async void b_Search_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            string query = tb_Search.Text;
            string getUri = "http://localhost:50859/api/Events";
            Uri uri = new Uri(getUri);
            var response = await client.GetStringAsync(uri);
            List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(response);
            List<Event> events = listEvents.FindAll(x => x.Title.Contains(query));
            lb_Events.Items.Clear();
            foreach (Event item in events)
            {
                ListBoxItem lb = new ListBoxItem();
                lb.Content = item.Title;
                lb_Events.Items.Add(lb);
            }
        }
        private void lvi_myEvents_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(myEvents));
        }

        private void lvi_invite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(Invites));
        }

        private void lvi_Change_Password_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame?.Navigate(typeof(ChangePassword));
        }
    }
}
