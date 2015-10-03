// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module SpuriousApp {
    "use strict";

    interface ILandingController {
        title: string;
        activate: () => void;
    }

    class LandingController implements ILandingController {
        title: string = "LandingController";
        subdivisions: Subdivision[] = [];
        map: any;
        realMap: any;
        mapsApi: any;

        static $inject: string[] = ["$location", "$http", "uiGmapGoogleMapApi", "$scope"];

        constructor(private $location: ng.ILocationService, private $http: ng.IHttpService, private googleMap: any, private $scope: ng.IScope) {
            this.activate();
            //var x = this.map.control;
        }

        activate() {
            this.$http.get<Array<any>>("/spuriousapi/api/subdivision/top10")
                .then((r) => {
                    this.subdivisions = Subdivision.subdivisionsFromJson(r.data);
                    this.googleMap.then((maps: any) => {
                        this.mapsApi = maps;
                        var firstSubdiv = this.subdivisions[0];
                        this.map = {
                            center: {
                                latitude: firstSubdiv.centreLatitude,
                                longitude: firstSubdiv.centreLongitude
                            },
                            zoom: 11,
                            events: {
                                tilesloaded: (map) => {
                                    this.$scope.$apply(() => {
                                        //var geojson = JSON.parse(this.subdivisions[0].boundaryGeoJson);
                                        //map.data.addGeoJson(geojson);
                                        map.data.setStyle({
                                            fillColor: 'green',
                                            strokeWeight: 1,
                                            fillOpacity: 0.1
                                        });
                                        this.realMap = map;
                                    });
                                }
                            },
                            control: {}
                        };
                    });
                    // success
                }, (r) => {
                    // error
                });


        }

        selectSubdiv(subdiv: Subdivision) {
            this.realMap.data.forEach((feature) => {
                this.realMap.data.remove(feature);
            });

            this.realMap.data.loadGeoJson("http://localhost/spuriousapi/api/subdivision/" + subdiv.id + "/boundary");
            var bounds = new this.mapsApi.LatLngBounds();
            this.realMap.data.addListener('addfeature', (e) => {
                this.processPoints(e.feature.getGeometry(), bounds.extend, bounds);
                this.realMap.fitBounds(bounds);
            });
            //this.realMap.data.forEach((feature) => {
            //    bounds.expand(                
            //});

            this.map.center = {
                latitude: subdiv.centreLatitude,
                longitude: subdiv.centreLongitude
            };
        }

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

    SpuriousApp.controller("LandingController", LandingController);
}