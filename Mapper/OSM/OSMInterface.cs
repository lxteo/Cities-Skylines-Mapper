using Mapper.Curves;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using UnityEngine;

namespace Mapper.OSM
{
    public class OSMInterface
    {
        public RoadMapping Mapping;
        private FitCurves fc;

        public Dictionary<string, Vector2> nodes = new Dictionary<string, Vector2>();
        public LinkedList<Way> ways = new LinkedList<Way>();

        double tolerance = 10;
        double curveError = 5;

        public OSMInterface(osmBounds bounds, double scale, double tolerance, double curveTolerance, double tiles)
        {
            this.tolerance = tolerance;
            curveError = curveTolerance;

            Mapping = new RoadMapping(tiles);
            fc = new FitCurves();

            var client = new WebClient();

            string nodes = "http://overpass-api.de/api/interpreter?data=node(" +
                           string.Format("{0},{1},{2},{3}", bounds.maxlat.ToString(), bounds.minlon.ToString(),
                               bounds.minlat.ToString(), bounds.maxlon.ToString()) + ");out;";
            string ways = "http://overpass-api.de/api/interpreter?data=way(" +
                          string.Format("{0},{1},{2},{3}", bounds.maxlat.ToString(), bounds.minlon.ToString(),
                              bounds.minlat.ToString(), bounds.maxlon.ToString()) + ");out;";


            var nodesResponse = client.DownloadData(nodes);
            var waysResponse = client.DownloadData(ways);
            var nodesMemoryStream = new MemoryStream(nodesResponse);
            var wayssMemoryStream = new MemoryStream(waysResponse);
            var nodesReader = new StreamReader(nodesMemoryStream);
            var waysReader = new StreamReader(wayssMemoryStream);

            var serializer = new XmlSerializer(typeof(OsmDataResponse));
            var nodesOsm = (OsmDataResponse) serializer.Deserialize(nodesReader);
            var waysOsm = (OsmDataResponse) serializer.Deserialize(waysReader);
            nodesOsm.way = waysOsm.way;

            nodesMemoryStream.Dispose();
            wayssMemoryStream.Dispose();
            nodesReader.Dispose();
            waysReader.Dispose();

            nodesOsm.bounds = bounds;
            Init(nodesOsm, scale);
        }

        public OSMInterface(osmBounds bounds, string path, double scale, double tolerance, double curveTolerance,
            double tiles)
        {
            this.tolerance = tolerance;
            this.curveError = curveTolerance;

            Mapping = new RoadMapping(tiles);
            fc = new FitCurves();

            var serializer = new XmlSerializer(typeof(OsmDataResponse));
            var reader = new StreamReader(path);

            var osm = serializer.Deserialize(reader) as OsmDataResponse;
            reader.Dispose();

            if (osm != null)
            {
                osm.bounds = bounds;
                Init(osm, scale);
            }
        }

