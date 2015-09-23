// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
module SpuriousApp {
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

    SpuriousApp.service("SubdivisionsService", SubdivisionsService);

    export class Subdivision {
        id: number;
        name: string;
        population: number;
        geoJSON: string;

        static subdivisionsFromJson(json: Array<any>): Array<Subdivision> {
            var result: Array<Subdivision> = [];
            for (var i = 0; i < json.length; i++) {
                result.push(Subdivision.subdivisionFromJson(json[i]));
            }

            return result;
        }

        static subdivisionFromJson(json: any): Subdivision {
            var result: Subdivision;
            result = {
                id: json.id,
                name: json.name,
                population: json.population,
                geoJSON: json.geoJSON
            };

            return result;
        }
    }
}