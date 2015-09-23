// Install the angularjs.TypeScript.DefinitelyTyped NuGet package 

module SpuriousApp {
    "use strict";

    // Create the module and define its dependencies.
    export var SpuriousApp = angular.module("SpuriousApp", [
        // Angular modules 
        "ngAnimate", // animations
        "ngRoute" // routing

        // Custom modules 

        // 3rd Party Modules
    ]);

    SpuriousApp.config(["$routeProvider", ($routeProvider: ng.route.IRouteProvider) => {
        var landingRoute: ng.route.IRoute = {
            templateUrl: "App/Landing/Landing.html"
        };

        $routeProvider
            .when("/landing", landingRoute)
            .otherwise({ redirectTo: "/landing" });
    }]);
}

