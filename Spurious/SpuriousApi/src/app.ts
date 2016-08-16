import { Router, RouterConfiguration } from 'aurelia-router';

export class App {
  router: Router;

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Spurious Alcohol Statistics';
    config.map([
      { route: ["", "top10"], name: "top10", moduleId: "list-and-map", nav: true, title: "Top 10 Overall", activationStrategy: "replace", settings: { viewname: "top10" } },
      { route: "top10Beer", name: "top10Beer", moduleId: "list-and-map", nav: true, title: "Top 10 Beer", activationStrategy: "replace", settings: { viewname: "top10Beer" } },
      { route: "top10Wine", name: "top10Wine", moduleId: "list-and-map", nav: true, title: "Top 10 Wine", activationStrategy: "replace", settings: { viewname: "top10Wine" } },
      { route: "top10Spirits", name: "top10Spirits", moduleId: "list-and-map", nav: true, title: "Top 10 Spirits", activationStrategy: "replace", settings: { viewname: "top10Spirits" } },
      { route: "bottom10", name: "bottom10", moduleId: "list-and-map", nav: true, title: "Bottom 10 Overall", activationStrategy: "replace", settings: { viewname: "bottom10" } },
      { route: "all", name: "all", moduleId: "list-and-map", nav: true, title: "All", activationStrategy: "replace", settings: { viewname: "all" } },
      { route: "about", name: "about", moduleId: "about", nav: true, title: "About" }
    ]);

    this.router = router;
  }
}
