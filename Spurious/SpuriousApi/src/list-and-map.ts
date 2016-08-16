import * as mapsapi from "google-maps-api";
import { inject } from "aurelia-framework";
import { RouteConfig } from "aurelia-router";
import { HttpClient } from 'aurelia-fetch-client';
import 'fetch';

@inject(mapsapi("AIzaSyDg6zramGtc2YcW-nwLc7FZE0tFUYk-HHw"), HttpClient)
export class ListAndMap {
    title: string;
    subdivisions: Subdivision[] = [];
    viewname: string;
    densityPropertyToUse: string;
    infoWindow: google.maps.InfoWindow;
    selectedSubdivId: number;

    private mapsLoadingPromise: any;
    private maps: any;
    private map: google.maps.Map;

    constructor(mapsApi: any, private http: HttpClient) {
        this.mapsLoadingPromise = mapsApi.then(maps => {
            this.maps = maps;
            this.infoWindow = new this.maps.InfoWindow();
        }).catch((err) => console.error(err));
    }

    get currentSubDivId(): number {
        return this.selectedSubdivId;
    }

    activate(params: any, routeConfig: RouteConfig) {
        this.title = routeConfig.title;
        this.viewname = routeConfig.settings.viewname;
        Promise.all<any, any>([
            this.http.fetch(`api/subdivision/${this.viewname}`).then((response) => response.json()),
            this.mapsLoadingPromise.then()
        ]).then((result) => {
            // Subdivisions stuff
            console.log("subdivs in promise.all:");
            console.log(result[0]);
            let mapandviewmodel = <ListAndMapViewModel>result[0];
            this.densityPropertyToUse = mapandviewmodel.densityPropertyToUse;
            this.subdivisions = mapandviewmodel.subdivisions;

            // Map stuff
            // Start map centered on all of Ontario
            var startCoords = { lat: 49.6830996, long: -88.9273242 };
            this.map = new this.maps.Map(document.getElementById("map-div"), {
                center: new this.maps.LatLng(startCoords.lat, startCoords.long),
                zoom: 5
            });

            // What to do when a shape (feature) is added (fit in view)
            this.map.data.addListener('addfeature', (e: google.maps.Data.AddFeatureEvent) => {
                let bounds: google.maps.LatLngBounds = new this.maps.LatLngBounds();
                this.processPoints(e.feature.getGeometry(), bounds.extend, bounds);
                this.map.fitBounds(bounds);
            });

            // Show location info when location clicked
            this.map.data.addListener('click', (e: google.maps.Data.MouseEvent) => {
                this.infoWindow.close();
                if (e.feature.getProperty("beerVolume") !== undefined) {
                    var city = e.feature.getProperty("city");
                    var name = e.feature.getProperty("name");
                    var beerVolume = e.feature.getProperty("beerVolume");
                    var wineVolume = e.feature.getProperty("wineVolume");
                    var spiritsVolume = e.feature.getProperty("spiritsVolume");
                    var content = '<div>' +
                        '<b>Name:</b> ' + name + '<br />' +
                        '<b>City:</b> ' + city + '<br />' +
                        '<b>Beer Volume:</b> ' + beerVolume + ' L<br />' +
                        '<b>Wine Volume:</b> ' + wineVolume + ' L<br />' +
                        '<b>Spirits Volume:</b> ' + spiritsVolume + ' L<br />' +
                        '</div>';
                    this.infoWindow.setContent(content);
                    this.infoWindow.setPosition((<google.maps.Data.Point>e.feature.getGeometry()).get());
                    this.infoWindow.setOptions({ pixelOffset: new this.maps.Size(0, -30) });
                    this.infoWindow.open(this.map);
                }
            });

            this.map.data.setStyle({
                fillColor: 'green',
                strokeWeight: 1,
                fillOpacity: 0.1
            });

            this.selectSubdiv(this.subdivisions[0]);

        }).catch((err) => console.error(err));
    }

    selectSubdiv(subdiv: Subdivision) {
        console.log("Starting select subdiv");
        this.map.data.forEach((feature) => {
            this.map.data.remove(feature);
        });

        this.http.fetch(`api/subdivision/${subdiv.id}/boundary`);
        this.map.data.loadGeoJson("api/subdivision/" + subdiv.id + "/boundary");
        this.map.setCenter(new this.maps.LatLng(subdiv.centreLatitude, subdiv.centreLongitude));

        subdiv.lcboStores.forEach((store) => {
            this.map.data.addGeoJson(store.geoJSON);
        });

        this.selectedSubdivId = subdiv.id;
    }

    // callback is typically bounds.extend
    private processPoints(geometry: any, callback: any, thisArg: any) {
        var type: string = "";
        if (geometry instanceof this.maps.LatLng) {
            type = "latlng";
            callback.call(thisArg, geometry);
        } else if (geometry instanceof this.maps.Data.Point) {
            type = "point";
            callback.call(thisArg, geometry.get());
        } else {
            geometry.getArray().forEach((g) => {
                this.processPoints(g, callback, thisArg);
            });
        }
    }
}

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
