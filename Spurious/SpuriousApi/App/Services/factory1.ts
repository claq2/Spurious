// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module App {
    "use strict";

    interface Ifactory1 {
        getData: () => string;
    }

    factory1.$inject = ["$http"];

    function factory1($http: ng.IHttpService): Ifactory1 {
        var service: Ifactory1 = {
            getData: getData
        };

        return service;

        function getData() {
            return "";
        }
    }


    angular.module("app").factory("factory1", factory1);
}