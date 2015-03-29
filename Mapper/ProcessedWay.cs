using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mapper
{
    public class ProcessedWay
    {
        public uint startNode;
        public uint endNode;

        public List<Segment> segments;
        public RoadTypes roadTypes;

        public ProcessedWay(Way way, List<Segment> fitted)
        {
            this.segments = fitted;
            this.roadTypes = way.rt;
            this.startNode = way.nodes[0];
            this.endNode = way.nodes[way.nodes.Count() - 1];            
        }
    }

    public class Segment
    {
        public Vector2 startPoint;
        public Vector2 controlA;
        public Vector2 controlB;
        public Vector2 endPoint;

        public Segment(Vector2 startPoint, Vector2 endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public Segment(Vector2[] bezCurve)
        {
            this.startPoint = bezCurve[0];
            this.controlA = bezCurve[1];
            this.controlB = bezCurve[2];
            this.endPoint = bezCurve[3];
        }
    }
}
