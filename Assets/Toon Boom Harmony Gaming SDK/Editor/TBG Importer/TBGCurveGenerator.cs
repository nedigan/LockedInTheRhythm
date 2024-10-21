#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace ToonBoom.TBGImporter
{
    public class TBGCurveGenerator
    {
        public bool Stepped;
        public int AnimationLength;
        public struct CurveInput
        {
            public string info;
            public string node;
            public string attribute;
            public double value;
            public List<TimedValuePoint> timedValuePoints;
        }
        public class Handle
        {
            public Handle(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            public double x;
            public double y;
        }
        public class BezierPoint
        {
            public int x;
            public double y;
            public bool constSeg;
            public Handle leftHandle;
            public Handle rightHandle;
        }

        private static double LinearInterpolation(double u, double a, double b)
        {
            return a + u * (b - a);
        }

        public class BezierCurve : List<BezierPoint>
        {
            public BezierCurve() { }
            public BezierCurve(IEnumerable<BezierPoint> points)
            {
                AddRange(points);
            }

            private double GetValueX(double u, BezierPoint left, BezierPoint right)
            {
                double a, b, c, d, e, f;
                a = LinearInterpolation(u, left.x, left.rightHandle.x);
                b = LinearInterpolation(u, left.rightHandle.x, right.leftHandle.x);
                c = LinearInterpolation(u, right.leftHandle.x, right.x);
                d = LinearInterpolation(u, a, b);
                e = LinearInterpolation(u, b, c);
                f = LinearInterpolation(u, d, e);
                return f;
            }

            private double GetValueY(double u, BezierPoint left, BezierPoint right)
            {
                double a, b, c, d, e, f;
                a = LinearInterpolation(u, left.y, left.rightHandle.y);
                b = LinearInterpolation(u, left.rightHandle.y, right.leftHandle.y);
                c = LinearInterpolation(u, right.leftHandle.y, right.y);
                d = LinearInterpolation(u, a, b);
                e = LinearInterpolation(u, b, c);
                f = LinearInterpolation(u, d, e);
                return f;
            }

            public double GetValue(int frame)
            {
                if (frame < this[0].x)
                {
                    return this[0].y;
                }
                else if (frame >= this[Count - 1].x)
                {
                    return this[Count - 1].y;
                }
                else
                {
                    for (int i = 0; i < Count - 1; i++)
                    {
                        if (frame >= this[i].x && frame < this[i + 1].x)
                        {
                            if (this[i].constSeg)
                            {
                                return this[i].y;
                            }
                            double u = FindU(frame, this[i], this[i + 1]);
                            return GetValueY(u, this[i], this[i + 1]);
                        }
                    }
                }
                throw new Exception("Could not find value for frame " + frame);
            }

            private double FindU(double time, BezierPoint left, BezierPoint right)
            {
                if (left.x == time)
                {
                    return 0.0f;
                }
                else if (right.x == time)
                {
                    return 1.0f;
                }
                else
                {
                    int i = 0;
                    double u, v;
                    double u1 = 0.0f;
                    double u2 = 1.0f;
                    do
                    {
                        u = 0.5f * (u1 + u2);
                        v = GetValueX(u, left, right);
                        if (v < time)
                        {
                            u1 = u;
                        }
                        else
                        {
                            u2 = u;
                        }
                    }
                    while ((Math.Abs(v - time) > 5e-10) && (++i < 52));
                    return u;
                }
            }
        }


        public BezierCurve FromTimedValues(CurveInput input, ValueMap valueMap)
        {
            try
            {
                Profiler.BeginSample("FromTimedValues");

                if (input.timedValuePoints == null)
                {
                    var value = valueMap(input.value);
                    return new BezierCurve {
                        new BezierPoint {
                            x = 0,
                            y = value,
                            constSeg = true,
                            leftHandle = new Handle(0, value),
                            rightHandle = new Handle(0, value),
                        },
                        new BezierPoint {
                            x = AnimationLength,
                            y = value,
                            constSeg = true,
                            leftHandle = new Handle(AnimationLength, value),
                            rightHandle = new Handle(AnimationLength, value),
                        },
                    };
                }
                return new BezierCurve(
                    input.timedValuePoints.Select(timedValuePoint => new BezierPoint
                    {
                        x = Mathf.RoundToInt((float)timedValuePoint.x) - 1,
                        y = valueMap(timedValuePoint.y),
                        constSeg = timedValuePoint.constSeg ?? false,
                        leftHandle = new Handle((timedValuePoint.lx ?? timedValuePoint.x) - 1, valueMap(timedValuePoint.ly ?? timedValuePoint.y)),
                        rightHandle = new Handle((timedValuePoint.rx ?? timedValuePoint.x) - 1, valueMap(timedValuePoint.ry ?? timedValuePoint.y)),
                    }));
            }
            catch (Exception e)
            {
                Debug.Log($"Could not set curve on clip for curve {input.info}");
                Debug.LogException(e);
                return null;
            }
            finally
            {
                Profiler.EndSample();
            }
        }
    }
}

#endif
