using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonFunc
{
    public static Vector3 Round(Vector3 v)
    {
        return new Vector3((float)Math.Round(v.x, 2), (float)Math.Round(v.y, 2), (float)Math.Round(v.z, 2));

    }
    public static float Round(float f)
    {
        return (float)Math.Round(f, 2);
    }
    public static float Cross(Vector3 a, Vector3 b)
    {
        return a.x * b.z - b.x * a.z;
    }
    const double eps = 1e-10;
    public static bool PointInVector2(Vector3 v1, Vector3 v2, Vector3 p)
    {
        if (p.Equals(v1) || p.Equals(v2))
        {
            return false;
        }
        float n1 = Cross(p - v2, p - v1);
        if (Mathf.Abs(n1) < eps)
        {
            if (Math.Min(v1.x, v2.x) - eps <= p.x && p.x - eps <= Math.Max(v1.x, v2.x))
            {
                if (Math.Min(v1.z, v2.z) - eps <= p.z && p.z - eps <= Math.Max(v1.z, v2.z))
                {
                    //printf("YES\n");
                    return true;
                }
            }
        }
        //Vector3 n1 = Vector3.Cross(v1 - v2, p - v1);
        //Vector3 n2 = Vector3.Cross(v1 - v2, p - v2);
        //if(n1.Equals(Vector3.zero) && n2.Equals (Vector3.zero))
        //{
        //    return true;
        //}
        ////bool r = Vector3.Dot(n1, n2) < 0;
        return false;
    }
    //public static bool IsCellPoint(Cell cell, Vector3 point)
    //{
    //    for (int i = 0; i < 3; i++)
    //    {
    //        if (cell.v[i].Equals(point))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public static void InsertNears(Cell a, Cell b, GtMsg.Cell a1, int aLineIndex, int bLineIndex)
    //{

    //    int index = b.index;
    //    var c = new Nears { Index = index, Distance = Vector3.Distance(a.MidPos, b.MidPos) };

    //    CommonFunc.Get2PointIndexBy1LineIndex(aLineIndex, ref c.PointIndex[0], ref c.PointIndex[1]);

    //    c.LineIndex = aLineIndex;
    //    c.OtherLineIndex = bLineIndex;

    //    a.nears[aLineIndex] = c;

    //    GtMsg.Nears c2 = a1.Nears[aLineIndex];
    //    c2.Index = index;
    //    c2.Enable = true;
    //    c2.Distance = c.Distance;
    //    c2.OtherLineIndex = c.OtherLineIndex;
    //    c2.LineIndex = c.LineIndex;
    //    c2.PointIndex.Add(c.PointIndex[0]);
    //    c2.PointIndex.Add(c.PointIndex[1]);


    //}
    //public static bool IsNearCell(Cell a, Cell b, ref int aLineIndex, ref int bLineIndex)
    //{
    //    for (int i = 0; i < 3; i++)
    //    {
    //        var ma = a.LineMidPos[i];

    //        for (int j = 0; j < 3; j++)
    //        {
    //            var mb = b.LineMidPos[j];

    //            if (ma.Equals(mb))
    //            {
    //                aLineIndex = i;
    //                bLineIndex = j;
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    //public static float GetLineMidDstance(Vector3 o, Vector3 v1, Vector3 v2)
    //{
    //    return Vector3.SqrMagnitude((o - v1) / 2 - (o - v2) / 2);
    //}
    //public static List<Cell> GetCellByPoint(List<Cell> list, Vector3 point)
    //{
    //    List<Cell> l = new List<Cell>();
    //    foreach (var c in list)
    //    {
    //        if (c.tobeRemove)
    //        {
    //            continue;
    //        }
    //        for (int i = 0; i < 3; i++)
    //        {
    //            if (point.Equals(c.v[i]))
    //            {
    //                l.Add(c);
    //            }
    //        }

    //    }
    //    return l;
    //}

    public static bool PointinTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        return SameSide(A, B, C, P) &&
            SameSide(B, C, A, P) &&
            SameSide(C, A, B, P);
    }
    public static bool PointinTriangleEX(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        return SameSideEX(A, B, C, P) &&
            SameSideEX(B, C, A, P) &&
            SameSideEX(C, A, B, P);
    }
    public static float GetMaxX(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Max(new float[3] { a.x, b.x, c.x });

    }
    public static float GetMaxZ(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Max(new float[3] { a.z, b.z, c.z });

    }
    public static float GetMinX(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Min(new float[3] { a.x, b.x, c.x });

    }
    public static float GetMinZ(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Min(new float[3] { a.z, b.z, c.z });

    }
    public static float GetAreaSize(Vector3 a, Vector3 b, Vector3 c)
    {
        var S = 0.5f * (a.x * b.z + b.x * c.z + c.x * a.z - a.x * c.z - b.x * a.z - c.x * b.z);
        return S;
    }
    //AB 和 CD 是否相交
    public static bool LinesCross(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        bool b1 = InSide(C, A, B, D);
        bool b2 = InSide(A, C, D, B);
        //bool b3 = PointInVector2(C, D, A);
        //bool b4 = PointInVector2(C, D, B);
        return b1 && b2 /*|| b3 || b4*/;
    }
    //判断PO是否在 V1O V2O 中间
    public static bool InSide(Vector3 O, Vector3 V1, Vector3 V2, Vector3 P)
    {
        Vector3 V1O = V1 - O;
        Vector3 V2O = V2 - O;
        Vector3 PO = P - O;

        Vector3 v1 = Vector3.Cross(PO, V1O);
        Vector3 v2 = Vector3.Cross(PO, V2O);

        return Vector3.Dot(v1, v2) <= 0;
    }
    //判断V1 O是否在PO O和V2 O的同一侧
    public static bool SameSide(Vector3 O, Vector3 V1, Vector3 V2, Vector3 P)
    {
        Vector3 V1O = V1 - O;
        Vector3 V2O = V2 - O;
        Vector3 PO = P - O;

        Vector3 v1 = Vector3.Cross(V1O, V2O);
        Vector3 v2 = Vector3.Cross(V1O, PO);//a × b = |a| |b| sin(θ) n

        // v1 and v2 should point to the same direction
        return Vector3.Dot(v1, v2) > 0;// v1.Dot(v2) >= 0;点乘的结果越大，表示两个向量越接近
    }
    public static bool SameSideEX(Vector3 O, Vector3 V1, Vector3 V2, Vector3 P)
    {
        Vector3 V1O = V1 - O;
        Vector3 V2O = V2 - O;
        Vector3 PO = P - O;

        Vector3 v1 = Vector3.Cross(V1O, V2O);
        Vector3 v2 = Vector3.Cross(V1O, PO);//a × b = |a| |b| sin(θ) n

        // v1 and v2 should point to the same direction
        return Vector3.Dot(v1, v2) >= 0;// v1.Dot(v2) >= 0;点乘的结果越大，表示两个向量越接近
    }
    //public static bool SameSideEX(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    //{
    //    Vector3 BA = B - A;
    //    Vector3 CA = C - A;
    //    Vector3 PA = P - A;

    //    Vector3 v1 = Vector3.Cross(BA, CA);
    //    Vector3 v2 = Vector3.Cross(BA, PA);//a × b = |a| |b| sin(θ) n

    //    // v1 and v2 should point to the same direction
    //    return Vector3.Dot(v1, v2) >= 0;// v1.Dot(v2) >= 0;点乘的结果越大，表示两个向量越接近
    ////}
    public static GtMsg.Vec3 GetVec3(Vector3 v3)
    {
        GtMsg.Vec3 v = new GtMsg.Vec3();
        v.X = v3.x;
        v.Y = v3.y;
        v.Z = v3.z;

        return v;
    }
    public static Vector3 GetVector3(GtMsg.Vec3 v3)
    {
        Vector3 v = new Vector3();
        v.x = v3.X;
        v.y = v3.Y;
        v.z = v3.Z;
        return v;
    }
    public static void Get2PointIndexBy1LineIndex(int lineIndex, ref int pointIndex1, ref int pointIndex2)
    {
        //pointIndex1 = -1;
        //pointIndex2 = -1;

        if (lineIndex == 0)
        {
            pointIndex1 = 0;
            pointIndex2 = 1;
        }
        else if (lineIndex == 1)
        {
            pointIndex1 = 1;
            pointIndex2 = 2;
        }
        else if (lineIndex == 2)
        {
            pointIndex1 = 2;
            pointIndex2 = 0;
        }
    }
    public static bool TryGetPointInLineWitchIsLengthFrom1Point(Vector3 v0, Vector3 v1, Vector3 v2, float mL,ref Vector3 v3)
    {
        float x0 = v0.x;
        float y0 = v0.z;
        float x1 = v1.x - x0;
        float y1 = v1.z - y0;
        float x2 = v2.x - x0;
        float y2 = v2.z - y0;

        var x2x1 = x2 - x1;
        if (x2x1 == 0)
        {
            x2x1 = Mathf.Max((x2 - x1), 0.0001f);
        }
        var a = (y2 - y1) / x2x1;
        var b = y2 - (y2 - y1) / x2x1 * x2;

        var t1 = (mL - b * b) / (1 + a * a) + Mathf.Pow(a * b / (1 + a * a), 2);
        if (t1 >= 0)
        {
            t1 = Mathf.Sqrt(t1);
            var t2 = a * b / (a * a + 1);

            bool f1enable = false;
            bool f2enable = false;
            var fx1 = t1 - t2;
            var fy1 = fx1 * a + b;

            var fx2 = -t1 - t2;
            var fy2 = fx2 * a + b;

            if (fx1 >= Mathf.Min(x1, x2) && fx1 <= Mathf.Max(x1, x2) && fy1 > Mathf.Min(y1, y2) && fy1 <= Mathf.Max(y1, y2))
            {
                f1enable = true;

            }
            if (fx2 >= Mathf.Min(x1, x2) && fx2 <= Mathf.Max(x1, x2) && fy2 > Mathf.Min(y1, y2) && fy2 <= Mathf.Max(y1, y2))
            {
                f2enable = true;
            }

            if (f1enable && f2enable)
            {
                if ((fx1 - x2) * (fx1 - x2) + (fy1 - y2) * (fy1 - y2) > (fx2 - x2) * (fx2 - x2) + (fy2 - y2) * (fy2 - y2))
                {
                    f1enable = false;

                }
                else
                {
                    f2enable = false;
                }
            }
            if (f1enable)
            {
                v3.x = fx1 + x0;
                v3.z = fy1 + y0;
                v3.y = 0;

                return true;
            }
            else if (f2enable)
            {
                v3.x = fx2 + x0;
                v3.z = fy2 + y0;
                v3.y = 0;
                return true;
            }
            return false;
        }
        return false;
    }

    public static void Get1PintIndexBy2LineIndex(int aLineIndex, int bLineIndex, ref int pointIndex)
    {
        if (aLineIndex == 0 && bLineIndex == 2 || aLineIndex == 2 && bLineIndex == 0)
        {
            pointIndex = 0;
        }
        else if (aLineIndex == 0 && bLineIndex == 1 || aLineIndex == 1 && bLineIndex == 0)
        {
            pointIndex = 1;
        }
        else if (aLineIndex == 1 && bLineIndex == 2 || aLineIndex == 2 && bLineIndex == 1)
        {
            pointIndex = 2;
        }
        else
        {
            Debug.Log("bbb");
        }
    }
    public static void QuadranglePoint(Vector3 AttackerPoint, Vector3 VictimPoint, float Width, float Length, ref Vector3 A, ref Vector3 B, ref Vector3 C, ref Vector3 D)
    {
        Vector3 line1 = VictimPoint - AttackerPoint;
        Vector3 farMid = line1.normalized * Length;

        Vector3 n = Vector3.Cross(farMid, new Vector3(0, 1, 0)).normalized * Width;
        A = AttackerPoint + farMid - n;
        D = AttackerPoint + farMid + n;
        C = AttackerPoint + n;
        B = AttackerPoint - n;
    }
    public static bool PointinQuadrangleEX(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 P)
    {
        bool b1 = SameSideEX(A, B, D, P);
        bool b2 = SameSideEX(D, A, C, P);
        bool b3 = SameSideEX(C, D, B, P);
        bool b4 = SameSideEX(B, C, A, P);
        return
         b1 && b2 && b3 && b4;
    }
}