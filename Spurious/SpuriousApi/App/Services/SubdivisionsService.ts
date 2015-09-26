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
        boundaryGeoJson: string;
        centreLatitude: number;
        centreLongitude: number;
        overallAlcoholDensity: number;

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
                boundaryGeoJson: json.geoJSON,
                overallAlcoholDensity: json.overallAlcoholDensity,
                centreLatitude: json.centreLatitude,
                centreLongitude: json.centreLongitude
            };

            return result;
        }
    }
}