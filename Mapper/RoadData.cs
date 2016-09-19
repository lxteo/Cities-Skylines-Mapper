using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapper
{
//    public class ProcessedWay
//    {
//        public int layer;

//        public RoadTypes roadTypes;

//        public ProcessedWay(Way way, List<Segment> fitted)
//        {
//            this.layer = way.layer;
//            this.segments = fitted;
//            this.roadTypes = way.rt;
//            this.startNode = way.nodes[0];
//            this.endNode = way.nodes[way.nodes.Count() - 1];            
//        }
//    }

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

    public class Way
    {
        public bool valid = false;
        public List<ulong> nodes = new List<ulong>();
        public List<Segment> segments;

        public RoadTypes roadTypes;
        public int layer;

        public ulong startNode
        {
            get { return nodes[0]; }
        }

        public ulong endNode
        {
            get { return nodes[nodes.Count() - 1]; }
        }

        public Way(List<ulong> points, RoadTypes rt, int layer)
        {
            this.roadTypes = rt;
            this.nodes = points;
            this.layer = layer;
        }


        internal void Update(List<Segment> list)
        {
            valid = true;
            segments = list;
        }
    }
}