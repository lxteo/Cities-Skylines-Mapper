using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapper
{
    public class RoadMaker2
    {
        public OSM.OSMInterface osm;
        private Randomizer rand;

        private Dictionary<RoadTypes, NetInfo> osmRoadTypeToGameRoadTypeMap = new Dictionary<RoadTypes, NetInfo>();
        private Dictionary<ulong, ushort> nodeMap = new Dictionary<ulong, ushort>();

        public RoadMaker2(OSM.OSMInterface osm)
        {
            this.osm = osm;
            this.rand = new Randomizer(0u);

            var roadTypeNames = Enum.GetNames(typeof(RoadTypes));
            for (var i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i += 1)
            {
                var roadAsset = PrefabCollection<NetInfo>.GetPrefab((uint) i);
                if (roadAsset != null)
                {
                    if (roadTypeNames.Contains(roadAsset.name.Replace(" ", "")))
                    {
                        osmRoadTypeToGameRoadTypeMap.Add((RoadTypes) Enum.Parse(typeof(RoadTypes), roadAsset.name.Replace(" ", "")), roadAsset);
                    }
                }
            }
        }


        public IEnumerator Make(int p, bool shouldMakePedestrian, bool shouldMakeRoad, bool shouldMakeHighway)
        {
            var nm = Singleton<NetManager>.instance;

            if (!nm.CheckLimits())
            {
                yield return null;
            }
            var osmMappedRoad = osm.ways.ElementAt(p);
            NetInfo gameRoad = null;

            // --------------------------- Validation logic ---------------------------------------------------------------------------------------------------

            // Don't make roads the don't map onto a specific type.
            if (osmMappedRoad.roadTypes == RoadTypes.None)
            {
                yield break;
            }

            // Don't make pedestrian ways if option is not selected.
            if (!shouldMakePedestrian && ((int) osmMappedRoad.roadTypes <= (int) RoadTypes.PedestrianElevated))
            {
                yield break;
            }

            // Don't make roads if option is not selected.
            if (!shouldMakeRoad && (int) osmMappedRoad.roadTypes > (int) RoadTypes.PedestrianElevated && (int) osmMappedRoad.roadTypes < (int) RoadTypes.TrainTrack)
            {
                yield break;
            }

            // Don't make highways if option is not selected.
            if (!shouldMakeHighway && (int) osmMappedRoad.roadTypes >= (int) RoadTypes.TrainTrack)
            {
                yield break;
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------

            if (osmRoadTypeToGameRoadTypeMap.ContainsKey(osmMappedRoad.roadTypes))
            {
                // If the current road maps onto a known road type mapping.
                gameRoad = osmRoadTypeToGameRoadTypeMap[osmMappedRoad.roadTypes];
            }
            else
            {
                Debug.Log("Failed to find net info: " + osmMappedRoad.roadTypes);
                yield return null;
            }

            float osmMappedRoadElevation = osmMappedRoad.layer;
            if (osmMappedRoadElevation < 0)
            {
                // Don't map subsurface roads.
                yield return null;
            }
            else if (osmMappedRoadElevation > 0)
            {
                // Adjust the elevation, by what I'm assuming is a factor that normalizes real world height to game engine height.
                osmMappedRoadElevation *= 11f;

                var errors = default(ToolBase.ToolErrors);
                gameRoad = gameRoad.m_netAI.GetInfo(osmMappedRoadElevation, osmMappedRoadElevation, 5f, false, false, false, false, ref errors);
            }

            if (!osm.nodes.ContainsKey(osmMappedRoad.startNode.ToString()) || !osm.nodes.ContainsKey(osmMappedRoad.endNode.ToString()))
            {
                // Return if this way's start and end nodes are not contained in the osm nodes data.
                yield return null;
            }


            ushort startNode;
            if (nodeMap.ContainsKey(osmMappedRoad.startNode))
            {
                // If the node map contains the startnode for this road, adjust the elevation to the current segments elevation.
                startNode = nodeMap[osmMappedRoad.startNode];
                AdjustElevation(startNode, osmMappedRoadElevation);
            }
            else
            {
                // Otherwise, created a new node and adjust the elevation and add it to the node map.
                CreateNode(out startNode, ref rand, gameRoad, osm.nodes[osmMappedRoad.startNode.ToString()], osmMappedRoadElevation);
                AdjustElevation(startNode, osmMappedRoadElevation);
                nodeMap.Add(osmMappedRoad.startNode, startNode);
            }

            ushort endNode;
            if (nodeMap.ContainsKey(osmMappedRoad.endNode))
            {
                // If the node map contains the endNode for this road, adjust the elevation to the current segment's value.
                endNode = nodeMap[osmMappedRoad.endNode];
                AdjustElevation(endNode, osmMappedRoadElevation);
            }
            else
            {
                // Otherwise created a new end node and adjust the elevation and add it to the node map.
                CreateNode(out endNode, ref rand, gameRoad, osm.nodes[osmMappedRoad.endNode.ToString()], osmMappedRoadElevation);
                AdjustElevation(endNode, osmMappedRoadElevation);
                nodeMap.Add(osmMappedRoad.endNode, endNode);
            }

            var currentStartNode = startNode;
            for (var i = 0; i < osmMappedRoad.segments.Count(); i += 1)
            {
                var segment = osmMappedRoad.segments[i];
                ushort currentEndNode;
                if (i == osmMappedRoad.segments.Count() - 1)
                {
                    currentEndNode = endNode;
                }
                else
                {
                    CreateNode(out currentEndNode, ref rand, gameRoad, segment.endPoint, osmMappedRoadElevation);
                    AdjustElevation(currentEndNode, osmMappedRoadElevation);
                }

                ushort segmentId;

                Vector3 position = nm.m_nodes.m_buffer[currentStartNode].m_position;
                Vector3 position2 = nm.m_nodes.m_buffer[currentEndNode].m_position;
                if (segment.controlA.x == 0f && segment.controlB.x == 0f)
                {
                    //    Vector3 vector = VectorUtils.NormalizeXZ(segment.endPoint - segment.startPoint);
                    Vector3 vector = position2 - position;
                    vector = VectorUtils.NormalizeXZ(vector);
                    if (nm.CreateSegment(out segmentId, ref rand, gameRoad, currentStartNode, currentEndNode, vector, -vector,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                    {
                        Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                    }
                }
                else
                {
                    var control = new Vector3(segment.controlA.x, 0, segment.controlA.y);
                    //control.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(control);
                    var control2 = new Vector3(segment.controlB.x, 0, segment.controlB.y);
                    //control2.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(control2);
                    //Vector3 entry = VectorUtils.NormalizeXZ(Bezier3.Tangent(position, segment.controlA, segment.controlB, position2, 0f));
                    //Vector3 exit = VectorUtils.NormalizeXZ(Bezier3.Tangent(position, segment.controlA, segment.controlB, position2, 1f));
                    Vector3 entry = VectorUtils.NormalizeXZ(control - position);
                    Vector3 exit = VectorUtils.NormalizeXZ(position2 - control2);
                    if (nm.CreateSegment(out segmentId, ref rand, gameRoad, currentStartNode, currentEndNode, entry, -exit,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
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
            var ele = (byte) Mathf.Clamp(Mathf.RoundToInt(Math.Max(node.m_elevation, elevation)), 0, 255);
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
                UpdateSegment(node.m_segment0, elevation);
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
            var netManager = Singleton<NetManager>.instance;
            if (elevation > 4)
            {
                var errors = default(ToolBase.ToolErrors);
                netManager.m_segments.m_buffer[segmentId].Info =
                    netManager.m_segments.m_buffer[segmentId].Info.m_netAI.GetInfo(elevation, elevation, 5, false, false, false, false, ref errors);
            }
        }

        private void CreateNode(out ushort startNode, ref Randomizer rand, NetInfo netInfo, Vector2 oldPos,
            float elevation)
        {
            var pos = new Vector3(oldPos.x, 0, oldPos.y);
            pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos, false, 0f);
            var nm = Singleton<NetManager>.instance;
            nm.CreateNode(out startNode, ref rand, netInfo, pos,
                Singleton<SimulationManager>.instance.m_currentBuildIndex);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
        }
    }
}