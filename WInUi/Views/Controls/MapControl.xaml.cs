using System.Linq;
using Windows.Devices.Geolocation;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NetTopologySuite.Geometries;
using Mapsui.Tiling.Layers;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Nts;
using URE.Core.Models.Equipment;
using BruTile.Predefined;
using URE.Services;
using Mapsui.Tiling.Fetcher;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.UI.WinUI;
using MapsuiMap = Mapsui.UI.WinUI.MapControl;
using Mapsui.UI.WinUI.Extensions;

namespace URE.Views.Controls
{
    public sealed partial class MapControl : UserControl
    {
        private readonly Dictionary<string, string> FeatureIcons = new Dictionary<string, string>
        {
            { "RouteStart", Package.Current.InstalledLocation.Path + "/Assets/routeStart.png" },
            { "RouteEnd", Package.Current.InstalledLocation.Path + "/Assets/routeEnd.png" },
            { "RoutePoint", Package.Current.InstalledLocation.Path + "/Assets/routePoint.png" }
        };

        private readonly Dictionary<string, int> BitmapCache = new Dictionary<string, int>();

        private readonly GenericCollectionLayer<List<GeometryFeature>> _pointLayer;
        private readonly List<MPoint> _routePoints = new List<MPoint>();
        private bool _followingEnabled = false;

