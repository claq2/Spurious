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

        static $inject: string[] = ["$location", "$http", "uiGmapGoogleMapApi"];

        constructor(private $location: ng.ILocationService, private $http: ng.IHttpService, private googleMap: any) {
            this.activate();
        }

        activate() {
            this.$http.get<Array<any>>("/spuriousapi/api/subdivision/top10")
                .then((r) => {
                    this.subdivisions = Subdivision.subdivisionsFromJson(r.data);
                    // success
                }, (r) => {
                    // error
                });

            this.googleMap.then((maps: any) => {
                this.map = { center: { latitude: 45, longitude: -73 }, zoom: 8 };
            });
        }
    }

    SpuriousApp.controller("LandingController", LandingController);
}