/************************************************
 * 文件名:
 * 描述:网格导航
 * 创建人:tinyult
 * 创建日期：20160926
 * http://blog.csdn.net/huutu/article/details/52672505
 * ************************************************/

using UnityEngine;
using System.Collections;
//using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine.AI;
using System;

public class Nears
{
    public bool enable = false;//是否有效
    public int Index;//网格索引
    public float Distance;//与相邻网格的距离
    public int[] PointIndex = new int[2];//相邻的点索引
    public int LineIndex;
    public int OtherLineIndex;//相邻cell的相邻LineIndex
    //public Vector3 LineMidPos = new Vector3();
    //public int NotNearPointIndex = -1;//不相邻的点
}
public class PosData
{
    public Vector3 Pos;
    public List<int> CellIndexs = new List<int>();
}
public class Cell
{
    public int index = -1;//网格索引
    public Vector3[] v = new Vector3[3];//网格点
    public int[] vIndex = new int[3];
    public Vector3[][] vNormal = new Vector3[3][];
    public Vector3 MidPos = new Vector3();//中心点
    public Vector3[] Normalize = new Vector3[3];
    public Vector3[] LineMidPos = new Vector3[3];//边中心点
    public float[] LineMidSqrDistance = new float[3];//边中心点
    public Nears[] nears = new Nears[3];//相邻网格

    public bool tobeRemove = false;

    public float costToSource = 0;//与源的距离
    public float fValue = 0;//消耗值
    public Cell parent = null;//父节点
    public int InLineIndex = -1;

    public void reset()
    {
       
        costToSource = 0;
        fValue = 0;
        parent = null;
        InLineIndex = -1;
    }
}
public class ExportNavMesh
{

   
    static public List<Cell> list;
    static public List<PosData> posList;
    static public List<Cell> testList = new List<Cell>();


    //  [MenuItem("NavMesh/test")]
    static void test()
    {
        bool b = CommonFunc.LinesCross(new Vector3(0, 0, 0), new Vector3(0.1f, 0, 0.1f), new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        Debug.Log(b);
    }
    //[MenuItem("NavMesh/Export2")]
    static void Export2()
    {
        Debug.Log("ExportNavMesh");

        NavMeshTriangulation tmpNavMeshTriangulation = NavMesh.CalculateTriangulation();

        //新建文件
        string tmpPath = Application.dataPath + "/" + SceneManager.GetActiveScene().name + ".txt";
        StreamWriter tmpStreamWriter = new StreamWriter(tmpPath);

        //顶点
        for (int i = 0; i < tmpNavMeshTriangulation.vertices.Length; i++)
        {
            tmpStreamWriter.WriteLine("v  " + tmpNavMeshTriangulation.vertices[i].x + " " + tmpNavMeshTriangulation.vertices[i].y + " " + tmpNavMeshTriangulation.vertices[i].z);
        }

        tmpStreamWriter.WriteLine("g pPlane1");

        //索引
        for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
        {
            tmpStreamWriter.WriteLine("f " + (tmpNavMeshTriangulation.indices[i] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 1] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 2] + 1));
            i = i + 3;
        }

        tmpStreamWriter.Flush();
        tmpStreamWriter.Close();

