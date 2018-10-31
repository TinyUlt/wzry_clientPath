using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Nav  {
    public class NavPoint
    {
        public int Index;//列表中的索引
        public Vector3 v;//该点坐标
        public int ConnectIndex = -1;//最后连接的另一边的导航点
        // public List<int> ConnectIndexs = new List<int>();//连接的另一边的导航点
    }

    public Transform StartPos;//开始位置
    public Transform EndPos;//结束位置
    public Transform FlyPos;

    public List<Cell> list = new List<Cell>();

    public List<NavPoint> firstPoints = new List<NavPoint>();//第一导航点列表
    public List<NavPoint> SecondPoints = new List<NavPoint>();//第二导航点列表 

    public List<Cell> openList = new List<Cell>();//开发列表
    public List<Cell> closeList = new List<Cell>();//关闭列表
    public List<Cell> CellList = new List<Cell>();//路径网格
    public List<Vector3> FinalPath = new List<Vector3>();//最终路径点


    int firstI = 0;//第一导航索引
    int secondI = 0;//第二导航索引
    int firstStep = 0;//第一导航跳跃间隔
    int secondStep = 0;//第二导航跳跃间隔
    int firstUntil = 0;//第一导航最大到达索引
    int secondUntil = 0;//第二导航最大到达索引
    bool isFirstQ = true;
    int last = -1;


    public Queue<int> CellIndexQueue = new Queue<int>();
    public List<int> CellIndexList = new List<int>();


    public void Init(string fileName)
    {
        TextAsset asset = Resources.Load("File/"+fileName) as TextAsset;
        //GameMgr.Instance.m_AssetsLoad.LoadConfig("OriginPathFile_Client", OnLoad);
        var bytes = asset.bytes;
        GtMsg.NavData nav = GtMsg.NavData.Parser.ParseFrom(bytes);

        list.Clear();
        foreach (var c in nav.Celllist)
        {
            Cell cell = new Cell();
            cell.index = c.Index;
            cell.v[0] = CommonFunc.GetVector3(c.V[0]);
            cell.v[1] = CommonFunc.GetVector3(c.V[1]);
            cell.v[2] = CommonFunc.GetVector3(c.V[2]);
            cell.MidPos = CommonFunc.GetVector3(c.MidPos);
            cell.LineMidSqrDistance[0] = c.LineMidSqrDistance[0];
            cell.LineMidSqrDistance[1] = c.LineMidSqrDistance[1];
            cell.LineMidSqrDistance[2] = c.LineMidSqrDistance[2];
            cell.LineMidPos[0] = CommonFunc.GetVector3(c.LineMidPos[0]);
            cell.LineMidPos[1] = CommonFunc.GetVector3(c.LineMidPos[1]);
            cell.LineMidPos[2] = CommonFunc.GetVector3(c.LineMidPos[2]);
            cell.Normalize[0] = CommonFunc.GetVector3(c.Normalize[0]);
            cell.Normalize[1] = CommonFunc.GetVector3(c.Normalize[1]);
            cell.Normalize[2] = CommonFunc.GetVector3(c.Normalize[2]);
            for (var i = 0; i < 3; i++)
            {
                var n = c.Nears[i];
                if (n.Enable)
                {
                    Nears near = new Nears();
                    near.Index = n.Index;
                    near.Distance = n.Distance;
                    near.PointIndex[0] = n.PointIndex[0];
                    near.PointIndex[1] = n.PointIndex[1];
                    near.OtherLineIndex = n.OtherLineIndex;
                    near.LineIndex = n.LineIndex;
                    cell.nears[i] = near;
                }
                else
                {
                    cell.nears[i] = null;
                }
            }
            list.Add(cell);
        }

    }
    //一条线穿越哪两个2个cell
    public Cell GetCellWitchCross(Cell startCell, Cell V1Cell, Vector3 V1, Vector3 V2, ref Vector3 normal, int lastLineIndex, float speed, int obsCount)
    {
        //Debug.Log("GetCellWitchCross");
        for (int i = 0; i < 3; i++)
        {
            if(lastLineIndex == i)
            {
                continue;
            }
            int i1 = -1;
            int i2 = -1;
            CommonFunc.Get2PointIndexBy1LineIndex(i, ref i1, ref i2);

            Vector3 v1 = startCell.v[i1];
            Vector3 v2 = startCell.v[i2];

            //Debug.Log("i:" + i);
            //Debug.Log("v1" + v1.ToString("f4"));
            //Debug.Log("v2" + v2.ToString("f4"));

            //Debug.Log("V1" + V1.ToString("f4"));
            //Debug.Log("V2" + V2.ToString("f4"));

            if (CommonFunc.LinesCross(v1, v2, V1, V2))
            {
                if (startCell.nears[i] != null)
                {
                    Cell cell = list[startCell.nears[i].Index];
                   // Debug.Log("CellIndex:" + cell.index);
                    if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], V2))
                    {
                        //Debug.Log("IC");
                        return cell;
                    }
                    else
                    {
                        //Debug.Log("OC");
                        return GetCellWitchCross(cell, V1Cell, V1, V2,ref normal, startCell.nears[i].OtherLineIndex, speed, obsCount);
                    }
                }
                else
                {
                    if (obsCount >= 1)
                    {
                        normal = Vector3.zero;

                        return V1Cell;//GetCellWitchCross(V1Cell, V1Cell, V1, V2, ref normal, -1, speed, obsCount);
                    }
                    obsCount++;
                    Vector3 n = startCell.Normalize[i];// Vector3.Normalize(v2 - v1);
                    //锐角
                    if (Vector3.Dot(n, normal) > 0)
                    {
                        normal = n;
                    }
                    else//钝角
                    {
                        normal = -n;
                    }
                    normal *= speed;
                    V2 = V1 + normal;
                    //Debug.Log("CN");
                    return GetCellWitchCross(V1Cell, V1Cell, V1, V2, ref normal, -1, speed, obsCount);
                    //todo
                    //normal = Vector3.zero;
                }
                //break;
                
            }
            //Debug.Log("CCCCCC");

            //else
            //{
            //    return OldIndex;
            //}
        }
        //Debug.Log("SC");
        return startCell;
    }
    //获取网格索引和方向单位向量
    /**
     * oldIndex 之前保存的网格索引
     * pos 当前位置
     * normal 方向单位向量
     * **/
    public int GetCellIndexAndSetMoveVector(int oldIndex, Vector3 pos, ref Vector3 normal, float speed)
    {
        //Debug.Log("B");
        if (oldIndex < 0 || oldIndex >= list.Count)
        {
            return GetCellIndex(pos);
        }
        Cell cell = list[oldIndex];
        if (!CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], pos))
        {
            normal = Vector3.zero;
            return oldIndex;
        }
        if (normal == Vector3.zero)
        {
            return oldIndex;
        }

        Cell finalCell = GetCellWitchCross(cell, cell, pos, pos + normal, ref normal, -1, speed, 0);

        return finalCell.index;
    }
    public void GetLineEnd(Cell startCell, Vector3 startPos, Vector3 endPos, ref Cell finalCell, ref Vector3 finalPos, int lastLineIndex)
    {
        for (int i = 0; i < 3; i++)
        {
            if (lastLineIndex == i)
            {
                continue;
            }
            int i1 = -1;
            int i2 = -1;
            CommonFunc.Get2PointIndexBy1LineIndex(i, ref i1, ref i2);

            Vector3 v1 = startCell.v[i1];
            Vector3 v2 = startCell.v[i2];

            //Debug.Log("i:" + i);
            //Debug.Log("v1" + v1.ToString("f4"));
            //Debug.Log("v2" + v2.ToString("f4"));

            //Debug.Log("V1" + V1.ToString("f4"));
            //Debug.Log("V2" + V2.ToString("f4"));

            if (CommonFunc.LinesCross(v1, v2, startPos, endPos))
            {
                if (startCell.nears[i] != null)
                {
                    Cell cell = list[startCell.nears[i].Index];
                    // Debug.Log("CellIndex:" + cell.index);
                    if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], endPos))
                    {
                        //Debug.Log("IC");
                        finalCell = cell;
                        finalPos = endPos;
                        return;
                    }
                    else
                    {
                        //Debug.Log("OC");
                        GetLineEnd(cell, startPos,  endPos, ref finalCell, ref finalPos, startCell.nears[i].OtherLineIndex);
                        return;
                    }
                }
                else
                {
                    finalCell = startCell;
                    finalPos = GetPointThatTwoLineCross(v1, v2, startPos, endPos);
                    return;
                }
            }
        }
        //Debug.Log("SC");
        finalCell = startCell;
        finalPos = endPos;
        return;
    }
    public void CFXZ(int startCellIndex, Vector3 startPos, Vector3 endPos, ref Vector3 finalPos, ref int finalCellIndex)
    {
        if (startCellIndex < 0 || startCellIndex >= list.Count)
        {
            Debug.Log("CFXZ 索引越界"); ;
        }
        Cell startCell = list[startCellIndex];
        if (!CommonFunc.PointinTriangleEX(startCell.v[0], startCell.v[1], startCell.v[2], startPos))
        {
            Debug.Log("初始位置不在该索引的Cell中"); ;
        }
        
        Cell finalCell = null;
        GetLineEnd(startCell, startPos, endPos, ref finalCell, ref finalPos, -1);
        finalCellIndex = finalCell.index;
    }
    //两线段相交于哪个点
    //y1=a1x1+b1
    //y2=a2x2+b2
    //a1x1+b1=a2x2+b2
    //a1x+b1=a2x+b2
    //x=(b2-b1)/(a1-a2)
    //y=a1x+b1
    public Vector3 GetPointThatTwoLineCross(Vector3 line1v1, Vector3 line1v2, Vector3 line2v1, Vector3 line2v2)
    {
        float a1 = 0;
        float b1 = 0;
        float a2 = 0;
        float b2 = 0;

        float x = 0;
        float z = 0;

        bool line1 = GetLineFunc(line1v1, line1v2, ref a1, ref b1);
        bool line2 = GetLineFunc(line2v1, line2v2, ref a2, ref b2);


        if (!line1)
        {
            x = line1v1.x;
            z = a2 * x + b2;

        }
        else if(!line2)
        {
            x = line2v1.x;
            z = a1 * x + b1;
        }
        else
        {
            x = (b2 - b1) / (a1 - a2);
            z = a1 * x + b1;
        }

        return new Vector3(x, 0, z);
    }
    //得到线性函数的a和b
    public bool GetLineFunc(Vector3 v1,Vector3 v2, ref float a, ref float b)
    {
        if((v1.x - v2.x) == 0)
        {
            return false;
        }
        a = (v1.z - v2.z) / (v1.x - v2.x);
        b = v1.z - a * v1.x;
        return true;
    }
    private int GetCellIndex(int OldIndex, Vector3 pos)
    {
        CellIndexQueue.Clear();
        CellIndexList.Clear();
        if(OldIndex<0 || OldIndex >= list.Count)
        {
            return GetCellIndex(pos);
        }
        //CellIndexQueue.Enqueue(OldIndex);
        Cell cell = list[OldIndex];
        CellIndexList.Add(OldIndex);
        while (true)
        {
            //change ex
            if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], pos))
            {
                return cell.index;
            }
            else
            {
                for(int i = 0; i < 3; i++)
                {
                    //int c1 = cell.nears[i].Index;
                    if (cell.nears[i]!=null)
                    {
                        int c1 = cell.nears[i].Index;
                        if (!CellIndexList.Contains(c1))
                        {
                            CellIndexQueue.Enqueue(c1);
                            CellIndexList.Add(c1);
                        }
                       
                    }
                }
                
            }
            if(CellIndexQueue.Count == 0)
            {
                break;
            }
            cell = list[CellIndexQueue.Dequeue()];
        }
      
        return GetCellIndex(pos);
    }
    
    //根据位置获取所以在的网格索引
    public int GetCellIndex(Vector3 pos)
    {
        foreach (var cell in list)
        {
            //change ex
            if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], pos))
            {
                return cell.index;
            }
        }
        return -1;
    }

    //服务器发来拐点列表时，设置一些参数
    /**
     * nowFrame 主逻辑当前帧 
     * speed 怪物速度， 当前服务器的速度是 2/60.0f
     * PosList 服务器发来的路径列表
     * pathList 客户端要保存的路径点
     * frameList 客户端要保存帧列表
     * pathIndex 客户端要保存的索引
     **/
    public void SetPath(int nowFrame, float speed, GtMsg.S_MovePosList PosList, ref List<Vector3> pathList, ref List<int> frameList, ref int pathIndex)
    {
        frameList.Clear();
        pathList.Clear();
        for (int i = 0; i < PosList.List.Count; i++)
        {
            var v3 = PosList.List[i];
            pathList.Add(new Vector3(v3.X, v3.Y, v3.Z));
            if (i == 0)
            {
                frameList.Add(nowFrame);
            }
            else
            {
                float distance = Vector3.Distance(pathList[i], pathList[i - 1]);
                int frame = (int)(distance / speed);
                frameList.Add(frameList[i - 1] + frame);
            }
        }
        pathIndex = 0;
    }


    //根据当前帧,获取物体位置, 每帧调用
    /**
     * 有点返回true， 无点返回false
     * nowFrame 主逻辑当前帧 
     * pathList 客户端之前保存的路径点
     * frameList 客户端之前保存帧列表
     * pathIndex  客户端之前保存的索引
     * pos 获得的路径点， 用来设置怪物的位置
     * **/
    public bool TryGetPosByFrame(int nowFrame, ref List<Vector3> pathList, ref List<int> frameList, ref int pathIndex, ref Vector3 pos)
    {
        if(frameList.Count == 0)
        {
            return false;
        }
        if (nowFrame > frameList[frameList.Count-1])
        {
            return false;
        }
        int size = frameList.Count;
        for (int i = pathIndex; i < size; i++)
        {
            if (frameList[i] > nowFrame)
            {
                pathIndex = i;
                break;
            }
        }

        Vector3 v0 = pathList[pathIndex - 1];
        Vector3 v2 = pathList[pathIndex];
        Vector3 v3 = v2 - v0;

        int f0 = frameList[pathIndex - 1];
        float f1 = nowFrame - f0;
        float f2 = frameList[pathIndex] - f0;

        float fp = f1 / f2;

        pos = v3 * fp + v0;
        return true;
    }
    //[ContextMenu("flyStart")]
