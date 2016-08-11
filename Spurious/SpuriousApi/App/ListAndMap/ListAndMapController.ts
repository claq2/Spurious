// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module SpuriousApp {
    "use strict";

    interface IListAndMapController {
        title: string;
        activate: () => void;
    }

    class ListAndMapController implements IListAndMapController {
        title: string;
        subdivisions: Subdivision[] = [];
        map2: any;
        map: any;
        realMap: google.maps.Map;
        mapsApi: any;
        selectedSubdivId: number;
        infoWindow: google.maps.InfoWindow;
        listName: string;
        densityPropertyToUse: string;

        static $inject: string[] = ["$location", "$http", "$routeParams", "uiGmapGoogleMapApi", "uiGmapIsReady"];

        constructor(private $location: ng.ILocationService,
            private $http: ng.IHttpService,
            private $routeParams: ng.route.IRouteParamsService,
            private googleMap: any,
            private uiGmapIsReady: any) {
            console.log("======================================");
            this.listName = this.$routeParams["listName"];
            console.log("About to activate in cctor");
            this.activate();
            console.log("Right after activate");
            // This map gets its values first so it's instance 0?
            this.map2 = { center: { latitude: 46, longitude: -73 }, zoom: 10 };

            this.uiGmapIsReady.promise(2)
                .then((instances) => {
                    console.log("Starting then after uiGmapIsReady");
                    let firstMap = instances[1].map;
                    this.realMap = firstMap;
                    this.infoWindow = new this.mapsApi.InfoWindow();

                    // What to do when a shape (feature) is added (fit in view)
                    this.realMap.data.addListener('addfeature', (e: google.maps.Data.AddFeatureEvent) => {
                        let bounds: google.maps.LatLngBounds = new this.mapsApi.LatLngBounds();
                        this.processPoints(e.feature.getGeometry(), bounds.extend, bounds);
                        this.realMap.fitBounds(bounds);
                    });

                    // Show location info when location clicked
                    this.realMap.data.addListener('click', (e: google.maps.Data.MouseEvent) => {
                        this.infoWindow.close();
                        if (e.feature.getProperty("beerVolume") !== undefined) {
                            var city = e.feature.getProperty("city");
                            var name = e.feature.getProperty("name");
                            var beerVolume = e.feature.getProperty("beerVolume");
                            var wineVolume = e.feature.getProperty("wineVolume");
                            var spiritsVolume = e.feature.getProperty("spiritsVolume");
                            var content = '<div>' +
                                '<b>Name:</b> ' + name + '<br />' +
                                '<b>City:</b> ' + city + '<br />' +
                                '<b>Beer Volume:</b> ' + beerVolume + ' mL<br />' +
                                '<b>Wine Volume:</b> ' + wineVolume + ' mL<br />' +
                                '<b>Spirits Volume:</b> ' + spiritsVolume + ' mL<br />' +
                                '</div>';
                            this.infoWindow.setContent(content);
                            this.infoWindow.setPosition((<google.maps.Data.Point>e.feature.getGeometry()).get());
                            this.infoWindow.setOptions({ pixelOffset: new this.mapsApi.Size(0, -30) });
                            this.infoWindow.open(this.realMap);
                        }
                    });

                    this.realMap.data.setStyle({
                        fillColor: 'green',
                        strokeWeight: 1,
                        fillOpacity: 0.1
                    });
                    console.log("About to select subdiv in cctor");
                    this.selectSubdiv(this.subdivisions[0]);
                });
        }

        activate() {
            this.$http.get<ListAndMapViewModel>("api/subdivision/" + this.listName)
                .then((r) => {
                    console.log("Got data");
                    this.subdivisions = r.data.subdivisions;
                    this.title = r.data.title;
                    this.densityPropertyToUse = r.data.densityPropertyToUse;
                    this.selectedSubdivId = this.subdivisions[0].id;
                    this.googleMap.then((maps: any) => {
                        console.log("googlemap.then");
                        this.mapsApi = maps;
                        var firstSubdiv = this.subdivisions[0];
                        this.map = {
                            center: {
                                latitude: firstSubdiv.centreLatitude,
                                longitude: firstSubdiv.centreLongitude
                            },
                            zoom: 11,
                            control: {}
                        };
                    });
                    // success
                }, (r) => {
                    // error
                });
        }

        selectSubdiv(subdiv: Subdivision) {
            console.log("Starting select subdiv");
            this.realMap.data.forEach((feature) => {
                this.realMap.data.remove(feature);
            });



            this.realMap.data.loadGeoJson("api/subdivision/" + subdiv.id + "/boundary");
            this.map.center = {
                latitude: subdiv.centreLatitude,
                longitude: subdiv.centreLongitude
            };

            subdiv.lcboStores.forEach((store) => {
                this.realMap.data.addGeoJson(store.geoJSON);
            });

            this.selectedSubdivId = subdiv.id;
        }

        // callback is typically bounds.extend
        processPoints(geometry: any, callback: any, thisArg: any) {
            var type: string = "";
            if (geometry instanceof this.mapsApi.LatLng) {
                type = "latlng";
                callback.call(thisArg, geometry);
            } else if (geometry instanceof this.mapsApi.Data.Point) {
                type = "point";
                callback.call(thisArg, geometry.get());
            } else {
                geometry.getArray().forEach((g) => {
                    this.processPoints(g, callback, thisArg);
                });
            }
        }
    }

    SpuriousApp.controller("ListAndMapController", ListAndMapController);
}