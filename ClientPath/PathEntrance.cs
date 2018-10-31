using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ClientPath
{
    public static class PathEntrance
    {
        public static  Nav m_Nav;


        public static void Init(string fileName)
        {
            //if (m_Nav == null)
           // {
                m_Nav = new Nav();
                m_Nav.Init(fileName);
            //}
        }


        //public static bool CalculateAndSetPath(Vector3 startPos, GtMsg.S_MonsterTerminal data, float speed, ref List<Vector3> pathList, ref List<Cell> cellList, ref List<int> frameList, ref int pathIndex)
        //{
        //    Vector3 endPos = CommonFunc.GetVector3(data.Pos);
        //    int endCellIndex = data.EndCellIndex;

        //    List<Vector3> tempPathList = new List<Vector3>();
        //    if (!m_Nav.TryGetPath(startPos, endPos, endCellIndex, tempPathList, cellList))
        //    {
        //        return false;
        //    }

        //    return CalculatePath(tempPathList, endPos, data.Frame, speed, pathList, frameList, ref pathIndex);
        //}

        //计算路径点
        private static bool CalculatePath(List<Vector3> tempPathList, Vector3 endPos, int frame,  float speed,  List<Vector3> pathList,   List<int> frameList/*, ref int pathIndex*/)
        {
            if (tempPathList.Count == 0)
            {
                return false;
            }
            pathList.Clear();
            frameList.Clear();

            float mL = 10;
            for (int i = 0; i < tempPathList.Count; i++)
            {

                var v2 = tempPathList[i];
                pathList.Add(v2);

                if (i + 1 < tempPathList.Count)
                {
                    var v1 = tempPathList[i + 1];
                    Vector3 v3 = new Vector3();
                    if (CommonFunc.TryGetPointInLineWitchIsLengthFrom1Point(endPos, v1, v2, mL, ref v3))
                    {
                        pathList.Add(v3);
                        break;
                    }
                }
            }

            for (int i = 0; i < pathList.Count; i++)
            {
                if (i == 0)
                {
                    frameList.Add(frame);
                }
                else
                {
                    float distance = Vector3.Distance(pathList[i], pathList[i - 1]);
                    int add = (int)(distance / speed);
                    frameList.Add(frameList[i - 1] + add);
                }
            }
            //pathIndex = 0;
            return true;
        }
        ////服务器发来拐点列表时，设置一些参数
        ///**
        // * nowFrame 主逻辑当前帧 
        // * speed 怪物速度， 当前服务器的速度是 2/60.0f
        // * PosList 服务器发来的路径列表
        // * pathList 客户端要保存的路径点
        // * frameList 客户端要保存帧列表
        // * pathIndex 客户端要保存的索引
        // **/
        public static void SetPath(int nowFrame, float speed, GtMsg.S_MovePosList PosList, ref List<Vector3> pathList, ref List<int> frameList/*, ref int pathIndex*/)
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
            //pathIndex = 0;
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
        public static bool TryGetPosByFrame(int nowFrame, ref List<Vector3> pathList, ref List<int> frameList, /*ref int pathIndex,*/ ref Vector3 pos, ref float passFrame, ref float sectionFrame)
        {
            if (frameList.Count == 0 || frameList[0] > nowFrame || frameList[frameList.Count - 1] < nowFrame)
            {
                return false;
            }
            var pathIndex = 0;
            for (int i = 0; i < frameList.Count; i++)
            {
                if (frameList[i] < nowFrame)
                {
                    pathIndex = i;
                }
            }

            Vector3 v0 = pathList[pathIndex];
            Vector3 v2 = pathList[pathIndex + 1];
            Vector3 v3 = v2 - v0;

            int f0 = frameList[pathIndex];
            passFrame = nowFrame - f0;
            sectionFrame = frameList[pathIndex + 1] - f0;

            if (sectionFrame == 0)
            {
                return false;
            }
            float fp = passFrame / sectionFrame;

            pos = v3 * fp + v0;
            // Debug.Log("pos:" + pos);
            return true;
        }

        //==========================================================

        public static int GetCellIndexAndSetMoveVector2(int oldIndex, Vector3 pos, ref Vector3 normal, float speed)
        {
            return m_Nav.GetCellIndexAndSetMoveVector2(oldIndex, pos, ref normal, speed);
        }


        public static bool GetPath(int frame, Vector3 startPos, Vector3 endPos, float speed,  List<Vector3> pathList,  List<Cell> cellList,  List<int> frameList/*, ref int pathIndex*/)
        {
            Cell startCell = null;
            if (cellList.Count > 0)
                startCell = m_Nav.GetCellInCellListByPos(startPos, cellList);
            if (startCell == null)
                startCell = m_Nav.FindCellByPos(startPos);
            if (startCell == null)
                startCell = FindCellOffset(startPos, 0.1f);

            Cell endCell = m_Nav.FindCellByPos(endPos);
            if (startCell == null || endCell == null)
            {
                Debug.LogError(" find path cell is out range !  startcell = " + startCell + "  endCell = " + endCell);
                return false;
            }

            List<Vector3> tempPathList = new List<Vector3>();
            TryGetPath(startCell, endCell, startPos, endPos, tempPathList, cellList);

            return CalculatePath(tempPathList, endPos, frame, speed, pathList, frameList/*, ref pathIndex*/);

        }
        public static bool TryGetPath(Vector3 startPos, Vector3 endPos, List<Vector3> pathList)
        {
            var startCell = m_Nav.FindCellByPos(startPos);
            var endCell = m_Nav.FindCellByPos(endPos);
            List<Cell> cellList = new List<Cell>();
            return TryGetPath(startCell, endCell, startPos, endPos, pathList, cellList);
            
        }
        public static bool TryGetPath(Cell startCell, Cell endCell, Vector3 startPos, Vector3 endPos,List<Vector3> pathList, List<Cell> cellList)
        {
            return m_Nav.TryGetPath(startCell, endCell, startPos, endPos, pathList, cellList);

        }

        private static Cell FindCellOffset(Vector3 pos, float v)
        {
            Cell c = null;
            Vector3 p = new Vector3(pos.x + v, pos.y, pos.z);
            c = m_Nav.FindCellByPos(p);
            if (c != null)
                return c;
            p = new Vector3(pos.x - v, pos.y, pos.z);
            c = m_Nav.FindCellByPos(p);
            if (c != null)
                return c;
            p = new Vector3(pos.x, pos.y, pos.z + v);
            c = m_Nav.FindCellByPos(p);
            if (c != null)
                return c;
            p = new Vector3(pos.x, pos.y, pos.z - v);
            c = m_Nav.FindCellByPos(p);
            if (c != null)
                return c;
            return null;
        }

        public static Cell FindCellByPos(Vector3 pos)
        {
            return m_Nav.FindCellByPos(pos);
        }

        public static void BWZ(Vector3 AttackerPoint, Vector3 VictimPoint, float Width, float Length, ref Vector3 A, ref Vector3 B, ref Vector3 C, ref Vector3 D)
        {
            CommonFunc.QuadranglePoint(AttackerPoint, VictimPoint, Width, Length, ref A, ref B, ref C, ref D);
        }
        public static void CFXZ(int startCellIndex, Vector3 startPos, Vector3 endPos, ref Vector3 finalPos, ref int finalCellIndex)
        {
            m_Nav.CFXZ(startCellIndex, startPos, endPos, ref finalPos, ref finalCellIndex);
        }
        public static Vector3 CFXZ( Vector3 startPos, Vector3 endPos)
        {
            int startCellIndex = m_Nav.GetCellIndex(startPos);
            int finalCellIndex = 0;
            Vector3 finalPos = new Vector3();
            m_Nav.CFXZ(startCellIndex, startPos, endPos, ref finalPos, ref finalCellIndex);
            return finalPos;
        }
    }

    
}
