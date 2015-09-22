// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module SpuriousApp {
    "use strict";

    interface ILandingController {
        title: string;
        activate: () => void;
    }

    class LandingController implements ILandingController {
        title: string = "LandingController";

        static $inject: string[] = ["$location"];

        constructor(private $location: ng.ILocationService) {
            this.activate();
        }

        activate() {

        }
    }

    angular.module("SpuriousApp").controller("LandingController", LandingController);
}