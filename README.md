# PWA-APIv2

A .NET Aspire version of Dakota's PWA-API project (https://github.com/thedakotes/PWA-API). 

Prereqs:

- Visual Studio 2022
- Docker (https://www.docker.com)

This project needs to live (for now) at the same level of the PWA-Web folder, until we figure out if this even works.

Docker needs to be running on your desktop prior to running the project.
Set the startup project to be PWAApi.AppHost and click the 'https' button. 

NOTE: The PWA-Web project has been set to a static port of 4200 for development. In order to debug the Web project from VS Code, the PWA-APIv2 project needs to be running first, and then select the 'Connect to Aspire' Launch option in VS Code, which should be set to connect to the instance already running at 4200. Debugging will only be triggered if you're using the browser that pops up from VS Code (not the browser that's loaded when launched from clicking on the web URL in the Aspire dashboard)