        public MapControl()
        {
            this.InitializeComponent();
            LoadFeatureIcons();

            MapTileCacheService cacheService = App.GetService<MapTileCacheService>();

            MyMap.Map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap, persistentCache: cacheService, minZoomLevel: 0, maxZoomLevel: 18),
                dataFetchStrategy: new DataFetchStrategy()));

            _pointLayer = new GenericCollectionLayer<List<GeometryFeature>>
            {
                Style = null,
                IsMapInfoLayer = true
            };

            MyMap.Map.Layers.Add(_pointLayer);

            InitializeWidgets();
            
            Loaded += OnLoaded;
        }

        public MapsuiMap GetMap()
        {
            return MyMap;
        }

        public void AddPoint(int id, double lon, double lat, SettingsValueState valueState, bool refresh = false)
        {
            MPoint point = SphericalMercator.FromLonLat(lon, lat).ToMPoint();

            if (!_routePoints.Any())
            { 
                GeometryFeature startFeature = new GeometryFeature
                {
                    Geometry = new Point(point.X, point.Y)
                };

                startFeature["SourceId"] = id;
                startFeature["IsStartOrEnd"] = true;

                startFeature.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.3,
                    Outline = null,
                    BitmapId = BitmapCache["RouteStart"],
                    Opacity = 0.8f
                });

                _pointLayer.Features.Add(startFeature);
            }
            else
            {
                var lastFeature = _pointLayer.Features.LastOrDefault(p => p["RoutePoint"] != null);
                if (lastFeature != null)
                {
                    _pointLayer.Features.Remove(lastFeature);
                }
            }

            GeometryFeature pointFeature = new GeometryFeature()
            {
                Geometry = new Point(point.X, point.Y)
            };

            pointFeature["SourceId"] = id;
            pointFeature["IsStartOrEnd"] = false;

            switch (valueState)
            {
                case SettingsValueState.Normal:
                    pointFeature.Styles.Add(new SymbolStyle()
                    {
                        SymbolScale = 0.3,
                        Fill = new Mapsui.Styles.Brush(Color.FromString("Green")),
                        Outline = null,
                        Opacity = 0.5f,
                    }) ;
                    break;
                case SettingsValueState.High:
                    pointFeature.Styles.Add(new SymbolStyle()
                    {
                        SymbolScale = 0.3,
                        Fill = new Mapsui.Styles.Brush(Color.FromString("Orange")),
                        Outline = null,
                        Opacity = 0.5f,
                    });
                    break;
                case SettingsValueState.Critical:
                    pointFeature.Styles.Add(new SymbolStyle()
                    {
                        SymbolScale = 0.3,
                        Fill = new Mapsui.Styles.Brush(Color.FromString("Red")),
                        Outline = null,
                        Opacity = 0.5f
                    });
                    break;
                default:
                    break;

            }

            _pointLayer.Features.Add(pointFeature);

            GeometryFeature endFeature = new GeometryFeature
            {
                Geometry = new Point(point.X, point.Y)
            };

            endFeature["SourceId"] = id;
            endFeature["IsStartOrEnd"] = !refresh;
            endFeature["RoutePoint"] = true;

            double angle = CalculateFeatureAngle(point);

            endFeature.Styles.Add(new SymbolStyle()
            {
                SymbolScale = refresh ? 0.4 : 0.3,
                Outline = null,
                Opacity = 0.8f,
                BitmapId = refresh ? BitmapCache["RoutePoint"] : BitmapCache["RouteEnd"],
                SymbolRotation = refresh ? angle : 0
            });

            _pointLayer.Features.Add(endFeature);

            if (refresh)
            {
                _pointLayer.DataHasChanged();
                if (_followingEnabled)
                {
                    MyMap.Map.Navigator.CenterOnAndZoomTo(point, 5);
                }
            }

            _routePoints.Add(point);
        }

        public void AddPoints(List<(int id, double Lon, double Lat, SettingsValueState ValueState)> points)
        {
            _routePoints.Clear();

            foreach (var point in points)
            {
                AddPoint(point.id, point.Lon, point.Lat, point.ValueState);
            }

            _pointLayer.DataHasChanged();
        }

        public void ShowPointInfo(int sourceId, double lon, double lat, string value, bool scale = false)
        {
            MPoint point = SphericalMercator.FromLonLat(lon, lat).ToMPoint();

            if (!IsPointInfoShown(point))
            {
                GeometryFeature calloutFeature = new GeometryFeature
                {
                    Geometry = new Point(point.X, point.Y),
                };

                calloutFeature["SourceId"] = sourceId;
                calloutFeature["IsStartOrEnd"] = false;

                var calloutTitleFont = new Font
                {
                    Size = 14,
                    FontFamily = "TimesNewRoman",
                    Bold = true
                };

                var calloutSubtitleFont = new Font
                {
                    Size = 14,
                    FontFamily = "TimesNewRoman",
                    Bold = false
                };

                calloutFeature.Styles.Add(new CalloutStyle
                {
                    Type = CalloutType.Detail,
                    Title = "Доза радіації",
                    Subtitle = value,
                    MaxWidth = 1000,
                    TitleFontColor = Color.Black,
                    TitleFont = calloutTitleFont,
                    SubtitleFontColor = Color.Black,
                    SubtitleFont = calloutSubtitleFont,
                    TitleTextAlignment = Alignment.Left
                });


                _pointLayer.Features.Add(calloutFeature);
                _pointLayer.DataHasChanged();

                if (scale)
                {
                    MyMap.Map.Navigator.CenterOnAndZoomTo(point, 0.5);
                }
            }
        }

        public void ShowStartOrEndPointInfo(int sourceId, double lon, double lat, string @operator, DateTime start, DateTime end, bool scale = false)
        {
            MPoint point = SphericalMercator.FromLonLat(lon, lat).ToMPoint();

            if (!IsPointInfoShown(point))
            {
                GeometryFeature calloutFeature = new GeometryFeature
                {
                    Geometry = new Point(point.X, point.Y),
                };

                calloutFeature["SourceId"] = sourceId;
                calloutFeature["IsStartOrEnd"] = true;

                var calloutTitleFont = new Font
                {
                    Size = 14,
                    FontFamily = "TimesNewRoman",
                    Bold = true
                };

                var calloutSubtitleFont = new Font
                {
                    Size = 14,
                    FontFamily = "TimesNewRoman",
                    Bold = false
                };

                calloutFeature.Styles.Add(new CalloutStyle
                {
                    Type = CalloutType.Detail,
                    Title = @operator,
                    Subtitle = $"{start} - {end}",
                    MaxWidth = 1000,
                    TitleFontColor = Color.Black,
                    TitleFont = calloutTitleFont,
                    SubtitleFontColor = Color.Black,
                    SubtitleFont = calloutSubtitleFont,
                    TitleTextAlignment = Alignment.Left
                });


                _pointLayer.Features.Add(calloutFeature);
                _pointLayer.DataHasChanged();

                if (scale)
                {
                    MyMap.Map.Navigator.CenterOnAndZoomTo(point, 0.5);
                }
            }
        }

        public void HidePointInfo(GeometryFeature callout)
        {
            _pointLayer.Features.Remove(callout);
            _pointLayer.DataHasChanged();
        }

        public void ClearMap()
        {
            _routePoints.Clear();
            _pointLayer.Features.Clear();
            _pointLayer.DataHasChanged();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Geoposition? geoposition = await GetGeopositionAsync();
            double latitude = 50.447100;
            double longitude = 30.521876;

            if (geoposition != null)
            {
                latitude = geoposition.Coordinate.Point.Position.Latitude;
                longitude = geoposition.Coordinate.Point.Position.Longitude;
            }

            MPoint cuurentLocation = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
            MyMap.Map.Navigator.CenterOnAndZoomTo(cuurentLocation, 5);

            MyMap.DoubleTapped += (s,e) => {
                MyMap.Map.Navigator.ZoomIn(e.GetPosition(MyMap).ToMapsui(), 300);
            };

            var minusDefaultIcon = LoadWidgetIcon("/Assets/minus-default.svg");
            var minusDisabledIcon = LoadWidgetIcon("/Assets/minus-disabled.svg");
            var plusDefaultIcon = LoadWidgetIcon("/Assets/plus-default.svg");
            var plusDisabledIcon = LoadWidgetIcon("/Assets/plus-disabled.svg");

            var resolutions = MyMap.Map.Navigator.Resolutions;

            var max = resolutions.First();
            var min = resolutions.Last();

            MyMap.Map.Navigator.ViewportChanged += (s, e) =>
            {
                if (MyMap.Map.Navigator.Viewport.Resolution < max)
                {
                    minusBtnWidget.SvgImage = minusDefaultIcon;
                }
                else
                {
                    minusBtnWidget.SvgImage = minusDisabledIcon;
                }

                if (MyMap.Map.Navigator.Viewport.Resolution > min)
                {
                    plusBtnWidget.SvgImage = plusDefaultIcon;
                }
                else
                {
                    plusBtnWidget.SvgImage = plusDisabledIcon;
                }
            };
        }

        private void WidgetFollow_Touched(object sender, WidgetTouchedEventArgs e)
        {
            _followingEnabled = !_followingEnabled;

            followBtnWidget.SvgImage = LoadWidgetIcon(_followingEnabled ? "/Assets/routePointEnabled.svg" : "/Assets/routePointDisabled.svg");

            _pointLayer.DataHasChanged();
        }

        private async Task<Geoposition?> GetGeopositionAsync()
        {
            Geoposition? geoposition = null;
            var accessStatus = await Geolocator.RequestAccessAsync();

            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                var geolocator = new Geolocator();
                geoposition = await geolocator.GetGeopositionAsync();
            }

            return geoposition;
        }

        private string LoadWidgetIcon(string path)
        {
            string fullPath = Package.Current.InstalledLocation.Path + path;

            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            
            return reader.ReadToEnd();
        }

        private ButtonWidget followBtnWidget;
        private ButtonWidget plusBtnWidget;
        private ButtonWidget minusBtnWidget;

        private void InitializeWidgets()
        {
            followBtnWidget = new ButtonWidget()
            {
                CornerRadius = 20,
                Text = " ",
                BackColor = Color.White,
                MarginX = 10,
                MarginY = 150,
                PaddingX = 20,
                PaddingY = 15,
                SvgImage = LoadWidgetIcon(_followingEnabled ? "/Assets/routePointEnabled.svg" : "/Assets/routePointDisabled.svg"),
                Opacity = 1,
            };

            followBtnWidget.WidgetTouched += WidgetFollow_Touched;

            MyMap.Map.Widgets.Add(followBtnWidget);

            plusBtnWidget = new ButtonWidget()
            {
                Text = " ",
                BackColor = Color.White,
                MarginX = 10,
                MarginY = 65,
                PaddingX = 20,
                PaddingY = 15,
                SvgImage = LoadWidgetIcon("/Assets/plus-default.svg"),
                Opacity = 1,
            };

            plusBtnWidget.WidgetTouched += (s, e) => {
                MyMap.Map.Navigator.ZoomIn(300);
            };

            MyMap.Map.Widgets.Add(plusBtnWidget);

            minusBtnWidget = new ButtonWidget()
            {
                Text = " ",
                BackColor = Color.White,
                MarginX = 10,
                MarginY = 20,
                PaddingX = 20,
                PaddingY = 15,
                SvgImage = LoadWidgetIcon("/Assets/minus-default.svg"),
                Opacity = 1
            };

            minusBtnWidget.WidgetTouched += (s, e) => {
                MyMap.Map.Navigator.ZoomOut(300);
            };

            MyMap.Map.Widgets.Add(minusBtnWidget);
        }

        private void LoadFeatureIcons()
        {
            foreach (var icon in FeatureIcons)
            {
                using var stream = new FileStream(icon.Value, FileMode.Open, FileAccess.Read);

                int id = BitmapRegistry.Instance.Register(stream.ToBytes());
                BitmapCache.Add(icon.Key, id);
            }  
        }

        private double CalculateFeatureAngle(MPoint point)
        {
            return Math.Atan(point.X / point.Y) * (180 / Math.PI);
        }

        private bool IsPointInfoShown(MPoint point)
        {
            var pointFeatures = _pointLayer.GetFeatures(new MRect(point.X - 1, point.Y - 1, point.X + 1, point.Y + 1), 0.1);
            return pointFeatures.FirstOrDefault(p => p.Styles.FirstOrDefault() is CalloutStyle) != null;
        }
    }
}
