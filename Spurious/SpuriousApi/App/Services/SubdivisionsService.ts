﻿// Install the angularjs.TypeScript.DefinitelyTyped NuGet package
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

    export class ListAndMapViewModel {
        title: string;
        subdivisions: Array<Subdivision>;
        densityName: string;
        densityPropertyToUse: string;
    }

    export class Subdivision {
        id: number;
        name: string;
        population: number;
        boundaryGeoJson: string;
        centreLatitude: number;
        centreLongitude: number;
        overallAlcoholDensity: number;
        beerDensity: number;
        wineDensity: number;
        spiritsDensity: number;
        lcboStores: LcboStore[];
    }

    export class LcboStore {
        id: number;
        geoJSON: GeoJSON.Feature<GeoJSON.Point>;
        name: string;
        volumes: Volumes;
    }

    export class Volumes {
        beer: number;
        wine: number;
        spirits: number;
        total: number;
    }
}