        private void Init(OsmDataResponse osmDataResponse, double scale)
        {
            Mapping.InitBoundingBox(osmDataResponse.bounds, scale);

            foreach (var node in osmDataResponse.node)
            {
                if (!nodes.ContainsKey(node.id) && node.lat != 0 && node.lon != 0)
                {
                    Vector2 pos = Vector2.zero;
                    if (Mapping.GetPos(node.lon, node.lat, ref pos))
                    {
                        nodes.Add(node.id, pos);
                    }
                }
            }

            foreach (var way in osmDataResponse.way.OrderBy(c => c.changeset))
            {
                RoadTypes rt = RoadTypes.None;
                List<string> points = null;
                int layer = 0;

                if (Mapping.Mapped(way, ref points, ref rt, ref layer))
                {
                    var currentList = new List<ulong>();
                    for (var i = 0; i < points.Count; i += 1)
                    {
                        var pp = points[i];
                        if (nodes.ContainsKey(pp))
                        {
                            currentList.Add(Convert.ToUInt64(pp));
                        }
                        else
                        {
                            if (currentList.Count() > 1 || currentList.Contains(Convert.ToUInt64(pp)))
                            {
                                ways.AddLast(new Way(currentList, rt, layer));
                                currentList = new List<ulong>();
                            }
                        }
                    }
                    if (currentList.Count() > 1)
                    {
                        ways.AddLast(new Way(currentList, rt, layer));
                    }
                }
            }

            var intersection = new Dictionary<ulong, List<Way>>();
            foreach (var ww in ways)
            {
                foreach (var pp in ww.nodes)
                {
                    if (!intersection.ContainsKey(pp))
                    {
                        intersection.Add(pp, new List<Way>());
                    }
                    intersection[pp].Add(ww);
                }
            }

            var allSplits = new Dictionary<Way, List<int>>();
            foreach (var inter in intersection)
            {
                if (inter.Value.Count > 1)
                {
                    foreach (var way in inter.Value)
                    {
                        if (!allSplits.ContainsKey(way))
                        {
                            allSplits.Add(way, new List<int>());
                        }
                        allSplits[way].Add(way.nodes.IndexOf(inter.Key));
                    }
                }
            }

            foreach (var waySplits in allSplits)
            {
                SplitWay(waySplits.Key, waySplits.Value);
            }

            BreakWaysWhichAreTooLong();
            SimplifyWays();
        }

        private void BreakWaysWhichAreTooLong()
        {
            var allSplits = new Dictionary<Way, List<int>>();
            foreach (var way in ways)
            {
                float length = 0f;
                for (var i = 0; i < way.nodes.Count() - 1; i += 1)
                {
                    length += (nodes[way.nodes[i + 1].ToString()] - nodes[way.nodes[i].ToString()]).magnitude;
                }
                int segments = Mathf.FloorToInt(length / 100f) + 1;
                float averageLength = length / (float) segments;
                if (segments <= 1)
                {
                    continue;
                }
                length = 0;
                var splits = new List<int>();
                for (var i = 0; i < way.nodes.Count() - 1; i += 1)
                {
                    length += (nodes[way.nodes[i + 1].ToString()] - nodes[way.nodes[i].ToString()]).magnitude;
                    if (length > averageLength && i != way.nodes.Count - 2)
                    {
                        splits.Add(i + 1);
                        length = 0;
                    }
                }
                if (splits.Count() > 0)
                {
                    allSplits.Add(way, splits);
                }
            }

            foreach (var waySplits in allSplits)
            {
                SplitWay(waySplits.Key, waySplits.Value);
            }
        }


        private void SplitWay(Way way, List<int> splits)
        {
            splits = splits.OrderBy(c => c).ToList();
            var index = ways.Find(way);
            for (var i = 0; i < splits.Count(); i += 1)
            {
                var nextIndex = way.nodes.Count() - 1;
                if (i != splits.Count - 1)
                {
                    nextIndex = splits[i + 1];
                }
                var newWay = new Way(way.nodes.GetRange(splits[i], 1 + nextIndex - splits[i]), way.roadTypes, way.layer);
                ways.AddAfter(index, newWay);
            }
            way.nodes.RemoveRange(splits[0] + 1, way.nodes.Count() - splits[0] - 1);
        }


        private void SimplifyWays()
        {
            foreach (var way in ways)
            {
                var points = new List<Vector2>();
                foreach (var pp in way.nodes)
                {
                    points.Add(nodes[pp.ToString()]);
                }

                List<Vector2> simplified;
                simplified = Douglas.DouglasPeuckerReduction(points, tolerance);
                if (simplified != null && simplified.Count > 1)
                {
                    way.Update(fc.FitCurve(simplified.ToArray(), curveError));
                }
            }

            var newList = new LinkedList<Way>();
            foreach (var way in ways)
            {
                if (way.valid)
                {
                    newList.AddLast(way);
                }
                this.ways = newList;
            }
        }
    }
}