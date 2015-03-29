using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Mapper
{
    public class RoadMaker
    {
        public OSMInterface osm;
        private Randomizer rand;

        private Dictionary<RoadTypes, NetInfo> netInfos = new Dictionary<RoadTypes, NetInfo>();
        private Dictionary<uint, ushort> nodeMap = new Dictionary<uint, ushort>();
        bool Pedestrians;

        public RoadMaker(string path, bool pedestrians, double scale, double tolerance, double curveTolerance,double tiles)
        {
            this.Pedestrians = pedestrians;
            this.osm = new OSMInterface(path, scale, tolerance, curveTolerance, tiles);
            this.rand = new Randomizer(0u);

            var roadTypes = Enum.GetNames(typeof(RoadTypes));
            for (var i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i += 1)
            {
                var pp = PrefabCollection<NetInfo>.GetPrefab((uint)i);
                if (roadTypes.Contains(pp.name.Replace(" ", "")))
                {
                    netInfos.Add((RoadTypes)Enum.Parse(typeof(RoadTypes), pp.name.Replace(" ", "")), pp);
                }
            }
        }


        public IEnumerator MakeRoad(int p)
        {
            var nm = Singleton<NetManager>.instance;

            if (! nm.CheckLimits())
            {
                yield return null;
            }
            var way = osm.processedWays[p];
            NetInfo ni = null;
            
            if (!Pedestrians && (way.roadTypes == RoadTypes.PedestrianGravel || way.roadTypes == RoadTypes.PedestrianPavement)){
                yield return null;
            }

            if (netInfos.ContainsKey(way.roadTypes))
            {
                ni = netInfos[way.roadTypes];
            }
            else
            {
                Debug.Log("Failed to find net info: " + way.roadTypes.ToString());
                yield return null;
            }

            if (!osm.nodes.ContainsKey(way.startNode) || !osm.nodes.ContainsKey(way.endNode))
            {
                yield return null;
            }

            ushort startNode;
            if (nodeMap.ContainsKey(way.startNode))
            {
                startNode = nodeMap[way.startNode];
            }
            else
            {
                CreateNode(out startNode, ref rand, ni, osm.nodes[way.startNode]);
                nodeMap.Add(way.startNode, startNode);
            }

            ushort endNode;
            if (nodeMap.ContainsKey(way.endNode))
            {
                endNode = nodeMap[way.endNode];
            }
            else
            {
                CreateNode(out endNode, ref rand, ni, osm.nodes[way.endNode]);
                nodeMap.Add(way.endNode, endNode);
            }

            var currentStartNode = startNode;
            for (var i = 0; i < way.segments.Count(); i += 1)
            {
                var segment = way.segments[i];
                ushort currentEndNode;
                if (i == way.segments.Count() - 1)
                {
                    currentEndNode = endNode;
                }
                else
                {
                    CreateNode(out currentEndNode, ref rand, ni, segment.endPoint);
                }

                ushort segmentId;

                Vector3 position = nm.m_nodes.m_buffer[(int)currentStartNode].m_position;
                Vector3 position2 = nm.m_nodes.m_buffer[(int)currentEndNode].m_position;
                if (segment.controlA.x == 0f && segment.controlB.x == 0f)
                {
                //    Vector3 vector = VectorUtils.NormalizeXZ(segment.endPoint - segment.startPoint);
                    Vector3 vector = position2 - position;
                    vector = VectorUtils.NormalizeXZ(vector);
                    if (nm.CreateSegment(out segmentId, ref rand, ni, currentStartNode, currentEndNode, vector, -vector, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                    {
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                else
                {
                    var control = new Vector3(segment.controlA.x,0,segment.controlA.y);
                    //control.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(control);
                    var control2 = new Vector3(segment.controlB.x,0, segment.controlB.y);
                    //control2.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(control2);
                    //Vector3 entry = VectorUtils.NormalizeXZ(Bezier3.Tangent(position, segment.controlA, segment.controlB, position2, 0f));
                    //Vector3 exit = VectorUtils.NormalizeXZ(Bezier3.Tangent(position, segment.controlA, segment.controlB, position2, 1f));
                    Vector3 entry = VectorUtils.NormalizeXZ(control - position);
                    Vector3 exit = VectorUtils.NormalizeXZ(position2 - control2);
                    if (nm.CreateSegment(out segmentId, ref rand, ni, currentStartNode, currentEndNode, entry, -exit, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                    {
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                currentStartNode = currentEndNode;
            }
            yield return null;
        }

        private void CreateNode(out ushort startNode, ref Randomizer rand, NetInfo netInfo, Vector2 oldPos)
        {
            var pos = new Vector3(oldPos.x, 0, oldPos.y);
            pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos,false,0f);
            var nm = Singleton<NetManager>.instance;
            nm.CreateNode(out startNode, ref rand, netInfo, pos, Singleton<SimulationManager>.instance.m_currentBuildIndex);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
            var node = nm.m_nodes.m_buffer[startNode];
            node.m_flags |= NetNode.Flags.OnGround;
        }




    }
}
