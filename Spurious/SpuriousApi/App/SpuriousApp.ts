// Install the angularjs.TypeScript.DefinitelyTyped NuGet package 

module SpuriousApp {
    "use strict";

    // Create the module and define its dependencies.
    export var SpuriousApp = angular.module("SpuriousApp", [
        // Angular modules 
        //"ngAnimate", // animations
        "ngRoute", // routing

        // Custom modules 

        // 3rd Party Modules
        "uiGmapgoogle-maps"
    ]);

    SpuriousApp.config(["$routeProvider", "uiGmapGoogleMapApiProvider", ($routeProvider: ng.route.IRouteProvider, googleMap:any) => {
        var listAndMapRoute: ng.route.IRoute = {
            templateUrl: "App/ListAndMap/ListAndMap.html"
        };

        var aboutRoute: ng.route.IRoute = {
            templateUrl: "App/About.html"
        };

        $routeProvider
            .when("/about", aboutRoute)
            .when("/listAndMap/:listName", listAndMapRoute)
            .otherwise({ redirectTo: "/listAndMap/top10" });

        googleMap.configure({
            key: 'AIzaSyDg6zramGtc2YcW-nwLc7FZE0tFUYk-HHw',
            v: '3', //defaults to latest 3.X anyhow
            libraries: 'geometry,visualization'
        });
    }]);
}

