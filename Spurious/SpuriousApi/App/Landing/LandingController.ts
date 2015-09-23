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

        static $inject: string[] = ["$location", "$http", "uiGmapGoogleMapApi", "$scope"];

        constructor(private $location: ng.ILocationService, private $http: ng.IHttpService, private googleMap: any, private $scope: ng.IScope) {
            this.activate();
        }

        activate() {
            this.$http.get<Array<any>>("/spuriousapi/api/subdivision/top10")
                .then((r) => {
                    this.subdivisions = Subdivision.subdivisionsFromJson(r.data);
                    this.googleMap.then((maps: any) => {
                        this.map = {
                            center: { latitude: 45, longitude: -73 }, zoom: 8, events: {
                                tilesloaded: (map) => {
                                    this.$scope.$apply(() => {
                                        var geojson = JSON.parse(this.subdivisions[0].geoJSON);
                                        map.data.addGeoJson(geojson);
                                    });
                                }
                            }
                        };
                        //                        maps.data.loadGeoJson(this.subdivisions[0].geoJSON);
                    });
                    // success
                }, (r) => {
                    // error
                });


        }
    }

    SpuriousApp.controller("LandingController", LandingController);
}