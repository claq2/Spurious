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
                            center: { latitude: 41.7829782176344, longitude: -82.764763246101 }, zoom: 11, events: {
                                tilesloaded: (map) => {
                                    this.$scope.$apply(() => {
                                        var geojson = JSON.parse(this.subdivisions[0].geoJSON);
                                        map.data.addGeoJson(geojson);
                                        map.data.setStyle({
                                            fillColor: 'green',
                                            strokeWeight: 1,
                                            fillOpacity: 0.1
                                        });
                                    });
                                }
                            }
                        };
                    });
                    // success
                }, (r) => {
                    // error
                });


        }
    }

    SpuriousApp.controller("LandingController", LandingController);
}