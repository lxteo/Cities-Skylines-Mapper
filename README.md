Import roads from real world road maps. Export your cities and view them with road map viewers.

##Instructions to Import##
Go to .terrain.party[terrain.party] and download heightmap (18km is 1:1 scale). Open the readme and copy the coordinates. 

Start a new game with the map, there should be a new road button on the right of the toolbar. Paste your coordinates into the coordinates box. 

Click on Load From terrain.party and wait for the file to download and load. 

Clean up the terrain, smoothen out the spikey bits. Then click on the "Load From overpass-api.de" button. 

It'll lag awhile while it loads the file. After it's done loading click on "Make Roads" and it should start drawing the roads.

Or you can load the map data from disk. The path is the location of your OSM file (by default you should just need to move the map file into your documents folder). Click "Load OSM From File". 

##Parameters##
The scale is based on real world scale, not the bounding box, adjust this if your terrain.party heightmap is on a different scale. (Not working correctly!) The tolerance is the amount the mod will remove extra points to simplify the data. The Curve Tolerence is the leeway the mod uses in fitting curves to the map points. The tiles to boundary is the maximum number of game tiles from the middle the mod will draw on. There is a 32767 limit on the number of road segments. 

##Export to .osm##
Click the export to osm button to export your city into an osm map. You can view this on any of the offline osm viewers like Java OpenStreetMapEditor. The file will be in your main city skylines folder and named after your city. More detailed instructions here. 

##Known issues##
Scale isn't working correctly. If you have any other issues please tell me which coordinates you used! 

My BTC Address: 1MTvtciWFo2gCWXsX3F6C4ppk8mRY6AkuR

© OpenStreetMap contributors