        Debug.Log("ExportNavMesh Success");
    }

    // [MenuItem("NavMesh/Export")]
    static public void Export(string fileName)
    {

        Debug.Log("ExportNavMesh");

        UnityEngine.AI.NavMeshTriangulation tmpNavMeshTriangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
        GtMsg.NavData navData = new GtMsg.NavData();


        list = new List<Cell>();
        posList = new List<PosData>();
        //初始化每个cell的点
        for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
        {
            var cell = new Cell();
            var v0 = tmpNavMeshTriangulation.indices[i];
            var v1 = tmpNavMeshTriangulation.indices[i + 1];
            var v2 = tmpNavMeshTriangulation.indices[i + 2];

            cell.v[0] = CommonFunc.Round(tmpNavMeshTriangulation.vertices[v0]);
            cell.v[1] = CommonFunc.Round(tmpNavMeshTriangulation.vertices[v1]);
            cell.v[2] = CommonFunc.Round(tmpNavMeshTriangulation.vertices[v2]);
            cell.v[0].y = 0;
            cell.v[1].y = 0;
            cell.v[2].y = 0;


            //cell.vNormal[0] = new Vector3[2];
            //cell.vNormal[0][0] = Vector3.Normalize(cell.v[0] - cell.v[1]);
            //cell.vNormal[0][1] = Vector3.Normalize(cell.v[0] - cell.v[2]);

            //cell.vNormal[1][0] = Vector3.Normalize(cell.v[1] - cell.v[0]);
            //cell.vNormal[1][1] = Vector3.Normalize(cell.v[1] - cell.v[2]);

            //cell.vNormal[2][0] = Vector3.Normalize(cell.v[2] - cell.v[1]);
            //cell.vNormal[2][1] = Vector3.Normalize(cell.v[2] - cell.v[0]);

            //cell.MaxX = GetMaxX(cell.v[0], cell.v[1], cell.v[2]);
            //cell.MaxY = GetMaxZ(cell.v[0], cell.v[1], cell.v[2]);
            //cell.MinX = GetMinX(cell.v[0], cell.v[1], cell.v[2]);
            //cell.MinY = GetMinZ(cell.v[0], cell.v[1], cell.v[2]);
            //cell.AreaSize = GetAreaSize(cell.v[0], cell.v[1], cell.v[2]);
            //cell.index = list.Count;
            list.Add(cell);

            i = i + 3;
        }

        //处理不合格的cell
        int Count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var a = list[i];
            if (a.tobeRemove)
            {
                continue;
            }
            for (int j = 0; j < list.Count; j++)
            {
                var b = list[j];
                if (b.tobeRemove)
                {
                    continue;
                }
                if (a != b)
                {
                    var a0 = a.v[0];
                    var a1 = a.v[1];
                    var a2 = a.v[2];

                    var b0 = b.v[0];
                    var b1 = b.v[1];
                    var b2 = b.v[2];

                    int index = -1;
                    int vx1 = -1;
                    int vx2 = -1;
                    if (CommonFunc.PointInVector2(a0, a1, b0))
                    {
                        index = 0;
                        vx1 = 0;
                        vx2 = 1;

                    }
                    else if (CommonFunc.PointInVector2(a1, a2, b0))
                    {
                        index = 0;
                        vx1 = 1;
                        vx2 = 2;
                    }
                    else if (CommonFunc.PointInVector2(a2, a0, b0))
                    {
                        index = 0;
                        vx1 = 2;
                        vx2 = 0;
                    }
                    else if (CommonFunc.PointInVector2(a0, a1, b1))
                    {
                        index = 1;
                        vx1 = 0;
                        vx2 = 1;
                    }
                    else if (CommonFunc.PointInVector2(a1, a2, b1))
                    {
                        index = 1;
                        vx1 = 1;
                        vx2 = 2;
                    }
                    else if (CommonFunc.PointInVector2(a2, a0, b1))
                    {
                        index = 1;
                        vx1 = 2;
                        vx2 = 0;
                    }
                    else if (CommonFunc.PointInVector2(a0, a1, b2))
                    {
                        index = 2;
                        vx1 = 0;
                        vx2 = 1;
                    }
                    else if (CommonFunc.PointInVector2(a1, a2, b2))
                    {
                        index = 2;
                        vx1 = 1;
                        vx2 = 2;
                    }
                    else if (CommonFunc.PointInVector2(a2, a0, b2))
                    {
                        index = 2;
                        vx1 = 2;
                        vx2 = 0;
                    }

                    if (index != -1)
                    {
                        testList = GetCellByPoint(b.v[index]);

                        var length1 = Vector3.SqrMagnitude(b.v[index] - a.v[vx1]);
                        var length2 = Vector3.SqrMagnitude(b.v[index] - a.v[vx2]);

                        Vector3 pp;

                        if (length1 > length2)
                        {
                            pp = a.v[vx2];
                        }
                        else
                        {
                            pp = a.v[vx1];
                        }

                        foreach (var tt in testList)
                        {
                            bool bb = IsCellPoint(tt, pp);
                            //
                            if (bb)
                            {
                                //删除小三角形
                                //list.Remove(tt);
                                tt.tobeRemove = true;
                                Count++;
                            }
                            else
                            {
                                //扩张大三角形
                                for (int k = 0; k < 3; k++)
                                {
                                    if (tt.v[k].Equals(b.v[index]))
                                    {
                                        tt.v[k] = pp;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //设置cell的属性
        Debug.Log(Count);
        List<Cell> listtemp = new List<Cell>();
        for (int k = 0; k < list.Count; k++)
        {
            var cell = list[k];
            if (!cell.tobeRemove)
            {

                for (int i = 0; i < 3; i++)
                {
                    int posIndex = -1;
                    for (int j = 0; j < posList.Count; j++)
                    {
                        var v = posList[j];
                        if (v.Pos.Equals(cell.v[i]))
                        {
                            posIndex = j;
                            break;
                        }

                    }
                    if (posIndex == -1)
                    {
                        posList.Add(new PosData { Pos = cell.v[i] });
                        navData.PosList.Add(new GtMsg.PosData { Pos = CommonFunc.GetVec3(cell.v[i]) });
                        posIndex = posList.Count - 1;
                    }
                    cell.vIndex[i] = posIndex;
                    posList[posIndex].CellIndexs.Add(k);
                    navData.PosList[posIndex].CellIndexs.Add(k);
                }


                cell.LineMidPos[0] = CommonFunc.Round((cell.v[0] + cell.v[1]) / 2);
                cell.LineMidPos[1] = CommonFunc.Round((cell.v[1] + cell.v[2]) / 2);
                cell.LineMidPos[2] = CommonFunc.Round((cell.v[2] + cell.v[0]) / 2);

                cell.Normalize[0] = Vector3.Normalize(cell.v[1] - cell.v[0]);
                cell.Normalize[1] = Vector3.Normalize(cell.v[2] - cell.v[1]);
                cell.Normalize[2] = Vector3.Normalize(cell.v[0] - cell.v[2]);


                cell.LineMidSqrDistance[0] = CommonFunc.Round(GetLineMidDstance(cell.v[0], cell.v[1], cell.v[2]));
                cell.LineMidSqrDistance[1] = CommonFunc.Round(GetLineMidDstance(cell.v[1], cell.v[0], cell.v[2]));
                cell.LineMidSqrDistance[2] = CommonFunc.Round(GetLineMidDstance(cell.v[2], cell.v[0], cell.v[1]));
                cell.MidPos = (cell.v[0] + cell.v[1] + cell.v[2]) / 3;

                listtemp.Add(cell);
                GtMsg.Cell msg_cell = new GtMsg.Cell();
                msg_cell.V.Add(CommonFunc.GetVec3(cell.v[0]));
                msg_cell.V.Add(CommonFunc.GetVec3(cell.v[1]));
                msg_cell.V.Add(CommonFunc.GetVec3(cell.v[2]));
                msg_cell.VIndex.Add(cell.vIndex[0]);
                msg_cell.VIndex.Add(cell.vIndex[1]);
                msg_cell.VIndex.Add(cell.vIndex[2]);


                msg_cell.MidPos = CommonFunc.GetVec3(cell.MidPos);
                msg_cell.Index = navData.Celllist.Count;
                msg_cell.LineMidSqrDistance.Add(cell.LineMidSqrDistance[0]);
                msg_cell.LineMidSqrDistance.Add(cell.LineMidSqrDistance[1]);
                msg_cell.LineMidSqrDistance.Add(cell.LineMidSqrDistance[2]);
                msg_cell.LineMidPos.Add(CommonFunc.GetVec3(cell.LineMidPos[0]));
                msg_cell.LineMidPos.Add(CommonFunc.GetVec3(cell.LineMidPos[1]));
                msg_cell.LineMidPos.Add(CommonFunc.GetVec3(cell.LineMidPos[2]));
                msg_cell.Normalize.Add(CommonFunc.GetVec3(cell.Normalize[0]));
                msg_cell.Normalize.Add(CommonFunc.GetVec3(cell.Normalize[1]));
                msg_cell.Normalize.Add(CommonFunc.GetVec3(cell.Normalize[2]));
                for (int i = 0; i < 3; i++)
                {
                    GtMsg.Nears n = new GtMsg.Nears();
                    n.Enable = false;
                    msg_cell.Nears.Add(n);
                }

                cell.index = msg_cell.Index;
                navData.Celllist.Add(msg_cell);
            }

        }
        list = listtemp;

        //设置相邻cell
        for (int i = 0; i < list.Count; i++)
        {
            var a = list[i];
            for (int j = 0; j < list.Count; j++)
            {
                var b = list[j];
                if (a != b)
                {
                    int aLineIndex = -1;
                    int bLineIndex = -1;

                    if (IsNearCell(a, b, ref aLineIndex, ref bLineIndex))
                    {
                        var a2 = navData.Celllist[i];
                        InsertNears(a, b, a2, aLineIndex, bLineIndex);
                    }
                }
            }
        }

        string resourcePath = Application.dataPath + "/Resources/File/";
        if (!Directory.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }


        using (FileStream output = File.Create(resourcePath + fileName + ".bytes"))
        {
            navData.WriteTo(output);
        }
       


        Debug.Log("ExportNavMesh Success");
    }


    static bool IsCellPoint(Cell cell, Vector3 point)
    {
        for (int i = 0; i < 3; i++)
        {
            if (cell.v[i].Equals(point))
            {
                return true;
            }
        }
        return false;
    }
    static List<Cell> GetCellByPoint(Vector3 point)
    {
        List<Cell> l = new List<Cell>();
        foreach (var c in list)
        {
            if (c.tobeRemove)
            {
                continue;
            }
            for (int i = 0; i < 3; i++)
            {
                if (point.Equals(c.v[i]))
                {
                    l.Add(c);
                }
            }

        }
        return l;
    }
    static public float GetLineMidDstance(Vector3 o, Vector3 v1, Vector3 v2)
    {
        return Vector3.SqrMagnitude((o - v1) / 2 - (o - v2) / 2);
    }



    static void InsertNears(Cell a, Cell b, GtMsg.Cell a1, int aLineIndex, int bLineIndex)
    {

        int index = b.index;
        var c = new Nears { Index = index, Distance = Vector3.Distance(a.MidPos, b.MidPos) };

        CommonFunc.Get2PointIndexBy1LineIndex(aLineIndex, ref c.PointIndex[0], ref c.PointIndex[1]);

        c.LineIndex = aLineIndex;
        c.OtherLineIndex = bLineIndex;

        a.nears[aLineIndex] = c;

        GtMsg.Nears c2 = a1.Nears[aLineIndex];
        c2.Index = index;
        c2.Enable = true;
        c2.Distance = c.Distance;
        c2.OtherLineIndex = c.OtherLineIndex;
        c2.LineIndex = c.LineIndex;
        c2.PointIndex.Add(c.PointIndex[0]);
        c2.PointIndex.Add(c.PointIndex[1]);


    }
    static bool IsNearCell(Cell a, Cell b, ref int aLineIndex, ref int bLineIndex)
    {
        for (int i = 0; i < 3; i++)
        {
            var ma = a.LineMidPos[i];

            for (int j = 0; j < 3; j++)
            {
                var mb = b.LineMidPos[j];

                if (ma.Equals(mb))
                {
                    aLineIndex = i;
                    bLineIndex = j;
                    return true;
                }
            }
        }
        return false;
    }
}
