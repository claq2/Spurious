// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module App {
    "use strict";

    interface ISubdivisionsService {
        getData: () => string;
    }
    
    class SubdivisionsService implements ISubdivisionsService {
        static $inject: string[] = ["$http"];

        constructor(private $http: ng.IHttpService) {
        }

        getData() {
            return "";
        }
    }

    angular.module("app").service("SubdivisionsService", SubdivisionsService);
}