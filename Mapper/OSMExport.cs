using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Mapper
{
    class OSMExport
    {
        public RoadMapping mapping;
        private int nodeCount;
        private int wayCount;
        private List<osmNode> nodes;
        private List<osmWay> ways;
        private Vector3 middle;
        private float buildingCount;


        internal void Export()
        {            
            mapping = new RoadMapping(4.5);
            var osm = new osm();
            osm.version = 0.6M;
            osm.upload = false;
            osm.meta = new osmMeta { osm_base = DateTime.Now };
            osm.generator = "Cities Skylines Magic Mapper Mod";
            osm.note = Singleton<SimulationManager>.instance.m_metaData.m_CityName;
            osm.bounds = new osmBounds { minlon = 35.753054M, minlat = 34.360353M, maxlon = 35.949310M, maxlat = 34.522050M };
            var nm = Singleton<NetManager>.instance;

            mapping.InitBoundingBox(osm.bounds, 1);

            nodeCount = 128000;
            wayCount = 128000;

            AddNodesAndWays();
            AddBuildings();
            AddDistricts();
            AddCity();

            osm.node = FilterUnusedNodes();
            osm.way = ways.ToArray();
            
            var serializer = new XmlSerializer(typeof(Mapper.osm));
            var ms = new StreamWriter(Singleton<SimulationManager>.instance.m_metaData.m_CityName + ".osm");
            serializer.Serialize(ms,osm);
            ms.Close();
        }

        private void AddBuildings()
        {
            var bm = Singleton<BuildingManager>.instance;
            for (var i = 0; i < 32768; i += 1)
            {
                var building = bm.m_buildings.m_buffer[i];
                if ((building.m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    var way = AddBuilding((ushort)i,building);
                    if (way != null)
                    {
                        ways.Add(way);
                    }
                }
            }
        }

        private osmWay AddBuilding(ushort buildingId, Building data)
        {
            int width = data.Width;
            int length = data.Length;
            Vector3 a = new Vector3(Mathf.Cos(data.m_angle), 0f, Mathf.Sin(data.m_angle)) * 8f;
            Vector3 a2 = new Vector3(a.z, 0f, -a.x);
            

            var tags = new List<osmWayTag>();

            string amenity = "";
            mapping.GetTags(buildingId,data, tags,ref amenity);

            wayCount += 1;
            if (tags.Count == 0)
            {
                return null;
            }

            this.middle += data.m_position;
            this.buildingCount += 1;

            osmWayND[] nd = new osmWayND[5];
            var firstNode = AddNode(data.m_position - (float)width * 0.5f * a - (float)length * 0.5f * a2);
            nd[0] = new osmWayND { @ref = firstNode };
            nd[1] = new osmWayND { @ref = AddNode(data.m_position + (float)width * 0.5f * a - (float)length * 0.5f * a2) };
            nd[2] = new osmWayND { @ref = AddNode(data.m_position + (float)width * 0.5f * a + (float)length * 0.5f * a2) };
            nd[3] = new osmWayND { @ref = AddNode(data.m_position - (float)width * 0.5f * a + (float)length * 0.5f * a2) };
            nd[4] = new osmWayND { @ref = firstNode };

            if (amenity != "")
            {
                var ammenityTag = new Dictionary<string, string>();
                ammenityTag.Add("amenity", amenity);
                AddNode(data.m_position, ammenityTag);
            }
            return new osmWay { changeset = 50000000, id = (uint)wayCount, timestamp = DateTime.Now, user = "CS", version = 1, nd = nd, tag = tags.ToArray() };            
        }

        private void AddCity()
        {
            var lon = 0M;
            var lat = 0M;
            mapping.GetPos(new Vector3(this.middle.x / this.buildingCount, 0, this.middle.y / this.buildingCount), out lon, out lat);
            var md = Singleton<SimulationManager>.instance;
            var tags = new List<osmNodeTag>();
            tags.Add(new osmNodeTag { k = "name", v = md.m_metaData.m_CityName });
            tags.Add(new osmNodeTag { k = "place", v = "city" });
            tags.Add(new osmNodeTag { k = "population", v = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_populationData.m_finalCount.ToString() });

            nodes.Add(new osmNode { changeset = 50000000, id = (uint)nodeCount, version = 1, timestamp = DateTime.Now, user = "CS", lon = lon, lat = lat, tag = tags.ToArray() });
        }

        private void AddDistricts()
        {
            var dm = Singleton<DistrictManager>.instance;
            for (var i = 1; i < 128; i +=1){
                if ((dm.m_districts.m_buffer[i].m_flags & District.Flags.Created) != District.Flags.None)
                {                    
                    AddDistrict(dm.GetDistrictName(i),"suburb", dm.m_districts.m_buffer[i].m_nameLocation);                    
                }
            }
        }

        private void AddDistrict(string name,string place, Vector3 vector3)
        {
            decimal lon = 0;
            decimal lat = 0;
            mapping.GetPos(vector3, out lon, out lat);
            var tags = new List<osmNodeTag>();
            tags.Add(new osmNodeTag { k = "name", v = name });
            tags.Add(new osmNodeTag { k = "place", v = place });

            nodes.Add(new osmNode { changeset = 50000000, id = (uint)nodeCount, version = 1, timestamp = DateTime.Now, user = "CS", lon = lon, lat = lat,tag = tags.ToArray() });
            nodeCount += 1;
        }

        private osmNode[] FilterUnusedNodes()
        {

            var found = new HashSet<uint>();
            foreach (var way in ways)
            {
                foreach (var nd in way.nd)
                {
                    found.Add(nd.@ref);
                }
            }

            var finalNodes = new List<osmNode>();
            foreach (var node in nodes)
            {
                if (node.tag != null || found.Contains(node.id))
                {
                    finalNodes.Add(node);
                }
            }
            return finalNodes.ToArray();
        }

        private void AddNodesAndWays()
        {
            var nm = Singleton<NetManager>.instance;
            nodes = new List<osmNode>();
            for (var i = 0; i < 32768u; i += 1)
            {
                if ((nm.m_nodes.m_buffer[i].m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
                {
                    nodes.Add(AddNode(i, nm.m_nodes.m_buffer[i]));
                }
            }

            ways = new List<osmWay>();
            for (var i = 0; i < 32768u; i += 1)
            {
                if ((nm.m_segments.m_buffer[i].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None)
                {
                    var result = AddWay(i, nm.m_segments.m_buffer[i]);
                    if (result != null)
                    {
                        ways.Add(result);
                    }
                }
            }
        }

        private osmWay AddWay(int i, NetSegment netSegment)
        {
            var nm = Singleton<NetManager>.instance;
            if (netSegment.m_startNode == 0 || netSegment.m_endNode == 0)
            {
                return null;
            }
            var startNode = netSegment.m_startNode;
            var endNode = netSegment.m_endNode;
            var startDirection = netSegment.m_startDirection;
            var endDirection = netSegment.m_endDirection;

            if ((netSegment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None){
                startNode = netSegment.m_endNode;
                endNode = netSegment.m_startNode;
                startDirection = netSegment.m_endDirection;
                endDirection = netSegment.m_startDirection;
            }
            osmWayND[] nd;
            if ( Vector3.Angle(startDirection,-endDirection) > 3f)
            {
                var start = nm.m_nodes.m_buffer[startNode].m_position;
                var end = nm.m_nodes.m_buffer[endNode].m_position;
                Vector3 a = Vector3.zero;
                Vector3 b = Vector3.zero;
                NetSegment.CalculateMiddlePoints(start, startDirection, end, endDirection, false, false, out a, out b);

                var bezier = new Bezier3(start,a,b,end);

                nd = new osmWayND[5];
                nd[0] = new osmWayND { @ref = startNode };
                nd[1] = new osmWayND { @ref = AddNode(bezier.Position( 0.25f)) };
                nd[2] = new osmWayND { @ref = AddNode(bezier.Position( 0.5f)) };
                nd[3] = new osmWayND { @ref = AddNode(bezier.Position( 0.75f)) };
                nd[4] = new osmWayND { @ref = endNode };    
            }
            else
            {
                nd = new osmWayND[2];
                nd[0] = new osmWayND { @ref = startNode };
                nd[1] = new osmWayND { @ref = endNode };
            }

            byte elevation = (byte)(Mathf.Clamp((nm.m_nodes.m_buffer[startNode].m_elevation + nm.m_nodes.m_buffer[endNode].m_elevation) / 2, 0, 255));
            var tags = new List<osmWayTag>();
            if (!mapping.GetTags(elevation, netSegment, tags))
            {
                return null;
            }
            return new osmWay{ changeset= 50000000,id = (uint)i, timestamp = DateTime.Now , user = "CS", version = 1, nd = nd,tag = tags.ToArray()};
        }

        private uint AddNode(Vector3 vector3, Dictionary<string, string> tags = null)
        {
            var node = AddNode(nodeCount, vector3);
            if (tags != null)
            {
                var ost = new List<osmNodeTag>();
                foreach(var kvp in tags){
                    ost.Add(new osmNodeTag { k = kvp.Key, v = kvp.Value });
                }
                node.tag = ost.ToArray();
            }
            nodes.Add(node);
            nodeCount += 1;
            return (uint)(nodeCount - 1);
        }

        private osmNode AddNode(int i, NetNode netNode)
        {
            return AddNode(i,netNode.m_position);            
        }

        private osmNode AddNode(int i, Vector3 vector3)
        {
            decimal lon = 0;
            decimal lat = 0;
            mapping.GetPos(vector3, out lon, out lat);
            return new osmNode { changeset = 50000000, id = (uint)i, version = 1, timestamp = DateTime.Now, user = "CS", lon = lon, lat = lat, };
        }

    }
}
