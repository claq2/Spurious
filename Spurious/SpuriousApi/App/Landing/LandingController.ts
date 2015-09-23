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

        static $inject: string[] = ["$location", "$http"];

        constructor(private $location: ng.ILocationService, private $http: ng.IHttpService) {
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
        }
    }

    SpuriousApp.controller("LandingController", LandingController);
}