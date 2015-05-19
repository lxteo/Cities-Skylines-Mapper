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
    public class RoadMaker2
    {
        public OSM.OSMInterface osm;
        private Randomizer rand;

        private Dictionary<RoadTypes, NetInfo> netInfos = new Dictionary<RoadTypes, NetInfo>();
        private Dictionary<uint, ushort> nodeMap = new Dictionary<uint, ushort>();
        public RoadMaker2(OSM.OSMInterface osm)
        {
            this.osm = osm;
            this.rand = new Randomizer(0u);

            var roadTypes = Enum.GetNames(typeof(RoadTypes));
            for (var i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i += 1)
            {
                var pp = PrefabCollection<NetInfo>.GetPrefab((uint)i);
                if (pp != null)
                {
                    if (roadTypes.Contains(pp.name.Replace(" ", "")))
                    {
                        netInfos.Add((RoadTypes)Enum.Parse(typeof(RoadTypes), pp.name.Replace(" ", "")), pp);
                    }
                }
                
            }
        }


        public IEnumerator MakeRoad(int p,bool pedestrians, bool roads, bool highways)
        {
            var nm = Singleton<NetManager>.instance;

            if (! nm.CheckLimits())
            {
                yield return null;
            }
            var way = osm.ways.ElementAt(p);
            NetInfo ni = null;

            if (way.roadTypes == RoadTypes.None)
            {
                yield break;
            }
            if (!pedestrians && ((int)way.roadTypes <= (int)RoadTypes.PedestrianElevated))
            {
                yield break;
            }

            if (!roads && (int)way.roadTypes > (int)RoadTypes.PedestrianElevated && (int)way.roadTypes < (int)RoadTypes.TrainTrack)
            {
                yield break;
            }

            if (!highways && (int)way.roadTypes >= (int)RoadTypes.TrainTrack)
            {
                yield break;
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
            float elevation = way.layer;
            if (elevation < 0)
            {
                yield return null;
            }
            else if (elevation > 0)
            {
                elevation *= 11f;

                var errors = default(ToolBase.ToolErrors);
                ni = ni.m_netAI.GetInfo(elevation, elevation, 5f, false, false, false, false, ref errors);

            }
            if (!osm.nodes.ContainsKey(way.startNode) || !osm.nodes.ContainsKey(way.endNode))
            {
                yield return null;
            }

            ushort startNode;
            if (nodeMap.ContainsKey(way.startNode))
            {
                startNode = nodeMap[way.startNode];
                AdjustElevation(startNode, elevation);
            }
            else
            {
                CreateNode(out startNode, ref rand, ni, osm.nodes[way.startNode],elevation);
                AdjustElevation(startNode, elevation);
                nodeMap.Add(way.startNode, startNode);
            }

            ushort endNode;
            if (nodeMap.ContainsKey(way.endNode))
            {
                endNode = nodeMap[way.endNode];
                AdjustElevation(endNode, elevation);
            }
            else
            {
                CreateNode(out endNode, ref rand, ni, osm.nodes[way.endNode],elevation);
                AdjustElevation(endNode, elevation);
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
                    CreateNode(out currentEndNode, ref rand, ni, segment.endPoint, elevation);
                    AdjustElevation(currentEndNode, elevation);
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

        private void AdjustElevation(ushort startNode, float elevation)
        {
            var nm = Singleton<NetManager>.instance;
            var node = nm.m_nodes.m_buffer[startNode];
            var ele = (byte)Mathf.Clamp(Mathf.RoundToInt(Math.Max(node.m_elevation, elevation)), 0, 255);
            var terrain = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(node.m_position, false, 0f);
            node.m_elevation = ele;
            node.m_position = new Vector3(node.m_position.x, ele + terrain, node.m_position.z);
            if (elevation < 11f)
            {
                node.m_flags |= NetNode.Flags.OnGround;
            }
            else
            {
                node.m_flags &= ~NetNode.Flags.OnGround;
                UpdateSegment(node.m_segment0,elevation);
                UpdateSegment(node.m_segment1, elevation);
                UpdateSegment(node.m_segment2, elevation);
                UpdateSegment(node.m_segment3, elevation);
                UpdateSegment(node.m_segment4, elevation);
                UpdateSegment(node.m_segment5, elevation);
                UpdateSegment(node.m_segment6, elevation);
                UpdateSegment(node.m_segment7, elevation);
                
            }
            nm.m_nodes.m_buffer[startNode] = node;
            //Singleton<NetManager>.instance.UpdateNode(startNode);
        }

        private void UpdateSegment(ushort segmentId, float elevation)
        {
            if (segmentId == 0)
            {
                return;
            }
            var nm = Singleton<NetManager>.instance;
            if (elevation > 4)
            {
                var errors = default(ToolBase.ToolErrors);
                nm.m_segments.m_buffer[segmentId].Info = nm.m_segments.m_buffer[segmentId].Info.m_netAI.GetInfo(elevation,elevation,5,false,false,false,false,ref errors);
            }
        }

        private void CreateNode(out ushort startNode, ref Randomizer rand, NetInfo netInfo, Vector2 oldPos, float elevation)
        {
            var pos = new Vector3(oldPos.x, 0, oldPos.y);
            pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos,false,0f);
            var nm = Singleton<NetManager>.instance;
            nm.CreateNode(out startNode, ref rand, netInfo, pos, Singleton<SimulationManager>.instance.m_currentBuildIndex);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
        }




    }
}
