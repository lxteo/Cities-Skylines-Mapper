using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mapper.Curves
{
    /*
         * From http://www.codeproject.com/KB/cs/Douglas-Peucker_Algorithm.aspx
         * By Craig Selbert
         */
        public static class Douglas
        {
            /// <summary>
            /// Uses the Douglas Peucker algorithm to reduce the number of points.
            /// </summary>
            /// <param name="Points">The points.</param>
            /// <param name="Tolerance">The tolerance.</param>
            /// <returns></returns>
            public static List<Vector2> DouglasPeuckerReduction(List<Vector2> Points, Double Tolerance)
            {
                if (Points == null || Points.Count < 3)
                    return Points;

                int firstPoint = 0;
                int lastPoint = Points.Count - 1;
                List<int> pointIndexsToKeep = new List<int>();

                //Add the first and last index to the keepers
                pointIndexsToKeep.Add(firstPoint);
                pointIndexsToKeep.Add(lastPoint);

                //The first and the last point cannot be the same
                while (Points[firstPoint].Equals(Points[lastPoint]))
                {
                    lastPoint--;
                    if (lastPoint <= firstPoint)
                    {
                        return null;
                    }
                }

                DouglasPeuckerReduction(Points, firstPoint, lastPoint,Tolerance, ref pointIndexsToKeep);

                List<Vector2> returnPoints = new List<Vector2>();
                pointIndexsToKeep.Sort();
                foreach (Int32 index in pointIndexsToKeep)
                {
                    returnPoints.Add(Points[index]);
                }

                return returnPoints;
            }

            /// <summary>
            /// Douglases the peucker reduction.
            /// </summary>
            /// <param name="points">The points.</param>
            /// <param name="firstPoint">The first point.</param>
            /// <param name="lastPoint">The last point.</param>
            /// <param name="tolerance">The tolerance.</param>
            /// <param name="pointIndexsToKeep">The point index to keep.</param>
            private static void DouglasPeuckerReduction(List<Vector2> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep)
            {
                Double maxDistance = 0;
                Int32 indexFarthest = 0;

                for (Int32 index = firstPoint; index < lastPoint; index++)
                {
                    Double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        indexFarthest = index;
                    }
                }

                if (maxDistance > tolerance && indexFarthest != 0)
                {
                    //Add the largest point that exceeds the tolerance
                    pointIndexsToKeep.Add(indexFarthest);

                    DouglasPeuckerReduction(points, firstPoint,
                    indexFarthest, tolerance, ref pointIndexsToKeep);
                    DouglasPeuckerReduction(points, indexFarthest,
                    lastPoint, tolerance, ref pointIndexsToKeep);
                }
            }

            /// <summary>
            /// The distance of a point from a line made from point1 and point2.
            /// </summary>
            /// <param name="pt1">The PT1.</param>
            /// <param name="pt2">The PT2.</param>
            /// <param name="p">The p.</param>
            /// <returns></returns>
            public static Double PerpendicularDistance
                (Vector2 Point1, Vector2 Point2, Vector2 Point)
            {

                Double area = Math.Abs(.5 * (Point1.x * Point2.y + Point2.x *
                Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x *
                Point2.y - Point1.x * Point.y));
                Double bottom = Math.Sqrt(Math.Pow(Point1.x - Point2.x, 2) +
                Math.Pow(Point1.y - Point2.y, 2));
                Double height = area / bottom * 2;

                return height;
            }
        }
    }