//    public void flyStart()
//    {
//        FlyPos.position = StartPos.position;
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.update += OnUpdate;
//#endif
//    }
//    //[ContextMenu("flyStop")]
//    public void flyStop()
//    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.update -= OnUpdate;
//#endif
//    }
    //void OnUpdate()
    //{
    //    Vector3 v = EndPos.position - FlyPos.position;
    //    float speed = 1f;
    //    FlyPos.position = FlyPos.position + Normalized(v) * speed;
    //    Debug.Log(FlyPos.position);
    //}
    Vector3 Normalized(Vector3 v)
    {
        float len = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        if (len == 0) len = 1;
        len = 1 / len;
        return new Vector3(v.x * len, v.y * len, v.z * len);
    }
  
    public Cell GetCellInCellListByPos(Vector3 pos,  List<Cell> cellList)
    {
        foreach (var cell in cellList)
        {
            if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], pos))
            {
                return cell;
            }
        }
        return null;
    }
    public bool TryGetPath(Vector3 startPos, Vector3 endPos, int endCellIndex, List<Vector3> pathList, List<Cell> cellList)
    {
        if (endCellIndex < 0 || endCellIndex >= list.Count)
        {
            return false;
        }
        Cell  endCell = list[endCellIndex];
        if (!CommonFunc.PointinTriangleEX(endCell.v[0], endCell.v[1], endCell.v[2], endPos))
        {
            return false;
        }

        Vector3 startPosTemp = startPos;
        Cell startCell = null;
        do
        {
            if (cellList.Count == 0)
            {
                var index = GetCellIndex(startPosTemp);
                if(index >= 0)
                {
                    startCell = list[index];
                }
            }
            if(startCell != null)
            {
                break;
            }

            startCell = GetCellInCellListByPos(startPosTemp,  cellList);
            if (startCell != null)
            {
                break;
            }

            startPosTemp = startPos;
            startPosTemp.x = startPos.x + 0.0001f;
            startCell = GetCellInCellListByPos(startPosTemp,  cellList);
            if (startCell != null)
            {
                break;
            }

            if (startCell != null)
            {
                break;
            }

            startPosTemp = startPos;
            startPosTemp.x = startPos.x - 0.0001f;
            startCell = GetCellInCellListByPos(startPosTemp, cellList);

            if (startCell != null)
            {
                break;
            }

            startPosTemp = startPos;
            startPosTemp.y = startPos.y + 0.0001f;
            startCell = GetCellInCellListByPos(startPosTemp,  cellList);

            if (startCell != null)
            {
                break;
            }

            startPosTemp = startPos;
            startPosTemp.y = startPos.y - 0.0001f;
            startCell = GetCellInCellListByPos(startPosTemp, cellList);

            if (startCell == null)
            {
                return false;
            }

        } while (false);
        //if (cellList.Count == 0)
        //{
        //    foreach (var cell in list)
        //    {
        //        if (startCell == null && CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], startPos))
        //        {
        //            startCell = cell;
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    foreach (var cell in cellList)
        //    {

        //        if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], startPos))
        //        {
        //            startCell = cell;
        //            break;
        //        }
        //    }
        //}
        //if (startCell == null)
        //{
        //    foreach (var cell in cellList)
        //    {

        //        if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], startPos))
        //        {
        //            startCell = cell;
        //            break;
        //        }
        //    }
        //}
        //if (startCell == null)
        //{
        //    return false;
        //}
        return TryGetPath(startCell, endCell, startPos, endPos, pathList, cellList);
    }
    public bool TryGetPath(Cell startCell, Cell endCell, Vector3 startPos, Vector3 endPos, List<Vector3> pathList, List<Cell> cellList)
    {
        BuildCellPath(startCell, startPos, endCell, endPos, cellList);
        BuildPointPath(startPos, endPos, pathList);
        return true;
    }
    //[ContextMenu("rand")]
    public void rand()
    {
        for(int i = 0; i < 100; i++)
        {
            Cell startCell = list[Random.Range(0, list.Count - 1)];
            Cell endCell = list[Random.Range(0, list.Count - 1)];

            Vector3 startPos = startCell.MidPos;
            Vector3 endPos = endCell.MidPos;

            firstPoints = new List<NavPoint>();
            SecondPoints = new List<NavPoint>();

            if (endCell != null && startCell != null)
            {
                BuildCellPath(startCell, startPos, endCell, endPos, CellList);
                BuildPointPath(startPos, endPos, FinalPath);
                Debug.Log("Build Done");
            }
        }
       
    }
    //[ContextMenu("Do")]
    public void Do()
    {
        
        Vector3 startPos = StartPos.position;
        Vector3 endPos = EndPos.position;

        firstPoints = new List<NavPoint>();
        SecondPoints = new List<NavPoint>();

        Cell startCell = null;
        Cell endCell = null;

        //寻找位置所在的三角形
        foreach (var cell in  list)
        {
            //change ex
            if (startCell == null && CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], startPos))
            {
                startCell = cell;
            }
            //change ex
            if (endCell == null && CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], endPos))
            {
                endCell = cell;
            }
            if (endCell != null && startCell != null)
            {
                break;
            }
        }
        if (endCell != null && startCell != null)
        {
            BuildCellPath(startCell, startPos, endCell, endPos, CellList);
            BuildPointPath(startPos, endPos, FinalPath);
            Debug.Log("Build Done");
        }
        //Begin(startPos, endPos);
    }
   
    //得到最短路径单元格
    Cell GetMinPathFormOpenList()
    {
        if (openList.Count > 0)
        {
            Cell minC = openList[0];
            foreach (var c in openList)
            {
                if (c.fValue < minC.fValue)
                {
                    minC = c;
                }
            }
            return minC;
        }

        return null;
    }
    //从开放列表中移除
    void RemoveObjFromOpenList(Cell cell)
    {
        if (cell == null)
        {
            return;
        }
        openList.Remove(cell);
    }
    //计算路径消耗
    void InspectTheAdjacentNodes(Cell cell, Cell adjacent, float sqrMagnitudeDistance, Vector3 endPos)
    {
        if (adjacent == null)
        {
            return;
        }
        adjacent.costToSource = cell.costToSource + sqrMagnitudeDistance;
        var G = adjacent.costToSource;
        var H =  Vector3.SqrMagnitude(endPos - adjacent.MidPos);
        adjacent.fValue = G + H;
        adjacent.parent = cell;

        closeList.Add(adjacent);
        openList.Add(adjacent);
    }
    //获取两个三角形相邻的2个点
    void GetEqualPoint(Cell a, Cell b, out Vector3 v1, out Vector3 v2/*, out Vector3 v3*/)
    {
        v1 = new Vector3();
        v2 = new Vector3();
        //v3 = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            var n = a.nears[i];
            if(n != null)
            {
                if (n.Index == b.index)
                {
                    v1 = a.v[n.PointIndex[0]];
                    v2 = a.v[n.PointIndex[1]];
                    //v3 = b.v[n.NotNearPointIndex];
                }
            }
           
        }
    }
    //建立路径网格
    void BuildCellPath(Cell start_Cell, Vector3 start_Pos, Cell end_Cell, Vector3 end_Pos, List<Cell> cellList)
    {
        openList.Clear();
        closeList.Clear();
        cellList.Clear();
        firstPoints.Clear();
        SecondPoints.Clear();

        Cell cell = null;
        if (start_Cell != end_Cell)
        {
            
            end_Cell.fValue = 0;
            end_Cell.costToSource = 0;

            //openList.Add(end_Cell);
            closeList.Add(end_Cell);

            for (int i = 0; i < 3; i++)
            {
                var nc = end_Cell.nears[i];
                if (nc != null)
                {
                    var ncc = list[nc.Index];
                    ncc.InLineIndex = nc.OtherLineIndex;
                    ncc.costToSource = Vector3.Distance(end_Pos, end_Cell.LineMidPos[i]);

                    ncc.parent = end_Cell;
                    openList.Add(ncc);
                    closeList.Add(ncc);

                }
            }

           


            while (true)
            {
                //得到离起始点最近的点(如果是第一次执行, 得到的是起点)
                cell = GetMinPathFormOpenList();
                if (cell == null)
                {
                    //不到路径
                    break;
                }
                //把计算过的点从开放列表中删除
                RemoveObjFromOpenList(cell);

                if (cell == start_Cell)
                {
                    break;
                }

                for (int i = 0; i < 3; i++)
                {
                    var ncell = cell.nears[i];
                    if (ncell != null)
                    {
                        var ncc = list[ncell.Index];

                        if (!closeList.Contains(ncc))
                        {
                            var pointIndex = -1;
                            CommonFunc.Get1PintIndexBy2LineIndex(cell.InLineIndex, ncell.LineIndex, ref pointIndex);
                            var sqrDistance = cell.LineMidSqrDistance[pointIndex];

                            ncc.InLineIndex = ncell.OtherLineIndex;

                            InspectTheAdjacentNodes(cell, ncc, sqrDistance, start_Pos);
                        }
                    }

                }
            }
        }
        else
        {
            cell = end_Cell;
        }



        Vector3 lastFirstV = new Vector3();
        Vector3 lastSecondV = new Vector3();
        int lastFirstIndex = 0;
        int lastSecondIndex = 0;
        //cell = end_Cell;
        last = -1;//最后一个点是哪边的 0-first， 1-second
        while (cell != null)
        {
            //把路径点加入到路径列表中
            cellList.Add(cell);

            var parent = cell.parent;
            //cell.reset();// = null;

            if (parent != null)
            {
                Vector3 V1;
                Vector3 V2;
                bool v1equalLastFirstV = false;
                bool v2equalLastSecondV = false;

                GetEqualPoint(cell, parent, out V1, out V2);

                if(V1 != lastFirstV)
                {
                    var navp = new NavPoint();
                    navp.v = V1;
                    navp.ConnectIndex = lastSecondIndex;
                    firstPoints.Add(navp);
                    lastFirstIndex = firstPoints.Count - 1;
                    lastFirstV = V1;
                    last = 0;

                }
                else
                {
                    v1equalLastFirstV = true;

                }

                if (V2 != lastSecondV)
                {
                    var navp = new NavPoint();
                    navp.v = V2;
                    navp.ConnectIndex = lastFirstIndex;
                    SecondPoints.Add(navp);
                    lastSecondIndex = SecondPoints.Count - 1;
                    lastSecondV = V2;
                    last = 1;
                }
                else
                {
                    v2equalLastSecondV = true;

                }

                if (v1equalLastFirstV)
                {
                    firstPoints[firstPoints.Count - 1].ConnectIndex = lastSecondIndex;
                }
                if(v2equalLastSecondV)
                {
                    SecondPoints[SecondPoints.Count - 1].ConnectIndex = lastFirstIndex;
                }
                //else
                //{
                //    firstPoints[firstPoints.Count - 1].ConnectIndex = SecondPoints.Count - 1;
                //}
                //if (firstPoints.Count == 0)
                //{
                //    firstPoints.Add(new NavPoint() { v = V1, ConnectIndex = 0 });
                //    lastFirst = V1;
                //    hasadd = true;
                //    last = 2;
                //}
                //if (SecondPoints.Count == 0)
                //{
                //    SecondPoints.Add(new NavPoint() { v = V2, ConnectIndex = 0});
                //    lastSecond = V2;
                //    hasadd = true;
                //    last = 2;
                //}

                //if (!hasadd)
                //{
                //    if (V1 == lastFirst)
                //    {
                //        SecondPoints.Add(new NavPoint() { Index = SecondPoints.Count, v = V2,  ConnectIndex = firstPoints.Count - 1});
                //        firstPoints[firstPoints.Count - 1].ConnectIndex = SecondPoints.Count - 1;
                //        lastSecond = V2;
                //        last = 1;
                //        hasadd = true;
                //    }
                //    else if (V1 == lastSecond)
                //    {
                //        firstPoints.Add(new NavPoint() { Index = firstPoints.Count, v = V2, ConnectIndex = SecondPoints.Count - 1 });
                //        SecondPoints[SecondPoints.Count - 1].ConnectIndex = firstPoints.Count - 1;
                //        lastFirst = V2;
                //        last = 0;
                //        hasadd = true;
                //    }
                //}

                //if (!hasadd)
                //{
                //    if (V2 == lastFirst)
                //    {
                //        SecondPoints.Add(new NavPoint() { Index = SecondPoints.Count, v = V1, ConnectIndex = firstPoints.Count - 1 });
                //        firstPoints[firstPoints.Count - 1].ConnectIndex = SecondPoints.Count - 1;
                //        lastSecond = V1;
                //        last = 1;
                //    }
                //    else if (V2 == lastSecond)
                //    {
                //        firstPoints.Add(new NavPoint() { Index = firstPoints.Count, v = V1, ConnectIndex = SecondPoints.Count - 1 });
                //        SecondPoints[SecondPoints.Count - 1].ConnectIndex = firstPoints.Count - 1;
                //        lastFirst = V1;
                //        last = 0;
                //    }
                //}
            }

            cell = parent;
        }
        //封尾
        if (last == 0)
        {
            firstPoints[firstPoints.Count - 1].ConnectIndex = (SecondPoints.Count);
        }
        else if(last == 1)
        {
            SecondPoints[SecondPoints.Count - 1].ConnectIndex = (firstPoints.Count);
        }else if(last == 2)
        {
            //firstPoints[firstPoints.Count - 1].ConnectIndex = (SecondPoints.Count);
            //SecondPoints[SecondPoints.Count - 1].ConnectIndex = (firstPoints.Count);
        }

        foreach (Cell c in closeList)
        {
            c.reset();
        }
       
    }
    //返回0 P在中间， 1 F在中间 ， 2 S在中间
    int WhichIsBetween(Vector3 O/*原点*/, Vector3 F/*first*/, Vector3 S/*second*/, Vector3 P)
    {
        if (IsBetween(O, F, S, P))
        {
            return 0;
        }
        else if (IsFirstBetween(O, F, S, P))
        {
            return 1;
        }
        else if (IsSecondBetween(O, F, S, P))
        {
            return 2;
        }
        else
        {
            var FN = (F - O).normalized;
            var PN = (P - O).normalized;
            var SN = (S - O).normalized;
            if (FN == PN)
            {
                return 1;
            }
            else if (SN == PN)
            {
                return 2;
            }
            return 1;
        }
    }
     bool IsBetween(Vector3 O/*原点*/, Vector3 F/*first*/, Vector3 S/*second*/, Vector3 P)
    {
        return CommonFunc.SameSide(O, F, S, P) && CommonFunc.SameSide(O, S, F, P);
    }

    //是否是first在中间
     bool IsFirstBetween(Vector3 O/*原点*/, Vector3 F/*first*/, Vector3 S/*second*/, Vector3 P)
    {
        if (IsBetween(O, S, P, F))
        {
            return true;
        }
        return false;
    }
    //是否是second在中间
     bool IsSecondBetween(Vector3 O/*原点*/, Vector3 F/*first*/, Vector3 S/*second*/, Vector3 P)
    {
        if (IsBetween(O, F, P, S))
        {
            return true;
        }
        return false;
    }

    int GetFirstI()
    {
        return Mathf.Min(firstI, firstPoints.Count - 2);
    }
    int GetFirstStep()
    {
        return Mathf.Min(firstI+firstStep+1, firstPoints.Count - 1);
    }
    int GetFirstStepReal()
    {
        return firstI + firstStep + 1;
    }
    int GetSecondI()
    {
        return Mathf.Min(secondI, SecondPoints.Count - 2);
    }
    int GetSecondStep()
    {
        return Mathf.Min(secondI + secondStep+1, SecondPoints.Count - 1);
    }
    int GetSecondStepReal()
    {
        return secondI + secondStep + 1;
    }
    bool IsFirstStepEnd()
    {
        return firstI + firstStep + 1 >= firstPoints.Count - 2;

    }
    bool IsSecendStepEnd()
    {
        return secondI + secondStep + 1 >= SecondPoints.Count - 2;
    }
    bool IsFirstEnd()
    {
        return firstI /*+ firstStep + 1 */>= firstPoints.Count - 1;

    }
    bool IsSecendEnd()
    {
        return secondI /*+ secondStep + 1*/ >= SecondPoints.Count - 1;
    }




    //计算最终路径点
    void BuildPointPath(Vector3 startPos, Vector3 endPos, List<Vector3> pathList)
    {
        

        pathList.Clear();
        pathList.Add(startPos);
        if(last == -1)
        {
            pathList.Add(endPos);
            return;
        }
        firstPoints.Add(new NavPoint() { v = endPos });
        SecondPoints.Add(new NavPoint() { v = endPos });

        firstI = 0;
        secondI = 0;
        firstStep = 0;
        secondStep = 0;
        firstUntil = 0;
        secondUntil = 0;

        int bi1 = -1;
        int bi2 = -1;


        //判断从那个边开始
        if (firstPoints[0].ConnectIndex != 0)
        {
            isFirstQ = false;
            secondUntil = firstPoints[0].ConnectIndex;
        }
        else if(SecondPoints[0].ConnectIndex != 0)
        {
            isFirstQ = true;
            firstUntil = SecondPoints[0].ConnectIndex;
        }
        else//只有2个Cell的情况下
        {

        }


        int whileCount = 0;

        while (true)
        {
            //以防死循环
            if(whileCount > 300)
            {
                Debug.LogError("失败");
                break;
            }else if(whileCount == 290)
            {
                Debug.LogError("失败");
            }
            whileCount++;

            //设置导航边开关
            if (isFirstQ)
            {
                if(GetFirstStepReal() > firstUntil)
                {
                    isFirstQ = false;
               
                    secondUntil = firstPoints[GetFirstStep() - 1].ConnectIndex;

                }
            }
            else
            {
                if(GetSecondStepReal() > secondUntil)
                {
                    isFirstQ = true;
                    //var ci = SecondPoints[GetSecondStep()-1].ConnectIndex;
                    firstUntil = SecondPoints[GetSecondStep() - 1].ConnectIndex;
                }
            }

            //var firstPointV = 
            //计算向量的相互位置
            bi1 = WhichIsBetween(startPos, firstPoints[GetFirstI()].v, SecondPoints[GetSecondI()].v, firstPoints[GetFirstStep()].v);
            bi2 = WhichIsBetween(startPos, firstPoints[GetFirstI()].v, SecondPoints[GetSecondI()].v, SecondPoints[GetSecondStep()].v);

            //过滤钝角
            if (bi2 == 1)
            {
                //change ex
                if (CommonFunc.PointinTriangleEX(firstPoints[GetFirstI()].v, SecondPoints[GetSecondI()].v, SecondPoints[GetSecondStep()].v, startPos)) 
                {
                    bi2 = 2;
                }
            }
            if(bi1 == 2)
            {
                //change ex
                if (CommonFunc.PointinTriangleEX(firstPoints[GetFirstI()].v, SecondPoints[GetSecondI()].v, firstPoints[GetFirstStep()].v, startPos))
                {
                    bi1 = 1;
                }
            }
            //如果2个子向量 在2个父向量里面
            if (bi1 == 0 && bi2 == 0 )
            {
                if (IsFirstStepEnd() && IsSecendStepEnd())
                {
                    break;
                }
                var bb1 = WhichIsBetween(startPos, SecondPoints[GetSecondI()].v, SecondPoints[GetSecondStep()].v, firstPoints[GetFirstStep()].v);
                var bb2 = WhichIsBetween(startPos, firstPoints[GetFirstI()].v, firstPoints[GetFirstStep()].v, SecondPoints[GetSecondStep()].v);
                //如果两个子向量的错位，非正常
                if (bb1 == 0 && bb2 == 0)
                {
                    //比较那个子向量比较长，设置另一边为拐点
                    if((firstPoints[GetFirstStep()].v - startPos).sqrMagnitude > (SecondPoints[GetSecondStep()].v - startPos).sqrMagnitude)
                    {
                        bi1 = 2;
                        bi2 = 2;
                        secondI = GetSecondStep();
                    }
                    else
                    {
                        bi2 = 1;
                        bi1 = 1;
                        firstI = GetFirstStep();
                    }
                }
            }

            //如果子向量在2个父向量中间（2个子向量位置正常）
            if (bi1 == 0 && isFirstQ)//s -- p_f -- f
            {
                firstI += (firstStep + 1);
                firstStep = 0;

            }
            //如果子向量中间隔了另一边的父向量， 则设置另一边为拐点
            if (/*(!IsFirstEnd()||IsSecendEnd()) &&*/ bi2 == 1)//s -- f -- p_s f为拐点
            {
                //如果另一个子向量不在2个父向量中间
                if (bi1 != 0)
                {
                    startPos = firstPoints[GetFirstI()].v;
                    pathList.Add(startPos);
                    firstStep = 0;
                    secondStep = 0;
                    secondI = firstPoints[GetFirstI()].ConnectIndex;
                    firstI++;

                    if (IsFirstEnd() || IsSecendEnd())
                    {
                        break;
                    }

                    secondUntil = secondI;
                    firstUntil = firstI;
                }
                else
                {
                    //跳过此轮， 等待另一边的计算
                    secondUntil = GetSecondStep() - 1;
                }
            }
            //如果子向量中间隔了一个父向量
            else if (bi1 == 1 && isFirstQ)//s -- f -- p_f
            {
                firstStep++;
            }




            if (bi2 == 0 && !isFirstQ)//f -- p_s -- s
            {
                secondI += (secondStep + 1);
                secondStep = 0;
            }

            if (/*(!IsSecendEnd()||IsFirstEnd()) &&*/ bi1 == 2)//p_f -- s -- f， s为拐点
            {
                if (bi2 != 0)
                {
                    startPos = SecondPoints[GetSecondI()].v;
                    pathList.Add(startPos);
                    firstStep = 0;
                    secondStep = 0;
                    firstI = SecondPoints[GetSecondI()].ConnectIndex;
                    secondI++;

                    if (IsFirstEnd() || IsSecendEnd())
                    {
                        break;
                    }
                    secondUntil = secondI;
                    firstUntil = firstI;
                }
                else
                {
                    firstUntil = GetFirstStep() - 1;
                }

            }
            else if (bi2 == 2 && !isFirstQ)//p_s -- s -- f
            {
                secondStep++;
            }

            

        }
        pathList.Add(endPos);
       // Debug.Log(whileCount);
    }

    //public void drawString(string text, Vector3 worldPos, Color? colour = null)
    //{
    //    UnityEditor.Handles.BeginGUI();
    //    if (colour.HasValue) GUI.color = colour.Value;
    //    var view = UnityEditor.SceneView.currentDrawingSceneView;
    //    Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
    //    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
    //    GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
    //    UnityEditor.Handles.EndGUI();
    //}
    //private void OnDrawGizmos()
    //{
    //    if (list != null)
    //    {

    //        foreach (var c in list)
    //        {
    //            Gizmos.color = Color.blue;
    //            Gizmos.DrawLine(c.v[0], c.v[1]);
    //            Gizmos.DrawLine(c.v[1], c.v[2]);
    //            Gizmos.DrawLine(c.v[2], c.v[0]);

    //            drawString(c.index.ToString(), c.MidPos, Color.red);
    //            //foreach (var n in c.nears)
    //            //{
    //            //    if (n != null)
    //            //    {
    //            //        Gizmos.color = Color.green;
    //            //        Gizmos.DrawLine(c.MidPos, list[n.Index].MidPos);
    //            //    }

    //            //}
    //        }

    //    }

    //    if (firstPoints != null)
    //    {
    //        Gizmos.color = Color.red;
    //        foreach (var c in firstPoints)
    //        {
    //            Gizmos.DrawSphere(c.v, 0.2f);

    //        }
    //    }

    //    if (SecondPoints != null)
    //    {
    //        Gizmos.color = Color.green;
    //        foreach (var c in SecondPoints)
    //        {
    //            Gizmos.DrawSphere(c.v, 0.2f);
    //        }
    //    }
    //    if (FinalPath != null && FinalPath.Count > 0)
    //    {
    //        Gizmos.color = Color.red;
    //        Vector3 oldV = FinalPath[0];
    //        foreach (var c in FinalPath)
    //        {
    //            Gizmos.DrawLine(oldV, c);
    //            oldV = c;
    //        }
    //    }
    //    //if (CellList != null)
    //    //{
    //    //    Gizmos.color = Color.black;
    //    //    foreach (var c in CellList)
    //    //    {
    //    //        //Gizmos.DrawIcon(c.MidPos, c.index.ToString(),true);

    //    //        Gizmos.DrawSphere(c.MidPos, 0.2f);
    //    //    }
    //    //}

    //    //Gizmos.color = Color.yellow;
    //    //foreach (var c in ExportNavMesh.testList)
    //    //{
    //    //    Gizmos.DrawLine(c.v[0], c.v[1]);
    //    //    Gizmos.DrawLine(c.v[1], c.v[2]);
    //    //    Gizmos.DrawLine(c.v[2], c.v[0]);
    //    //}
    //}



    //========================================================================================
    //rubish
    public int GetCellIndexAndSetMoveVector2(int oldIndex, Vector3 pos, ref Vector3 normal, float speed)
    {
        if (oldIndex < 0 || oldIndex >= list.Count)
        {
            return GetCellIndex(pos);
        }
        Cell cell = list[oldIndex];
        if (!CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], pos))
        {
            Debug.Log("起始位置不在记录的网格中！");
            cell = FindCellByPos(pos);
            if (cell == null)
            {
                Debug.LogError("3 起始位置超出边界了！");
                normal = Vector3.zero;
                return oldIndex;
            }
        } 
        if (normal == Vector3.zero)
        {
            return oldIndex;
        }



        Cell finalCell = GetCellWitchCross(cell, cell, pos, pos + normal, ref normal, -1, speed, 0);
        return finalCell.index;
    }

    public Cell FindCellByPos(Vector3 pos)
    {
        foreach (var c in list)
        {
            if (CommonFunc.PointinTriangleEX(c.v[0], c.v[1], c.v[2], pos))
            {
                return c;
            }
        }
        return null;
    }
    public Cell FindCellByIndex(int index)
    {
        if (list == null)
            return null;
        if (index < 0 || index >= list.Count)
            return null;
        return list[index];
    }


    public Cell FindEndPosCell(Cell startCell, Vector3 startPos, Vector3 endPos, int lastLineIndex = -1)
    {
        for (int i = 0; i < 3; i++)
        {
            if (lastLineIndex == i)
                continue;

            int i1 = -1;
            int i2 = -1;
            CommonFunc.Get2PointIndexBy1LineIndex(i, ref i1, ref i2);
            Vector3 v1 = startCell.v[i1];
            Vector3 v2 = startCell.v[i2];

            if (CommonFunc.LinesCross(v1, v2, startPos, endPos))
            {
                if (startCell.nears[i] != null)
                {
                    Cell cell = list[startCell.nears[i].Index];
                    if (CommonFunc.PointinTriangleEX(cell.v[0], cell.v[1], cell.v[2], endPos))
                    {
                        return cell;
                    }
                    else
                    {
                        return FindEndPosCell(cell, startPos, endPos, startCell.nears[i].OtherLineIndex);
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        return startCell;
    }
}
