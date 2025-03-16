using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWP_Template_Test_SQL.Helpers;
using UWP_Template_Test_SQL.Services;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Template_Test_SQL.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MapControlPage : Page
    {
        public class RouteResponse
        {
            public string Type { get; set; }
            public List<Feature> Features { get; set; }
        }

        public class Feature
        {
            public string Type { get; set; }
            public Properties Properties { get; set; }
            public Geometry Geometry { get; set; }
        }

        public class Properties
        {
            public double Distance { get; set; }
            public double Duration { get; set; }
        }

        public class Geometry
        {
            public string Type { get; set; }
            public List<double[]> Coordinates { get; set; } // Координаты маршрута
        }
        public MapControlPage()
        {
            this.InitializeComponent();
            // Координаты Пинска, Беларусь
            double longitude = 26.0951;
            double latitude = 52.1139;
            int zoomLevel = 13;

            // Формируем URL с параметрами
            string yandexMapsUrl = $"https://yandex.ru/maps/?ll={longitude},{latitude}&z={zoomLevel}";

            // Загрузка Яндекс.Карт с начальной точкой и зумом
            yandexMapWebView.Navigate(new Uri(yandexMapsUrl));

            //    string yandexMapHtml = @"
            //<!DOCTYPE html>
            //<html>
            //<head>
            //    <meta charset=""utf-8"">
            //    <title>Яндекс.Карты</title>
            //    <script src=""https://api-maps.yandex.ru/2.1/?apikey=dc002810-6522-49ab-a436-cdd9cff342f5&lang=ru_RU"" type=""text/javascript""></script>
            //    <script type=""text/javascript"">
            //        ymaps.ready(init);

            //        var myMap;

            //        function init() {
            //            myMap = new ymaps.Map('map', {
            //                center: [55.7558, 37.6176], // Москва
            //                zoom: 10
            //            });

            //            // Добавление поиска
            //            var searchControl = new ymaps.control.SearchControl({
            //                options: {
            //                    provider: 'yandex#search'
            //                }
            //            });
            //            myMap.controls.add(searchControl);

            //            // Добавление маршрутизации
            //            var routeControl = new ymaps.control.RouteEditor();
            //            myMap.controls.add(routeControl);

            //            // Добавление других элементов управления
            //            myMap.controls.add('zoomControl');
            //            myMap.controls.add('typeSelector');
            //            myMap.controls.add('fullscreenControl');
            //        }

            //        // Функция для добавления точки
            //        function addPoint(lat, lon) {
            //            var placemark = new ymaps.Placemark([lat, lon], {
            //                balloonContent: 'Точка маршрута'
            //            });
            //            myMap.geoObjects.add(placemark);
            //        }

            //        // Функция для получения информации о маршруте
            //        function getRouteInfo() {
            //            var route = myMap.controls.get('routeEditor').getRoutes().get(0);
            //            var distance = route.properties.get('distance').text;
            //            var duration = route.properties.get('duration').text;
            //            return JSON.stringify({ distance: distance, duration: duration });
            //        }
            //    </script>
            //    <style>
            //        html, body {
            //            margin: 0;
            //            padding: 0;
            //            width: 100%;
            //            height: 100%;
            //            overflow: hidden;
            //        }
            //        #map {
            //            width: 100%;
            //            height: 100%;
            //        }
            //    </style>
            //</head>
            //<body>
            //    <div id=""map""></div>
            //</body>
            //</html>";

            //    yandexMapWebView.NavigateToString(yandexMapHtml);
            yandexMapWebView.NavigationCompleted += YandexMapWebView_NavigationCompleted;
        }
        private async void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            double lat = 55.7558;
            double lon = 37.6176;
            await yandexMapWebView.InvokeScriptAsync("addPoint", new[] { lat.ToString(), lon.ToString() });
        }

        private async void GetRouteInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string routeInfo = await yandexMapWebView.InvokeScriptAsync("getRouteInfo", null);
            Debug.WriteLine(routeInfo);
        }
        private void YandexMapWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                Debug.WriteLine("Карта успешно загружена.");
            }
            else
            {
                Debug.WriteLine($"Ошибка загрузки карты: {args.WebErrorStatus}");
            }
        }
        //private async void CalculateRouteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    double startLat = 55.7558; // Москва
        //    double startLon = 37.6176;
        //    double endLat = 59.9343;  // Санкт-Петербург
        //    double endLon = 30.3351;

        //    await CalculateRouteAsync(startLat, startLon, endLat, endLon);
        //}

        //private async Task CalculateRouteAsync(double startLat, double startLon, double endLat, double endLon)
        //{
        //    string requestUrl = $"https://api-maps.yandex.ru/services/route/2.0/?apikey=dc002810-6522-49ab-a436-cdd9cff342f5&rll={startLon},{startLat}~{endLon},{endLat}&results=1";

        //    using (HttpClient client = new HttpClient())
        //    {
        //        HttpResponseMessage response = await client.GetAsync(requestUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            string jsonResponse = await response.Content.ReadAsStringAsync();
        //            var routeData = JsonConvert.DeserializeObject<RouteResponse>(jsonResponse);

        //            // Передаем координаты маршрута в DrawRouteOnMap
        //            DrawRouteOnMap(routeData.Features[0].Geometry.Coordinates);
        //        }
        //    }
        //}

        //private void DrawRouteOnMap(List<double[]> routePoints)
        //{
        //    // Преобразуем координаты в формат, подходящий для Яндекс.Карт
        //    var points = routePoints.Select(p => $"[{p[0]}, {p[1]}]").ToArray();
        //    string pointsString = string.Join(", ", points);

        //    // Генерация HTML для отображения маршрута на карте
        //    string yandexMapHtml = @"
        //<!DOCTYPE html>
        //<html>
        //<head>
        //    <meta charset=""utf-8"">
        //    <script src=""https://api-maps.yandex.ru/2.1/?apikey=dc002810-6522-49ab-a436-cdd9cff342f5&lang=ru_RU"" type=""text/javascript""></script>
        //    <script type=""text/javascript"">
        //        ymaps.ready(init);
        //        function init() {
        //            var myMap = new ymaps.Map('map', {
        //                center: [55.7558, 37.6176], // Москва
        //                zoom: 10
        //            });

        //            var route = new ymaps.multiRouter.MultiRoute({
        //                referencePoints: [" + pointsString + @"],
        //                params: {
        //                    routingMode: 'auto'
        //                }
        //            }, {
        //                boundsAutoApply: true
        //            });

        //            myMap.geoObjects.add(route);
        //        }
        //    </script>
        //    <style>
        //    html, body {
        //        margin: 0;
        //        padding: 0;
        //        width: 100%;
        //        height: 100%;
        //        overflow: hidden;
        //    }
        //    #map {
        //        width: 100%;
        //        height: 100%;
        //    }
        //</style>
        //</head>
        //<body>
        //    <div id=""map"" style=""width: 100%; height: 100%;""></div>
        //</body>
        //</html>";

        //    // Загрузка HTML в WebView
        //    yandexMapWebView.NavigateToString(yandexMapHtml);
        //}
    }
}
