using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* Adapted from http://stackoverflow.com/questions/5525665/smoothing-a-hand-drawn-curve */

namespace Mapper.Curves
{
/*
An Algorithm for Automatically Fitting Digitized Curves
by Philip J. Schneider
from "Graphics Gems", Academic Press, 1990
*/
    /*
An Algorithm for Automatically Fitting Digitized Curves
by Philip J. Schneider
from "Graphics Gems", Academic Press, 1990
*/
    public class FitCurves
    {
        /*  Fit the Bezier curves */

        private const int MAXPOINTS = 30000;
        public List<Segment> FitCurve(Vector2[] d, double error)
        {
            

            Vector2 tHat1, tHat2;    /*  Unit tangent vectors at endpoints */

            tHat1 = ComputeLeftTangent(d, 0);
            tHat2 = ComputeRightTangent(d, d.Length - 1);
            List<Segment> result = new List<Segment>();

            //for (var i = 0; i < d.Length - 1; i += 1)
            //{
            //    result.Add(new Segment(d[i], d[i + 1]));
            //}
            //return result;

            FitCubic(d, 0, d.Length - 1, tHat1, tHat2, error, result);
            return result;
        }

        private void FitCubic(Vector2[] d, int first, int last, Vector2 tHat1, Vector2 tHat2, double error, List<Segment> result)
        {
            Vector2[] bezCurve; /*Control points of fitted Bezier curve*/
            float[] u;     /*  Parameter values for point  */
            float[] uPrime;    /*  Improved parameter values */
            double maxError;    /*  Maximum fitting error    */
            int splitPoint; /*  Point to split point set at  */
            int nPts;       /*  Number of points in subset  */
            double iterationError; /*Error below which you try iterating  */
            int maxIterations = 4; /*  Max times to try iterating  */
            Vector2 tHatCenter;      /* Unit tangent vector at splitPoint */
            int i;

            iterationError = error * error;
            nPts = last - first + 1;

            /*  Use heuristic if region only has two points in it */
            if (nPts == 2)
            {                
                result.Add(new Segment(d[first], d[last]));                
                return;
            }

            /*  Parameterize points, and attempt to fit curve */
            u = ChordLengthParameterize(d, first, last);
            bezCurve = GenerateBezier(d, first, last, u, tHat1, tHat2);

            /*  Find max deviation of points to fitted curve */
            maxError = ComputeMaxError(d, first, last, bezCurve, u, out splitPoint);
            if (maxError < error)
            {
                result.Add(new Segment(bezCurve));
                return;
            }


            /*  If error not too large, try some reparameterization  */
            /*  and iteration */
            if (maxError < iterationError)
            {
                for (i = 0; i < maxIterations; i++)
                {
                    uPrime = Reparameterize(d, first, last, u, bezCurve);
                    bezCurve = GenerateBezier(d, first, last, uPrime, tHat1, tHat2);
                    maxError = ComputeMaxError(d, first, last,
                               bezCurve, uPrime, out splitPoint);
                    if (maxError < error)
                    {
                        result.Add(new Segment(bezCurve));
                        return;
                    }
                    u = uPrime;
                }
            }

            /* Fitting failed -- split at max error point and fit recursively */
            tHatCenter = ComputeCenterTangent(d, splitPoint);
            FitCubic(d, first, splitPoint, tHat1, tHatCenter, error, result);
            tHatCenter = -tHatCenter;
            FitCubic(d, splitPoint, last, tHatCenter, tHat2, error, result);
        }

