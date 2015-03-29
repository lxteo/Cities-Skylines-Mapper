using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Mapper
{
    public class OSMInterface
    {
        public RoadMapping mapping;
        private FitCurves fc;

        public Dictionary<uint,Vector2> nodes = new Dictionary<uint,Vector2>();
        private LinkedList<Way> ways = new LinkedList<Way>();
        public List<ProcessedWay> processedWays = new List<ProcessedWay>();

        double tolerance = 10;
        double curveError = 5;

        public OSMInterface(string path, double scale, double tolerance, double curveTolerance,double tiles)
        {
            this.tolerance = tolerance;
            this.curveError = curveTolerance;

            mapping = new RoadMapping(tiles);
            fc = new FitCurves();

            var serializer = new XmlSerializer(typeof(Mapper.osm));
            StreamReader reader = new StreamReader(path);
            var osm = (Mapper.osm)serializer.Deserialize(reader);
            reader.Close();

            mapping.InitBoundingBox(osm.bounds,scale);

            foreach (var node in osm.node)
            {
                if (!nodes.ContainsKey(node.id) && node.lat != 0 && node.lon != 0)
                {
                    Vector2 pos = Vector2.zero;
                    if (mapping.GetPos(node.lon, node.lat, ref pos))
                    {
                        nodes.Add(node.id, pos);
                    }                    
                }                
            }

            foreach (var way in osm.way.OrderBy(c=> c.changeset))
            {
                RoadTypes rt = RoadTypes.None;
                List<uint> points = null;

                if (mapping.Mapped(way,ref points, ref rt))
                {
                    Vector2 previousPoint= Vector2.zero;
                    var currentList = new List<uint>();
                    for(var i = 0; i <points.Count; i +=1)
                    {
                        var pp = points[i];
                        if (nodes.ContainsKey(pp))
                        {
                            currentList.Add(pp);
                            previousPoint = nodes[pp];
                        }
                        else
                        {
                            if (currentList.Count() > 1 || currentList.Contains(pp))
                            {
                                ways.AddLast(new Way(currentList, rt));
                                currentList = new List<uint>();
                            }
                        }
                        
                    }
                    if (currentList.Count() > 1)
                    {
                        ways.AddLast(new Way(currentList, rt));
                    }                    
                }
            }

            var intersection = new Dictionary<uint,List<Way>>();
            foreach (var ww in ways){
                foreach (var pp in ww.nodes)
                {
                    if (!intersection.ContainsKey(pp))
                    {
                        intersection.Add(pp, new List<Way>());                        
                    }
                    intersection[pp].Add(ww);
                }
            }

            foreach (var inter in intersection)
            {
                if (inter.Value.Count > 1)
                {
                    foreach (var way in inter.Value)
                    {
                        SplitWay(way, inter.Key);
                    }
                }
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
                for (var i = 0; i < way.nodes.Count() -1 ; i +=1)
                {
                    length += (nodes[way.nodes[i +1]] - nodes[way.nodes[i]]).magnitude;
                }
                int segments = Mathf.FloorToInt(length / 100f) + 1;             
                float averageLength = length / (float)segments;
                if (segments <= 1 ){
                    continue;
                }
                length = 0;
                var splits = new List<int>();
                for (var i = 0; i < way.nodes.Count() - 1; i += 1)
                {
                    length += (nodes[way.nodes[i + 1]] - nodes[way.nodes[i]]).magnitude;
                    if (length > averageLength && i != way.nodes.Count-2)
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

            foreach (var waySplits in allSplits){
                SplitWay(waySplits.Key, waySplits.Value);
            }
        }
        
        private void SplitWay(Way way, List<int> splits)
        {
            var index = ways.Find(way);
            for (var i = 0; i < splits.Count(); i +=1)
            {
                var nextIndex = way.nodes.Count() - 1;
                if (i != splits.Count - 1)
                {
                    nextIndex = splits[i + 1];
                }
                var newWay = new Way(way.nodes.GetRange(splits[i], 1 + nextIndex - splits[i]), way.rt);
                ways.AddAfter(index, newWay);
            }
            way.nodes.RemoveRange(splits[0] + 1, way.nodes.Count() - splits[0] - 1);
        }

        private Way SplitWay(Way inter, uint pp)
        {
            var removeIndex = inter.nodes.IndexOf(pp);
            if (removeIndex <= 0 || removeIndex == inter.nodes.Count() - 1)
            {
                return null;
            }
            var newWay = new Way(inter.nodes.GetRange(removeIndex, inter.nodes.Count() - removeIndex), inter.rt);
            ways.AddAfter(ways.Find(inter), newWay);
            inter.nodes.RemoveRange(removeIndex + 1, inter.nodes.Count() - removeIndex - 1);
            return newWay;
        }

        private void SimplifyWays()
        {
            foreach (var way in ways)
            {
                var points = new List<Vector2>();
                foreach (var pp in way.nodes)
                {
                    points.Add(nodes[pp]);
                }

                List<Vector2> simplified;
                simplified = Douglas.DouglasPeuckerReduction(points, tolerance);
                if (simplified != null && simplified.Count > 1)
                {
                    var fitted = fc.FitCurve(simplified.ToArray(), curveError);
                    var processed = new ProcessedWay(way, fitted);
                    this.processedWays.Add(processed);
                }
                else
                {
                    var a = 1;
                }
            }
        }


        
    }
}
