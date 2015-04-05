using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace MapperTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //decimal startLat = 48.005940M;
            //decimal startLon = -90.8338M;
            //decimal endLat = 48.167637M;
            //decimal endLon = -90.596378M;
            //var client = new WebClient();
            ////client.DownloadDataCompleted +=client_DownloadDataCompleted;
            //var image =  new Bitmap(new MemoryStream(client.DownloadData(new System.Uri("http://terrain.party/api/export?box=" + string.Format("{0},{1},{2},{3}", endLon, endLat, startLon, startLat) + "&heightmap=merged"))));
            //image.Save("test.png");
            //var abv = 1;
            //abv += 1;
            ////var test = new Mapper.OSMInterface("100.89844,4.2372423,101.25000,5.9879921", 1, 24, 6, 6);            
            var z = new double[] { 0 };
            var result = new Dictionary<Vector2, List<Vector2>>[z.Length];
            for (var i = 0; i < z.Length; i += 1)
            {
                result[i] = new Dictionary<Vector2, List<Vector2>>();
            }

            var key = new Vector2(1, 3);
            result[0].Add(key, new List<Vector2>());
            result[0][key].Add(new Vector2(3, 4));

            key = new Vector2(5, 10);
            result[0].Add(key, new List<Vector2>());
            result[0][key].Add(new Vector2(4, 2));

            key = new Vector2(3,4);
            result[0].Add(key, new List<Vector2>());
            result[0][key].Add(new Vector2(4, 2));
            
            key = new Vector2(4, 2);
            result[0].Add(key, new List<Vector2>());
            result[0][key].Add(new Vector2(1, 3));
            var chains = Mapper.OSM.OSMExport.Process(result[0]);
            var a = chains.ToString();
        }
    }
}