        Vector2[] GenerateBezier(Vector2[] d, int first, int last, float[] uPrime, Vector2 tHat1, Vector2 tHat2)
        {
            int i;
            Vector2[,] A = new Vector2[MAXPOINTS, 2];/* Precomputed rhs for eqn    */

            int nPts;           /* Number of pts in sub-curve */
            float[,] C = new float[2, 2];            /* Matrix C     */
            float[] X = new float[2];          /* Matrix X         */
            float det_C0_C1,      /* Determinants of matrices */
                    det_C0_X,
                    det_X_C1;
            float alpha_l,        /* Alpha values, left and right */
                    alpha_r;
            Vector2 tmp;            /* Utility variable     */
            Vector2[] bezCurve = new Vector2[4];    /* RETURN bezier curve ctl pts  */
            nPts = last - first + 1;

            /* Compute the A's  */
            for (i = 0; i < nPts; i++)
            {
                Vector2 v1, v2;
                v1 = tHat1;
                v2 = tHat2;
                v1 *= B1(uPrime[i]);
                v2 *= B2(uPrime[i]);
                A[i, 0] = v1;
                A[i, 1] = v2;
            }

            /* Create the C and X matrices  */
            C[0, 0] = 0f;
            C[0, 1] = 0f;
            C[1, 0] = 0f;
            C[1, 1] = 0f;
            X[0] = 0f;
            X[1] = 0f;

            for (i = 0; i < nPts; i++)
            {
                C[0, 0] += V2Dot(A[i, 0], A[i, 0]);
                C[0, 1] += V2Dot(A[i, 0], A[i, 1]);
                /*                  C[1][0] += V2Dot(&A[i][0], &A[i][9]);*/
                C[1, 0] = C[0, 1];
                C[1, 1] += V2Dot(A[i, 1], A[i, 1]);

                tmp = ((Vector2)d[first + i] -
                    (
                      ((Vector2)d[first] * B0(uPrime[i])) +
                        (
                            ((Vector2)d[first] * B1(uPrime[i])) +
                                    (
                                    ((Vector2)d[last] * B2(uPrime[i])) +
                                        ((Vector2)d[last] * B3(uPrime[i]))))));


                X[0] += V2Dot(A[i, 0], tmp);
                X[1] += V2Dot(A[i, 1], tmp);
            }

            /* Compute the determinants of C and X  */
            det_C0_C1 = C[0, 0] * C[1, 1] - C[1, 0] * C[0, 1];
            det_C0_X = C[0, 0] * X[1] - C[1, 0] * X[0];
            det_X_C1 = X[0] * C[1, 1] - X[1] * C[0, 1];

            /* Finally, derive alpha values */
            alpha_l = (det_C0_C1 == 0) ? 0f : det_X_C1 / det_C0_C1;
            alpha_r = (det_C0_C1 == 0) ? 0f : det_C0_X / det_C0_C1;

            /* If alpha negative, use the Wu/Barsky heuristic (see text) */
            /* (if alpha is 0, you get coincident control points that lead to
             * divide by zero in any subsequent NewtonRaphsonRootFind() call. */
            float segLength = (d[first] - d[last]).magnitude;
            float epsilon = (float)(1.0e-6) * segLength;
            if (alpha_l < epsilon || alpha_r < epsilon)
            {
                /* fall back on standard (probably inaccurate) formula, and subdivide further if needed. */
                float dist = segLength / 3f;
                bezCurve[0] = d[first];
                bezCurve[3] = d[last];
                bezCurve[1] = (tHat1 * dist) + bezCurve[0];
                bezCurve[2] = (tHat2 * dist) + bezCurve[3];
                return (bezCurve);
            }

            /*  First and last control points of the Bezier curve are */
            /*  positioned exactly at the first and last data points */
            /*  Control points 1 and 2 are positioned an alpha distance out */
            /*  on the tangent vectors, left and right, respectively */
            bezCurve[0] = d[first];
            bezCurve[3] = d[last];
            bezCurve[1] = (tHat1 * alpha_l) + bezCurve[0];
            bezCurve[2] = (tHat2 * alpha_r) + bezCurve[3];
            return (bezCurve);
        }

        /*
         *  Reparameterize:
         *  Given set of points and their parameterization, try to find
         *   a better parameterization.
         *
         */
        float[] Reparameterize(Vector2[] d, int first, int last, float[] u, Vector2[] bezCurve)
        {
            int nPts = last - first + 1;
            int i;
            float[] uPrime = new float[nPts];      /*  New parameter values    */

            for (i = first; i <= last; i++)
            {
                uPrime[i - first] = NewtonRaphsonRootFind(bezCurve, d[i], u[i - first]);
            }
            return uPrime;
        }



        /*
         *  NewtonRaphsonRootFind :
         *  Use Newton-Raphson iteration to find better root.
         */
        float NewtonRaphsonRootFind(Vector2[] Q, Vector2 P, float u)
        {
            double numerator, denominator;
            Vector2[] Q1 = new Vector2[3], Q2 = new Vector2[2];   /*  Q' and Q''          */
            Vector2 Q_u, Q1_u, Q2_u; /*u evaluated at Q, Q', & Q''  */
            double uPrime;     /*  Improved u          */
            int i;

            /* Compute Q(u) */
            Q_u = BezierII(3, Q, u);

            /* Generate control vertices for Q' */
            for (i = 0; i <= 2; i++)
            {
                Q1[i].x = (Q[i + 1].x - Q[i].x) * 3f;
                Q1[i].y = (Q[i + 1].y - Q[i].y) * 3f;
            }

            /* Generate control vertices for Q'' */
            for (i = 0; i <= 1; i++)
            {
                Q2[i].x = (Q1[i + 1].x - Q1[i].x) * 2f;
                Q2[i].y = (Q1[i + 1].y - Q1[i].y) * 2f;
            }

            /* Compute Q'(u) and Q''(u) */
            Q1_u = BezierII(2, Q1, u);
            Q2_u = BezierII(1, Q2, u);

            /* Compute f(u)/f'(u) */
            numerator = (Q_u.x - P.x) * (Q1_u.x) + (Q_u.y - P.y) * (Q1_u.y);
            denominator = (Q1_u.x) * (Q1_u.x) + (Q1_u.y) * (Q1_u.y) +
                          (Q_u.x - P.x) * (Q2_u.x) + (Q_u.y - P.y) * (Q2_u.y);
            if (denominator == 0f) return u;

            /* u = u - f(u)/f'(u) */
            uPrime = u - (numerator / denominator);
            return (float)uPrime;
        }



