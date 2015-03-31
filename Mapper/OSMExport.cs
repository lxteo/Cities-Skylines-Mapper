using ColossalFramework;
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
        
        internal void Export()
        {            
            mapping = new RoadMapping(4.5);
            var osm = new osm();
            osm.version = 0.6M;
            
            osm.meta = new osmMeta { osm_base = DateTime.Now };

            osm.note = Singleton<SimulationManager>.instance.m_metaData.m_CityName;
            osm.bounds = new osmBounds { minlon = 35.753054M, minlat = 34.360353M, maxlon = 35.949310M, maxlat = 34.522050M };
            var nm = Singleton<NetManager>.instance;

            mapping.InitBoundingBox(osm.bounds, 1);

            var nodes = new osmNode[nm.m_nodeCount];
            var index = 0;
            for (var i = 0; i < 32768u; i += 1)
            {
                if ((nm.m_nodes.m_buffer[i].m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
                {
                    nodes[index] = AddNode(i, nm.m_nodes.m_buffer[i]);
                    index += 1;
                }
            }
            osm.node = nodes;

            var ways = new osmWay[nm.m_segmentCount];
            index = 0;
            for (var i = 0; i < 32768u; i += 1)
            {
                if ((nm.m_segments.m_buffer[i].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None)
                {
                    var result = AddWay(i, nm.m_segments.m_buffer[i]);
                    if (result != null)
                    {
                        ways[index] = result;
                        index += 1;
                    }
                }
            }
            osm.way = ways;


            var serializer = new XmlSerializer(typeof(Mapper.osm));
            var ms = new StreamWriter(Singleton<SimulationManager>.instance.m_metaData.m_CityName + ".osm");
            serializer.Serialize(ms,osm);
            ms.Close();
        }

        private osmWay AddWay(int i, NetSegment netSegment)
        {
            if (netSegment.m_startNode == 0 || netSegment.m_endNode == 0)
            {
                return null;
            }
            var nd = new osmWayND[2];
            var nm = Singleton<NetManager>.instance;
            byte elevation = (byte)(Mathf.Clamp((nm.m_nodes.m_buffer[netSegment.m_startNode].m_elevation + nm.m_nodes.m_buffer[netSegment.m_endNode].m_elevation) / 2,0,255));
            nd[0] = new osmWayND{ @ref=netSegment.m_startNode};
            nd[1] = new osmWayND{ @ref=netSegment.m_endNode};
            var tags = new List<osmWayTag>();
            if (!mapping.GetTags(elevation, netSegment, tags))
            {
                return null;
            }
            return new osmWay{ changeset= 50000000,id = (uint)i, timestamp = DateTime.Now , user = "CS", version = 1, nd = nd,tag = tags.ToArray()};
        }

        private osmNode AddNode(int i, NetNode netNode)
        {
            decimal lon = 0;
            decimal lat = 0;
            mapping.GetPos(netNode.m_position, out lon, out lat);
            return new osmNode { changeset = 50000000, id = (uint)i,version = 1, timestamp=DateTime.Now, user="CS", lon = lon, lat = lat,  };
        }

    }
}
