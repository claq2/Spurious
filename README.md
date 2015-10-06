http://spurious.azurewebsites.net

What does the amount of alcohol in a census subdivision mean? Nothing. But it's fun to map it out like it does. 

Population and census subdivision data comes from Statistics Canada's 2011 census data, which is the most recent one. 

Alcohol inventory comes from LCBO API, which is not actually published by the LCBO, but scraped from its website daily. 

PostgreSQL and PostGIS provide data storage and the geographic matching of LCBO locations inside subdivisions. The backend is .NET WebAPI. The frontend is AngularJS written in TypeScript. HTML5 Boilerplate and Bootstrap provide the good looks. Google map is by Angular Google Maps. It all lives in Azure. 