        /*
         *  Bezier :
         *      Evaluate a Bezier curve at a particular parameter value
         * 
         */
        Vector2 BezierII(int degree, Vector2[] V, float t)
        {
            int i, j;
            Vector2 Q;          /* Point on curve at parameter t    */
            Vector2[] Vtemp;      /* Local copy of control points     */

            /* Copy array   */
            Vtemp = new Vector2[degree + 1];
            for (i = 0; i <= degree; i++)
            {
                Vtemp[i] = V[i];
            }

            /* Triangle computation */
            for (i = 1; i <= degree; i++)
            {
                for (j = 0; j <= degree - i; j++)
                {
                    Vtemp[j].x = (1f - t) * Vtemp[j].x + t * Vtemp[j + 1].x;
                    Vtemp[j].y = (1f - t) * Vtemp[j].y + t * Vtemp[j + 1].y;
                }
            }

            Q = Vtemp[0];
            return Q;
        }


        /*
         *  B0, B1, B2, B3 :
         *  Bezier multipliers
         */
        float B0(float u)
        {
            float tmp = 1f - u;
            return (tmp * tmp * tmp);
        }


        float B1(float u)
        {
            float tmp = 1f - u;
            return (3 * u * (tmp * tmp));
        }

        float B2(float u)
        {
            float tmp = 1f - u;
            return (3 * u * u * tmp);
        }

        float B3(float u)
        {
            return (u * u * u);
        }

        /*
         * ComputeLeftTangent, ComputeRightTangent, ComputeCenterTangent :
         *Approximate unit tangents at endpoints and "center" of digitized curve
         */
        Vector2 ComputeLeftTangent(Vector2[] d, int end)
        {
            Vector2 tHat1;
            tHat1 = d[end + 1] - d[end];
            tHat1.Normalize();
            return tHat1;
        }

        Vector2 ComputeRightTangent(Vector2[] d, int end)
        {
            Vector2 tHat2;
            tHat2 = d[end - 1] - d[end];
            tHat2.Normalize();
            return tHat2;
        }

        Vector2 ComputeCenterTangent(Vector2[] d, int center)
        {
            Vector2 V1, V2, tHatCenter = new Vector2();

            V1 = d[center - 1] - d[center];
            V2 = d[center] - d[center + 1];
            tHatCenter.x = (V1.x + V2.x) / 2f;
            tHatCenter.y = (V1.y + V2.y) / 2f;
            tHatCenter.Normalize();
            return tHatCenter;
        }


        /*
         *  ChordLengthParameterize :
         *  Assign parameter values to digitized points 
         *  using relative distances between points.
         */
        float[] ChordLengthParameterize(Vector2[] d, int first, int last)
        {
            int i;
            float[] u = new float[last - first + 1];           /*  Parameterization        */

            u[0] = 0f;
            for (i = first + 1; i <= last; i++)
            {
                u[i - first] = u[i - first - 1] + (d[i - 1] - d[i]).magnitude;
            }

            for (i = first + 1; i <= last; i++)
            {
                u[i - first] = u[i - first] / u[last - first];
            }

            return u;
        }




        /*
         *  ComputeMaxError :
         *  Find the maximum squared distance of digitized points
         *  to fitted curve.
        */
        double ComputeMaxError(Vector2[] d, int first, int last, Vector2[] bezCurve, float[] u, out int splitPoint)
        {
            int i;
            double maxDist;        /*  Maximum error       */
            double dist;       /*  Current error       */
            Vector2 P;          /*  Point on curve      */
            Vector2 v;          /*  Vector from point to curve  */

            splitPoint = (last - first + 1) / 2;
            maxDist = 0f;
            for (i = first + 1; i < last; i++)
            {
                P = BezierII(3, bezCurve, u[i - first]);
                v = P - d[i];
                dist = v.sqrMagnitude;
                if (dist >= maxDist)
                {
                    maxDist = dist;
                    splitPoint = i;
                }
            }
            return maxDist;
        }

        private float V2Dot(Vector2 a, Vector2 b)
        {
            return ((a.x * b.x) + (a.y * b.y));
        }

    }
}
