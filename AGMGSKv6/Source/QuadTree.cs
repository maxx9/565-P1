using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;



namespace AGMGSKv6
{
    public class QuadNode
    {   
        //attributes
        public int X;
        public int Z;
        public int level;
        public List<QuadNode> adjacent;

        //used for a*
        public bool used;
        public double cost, distanceToSource, DistanceToGoal;
        public QuadNode predecessor;
    }
    
    public class QuadTree
    {
        private int MAX_LEVELS = 9;

        public int level;
        private int spacing = 150;
        private Stage stage;
        private int xmin, xmax, zmin, zmax;

        public QuadTree TopLeftTree;
        public QuadTree TopRightTree;
        public QuadTree BottomLeftTree;
        public QuadTree BottomRightTree;

        //public Point TopLeftPoint;
        //public Point TopRightPoint;
        //public Point BottomLeftPoint;
        //public Point BottomRightPoint;

        //public bool isTLN;
        //public bool isTRN;
        //public bool isBLN;
        //public bool isBRN;

        public Point CenterPoint;
        public bool isCP;
        public bool leaf;

        public Path pathpath;

        //quadtree constructor
        public QuadTree(Stage istage, int ilevel, int ixmin, int ixmax, int izmin, int izmax)
        {
            stage = istage;
            level = ilevel;
            xmin = ixmin;
            xmax = ixmax;
            zmin = izmin;
            zmax = izmax;
            leaf = true;

            if (testRegionColls() && (level < MAX_LEVELS))
                split();
            else if(level<5)
                split();
            else
            {
                //TopLeftPoint = new Point(xmin, zmin);
                //TopRightPoint = new Point(xmax, zmin);
                //BottomLeftPoint = new Point(xmin, zmax);
                //BottomRightPoint = new Point(xmax, zmax);

                //isTLN = testPointColls(TopLeftPoint);
                //isTRN = testPointColls(TopRightPoint);
                //isBLN = testPointColls(BottomLeftPoint);
                //isBRN = testPointColls(BottomRightPoint);

                CenterPoint = new Point((xmin + xmax) / 2, (zmin + zmax) / 2);
                isCP = testPointColls(CenterPoint);
            }
        }

        //creates 4 children
        private void split()
        {
            if (level == MAX_LEVELS)
                return;
            
            leaf = false;

            TopLeftTree = new QuadTree(stage, level + 1, xmin, (xmin + xmax) / 2, zmin, (zmin + zmax) / 2);
            TopRightTree = new QuadTree(stage, level + 1, (xmin + xmax + 1) / 2, xmax, zmin, (zmin + zmax) / 2);
            BottomLeftTree = new QuadTree(stage, level + 1, xmin, (xmin + xmax) / 2, (zmin + zmax + 1) / 2, zmax);
            BottomRightTree = new QuadTree(stage, level + 1, (xmin + xmax + 1) / 2, xmax, (zmin + zmax + 1) / 2, zmax);
        }

        //tests region collisions for splits
        private bool testRegionColls()
        {
            foreach ( Object3D obj in stage.Collidable )
            {
                if(obj.UseForGraph)
                {
                    if (collideded(new Point((xmin + xmax) / 2, (zmin + zmax) / 2), new Point((int)obj.Translation.X / 150, (int)obj.Translation.Z / 150), xmax - xmin + 1, obj.ObjectBoundingSphereRadius / 150 ))
                        return true;
                }
            }

            for (int i = 0; i < stage.npAgent.pathNode.Length / 2; i++)
            {
                if (collideded(new Point((xmin + xmax) / 2, (zmin + zmax) / 2), new Point(stage.npAgent.pathNode[i, 0], stage.npAgent.pathNode[i, 1]), xmax - xmin, 1))
                    return true;
            }

            for (int i = 0; i < stage.treasures.locations.Count; i++)
            {
                if (collideded(new Point((xmin + xmax) / 2, (zmin + zmax) / 2), new Point(stage.treasures.locations[i].X, stage.treasures.locations[i].Y), xmax - xmin, 1))
                    return true;
            }

            //if (collideded(new Point((xmin + xmax) / 2, (zmin + zmax) / 2), new Point(490, 450), xmax - xmin, 1))
            //    return true;

           return false;
        }
        
        //tests point collisions to see if vertex is navigatable
        private bool testPointColls( Point P)
        {
            foreach (Object3D obj in stage.Collidable)
            {
                if (obj.UseForGraph)
                {
                    if (collideded(P, new Point((int)obj.Translation.X / 150, (int)obj.Translation.Z / 150), stage.Collidable[0].ObjectBoundingSphereRadius / 75, obj.ObjectBoundingSphereRadius / 150))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //tests 2 bounding spheres
        private bool collideded(Point P1, Point P2, float R1, float R2)
        {
            double distance = getDistance(P1.X, P1.Y, P2.X, P2.Y);

            if ((R1 + R2) > distance)
                return true;
            else
                return false;
        }

        private double getDistance( int X1, int Z1, int X2, int Z2 )
        {
            return Math.Sqrt(((X1 - X2) * (X1 - X2)) + ((Z1 - Z2) * (Z1 - Z2)));
        }




        public int countN = 0;
        public int countL = 0;

        //goes through the quad tree and makes a nice arraylist of navigatable nodes
        public void traverseTree(QuadTree Q, List<QuadNode> QM )
        {
            countN++;
            if (Q.leaf)
            {
                countL++;
                //if (Q.isTLN)
                //    addNode((int)Q.TopLeftPoint.X, (int)Q.TopLeftPoint.Y, Q.level, QM);
                //if (Q.isTRN)
                //    addNode((int)Q.TopRightPoint.X, (int)Q.TopRightPoint.Y, Q.level, QM);
                //if (Q.isBLN)
                //    addNode((int)Q.BottomLeftPoint.X, (int)Q.BottomLeftPoint.Y, Q.level, QM);
                //if (Q.isBRN)
                //    addNode((int)Q.BottomRightPoint.X, (int)Q.BottomRightPoint.Y, Q.level, QM);

                if (Q.isCP)
                    addNode((int)Q.CenterPoint.X, (int)Q.CenterPoint.Y, Q.level, QM);
            }
            else
            {
                traverseTree(Q.TopLeftTree, QM);
                traverseTree(Q.TopRightTree, QM);
                traverseTree(Q.BottomLeftTree, QM);
                traverseTree(Q.BottomRightTree, QM);
            }
        }

        //adds node to array list for traverseTree()
        private void addNode(int X, int Z, int L, List<QuadNode> nodeMap)
        {
            QuadNode temp = new QuadNode();
            temp.X = X;
            temp.Z = Z;
            temp.level = L;
            temp.adjacent = new List<QuadNode>();

            nodeMap.Add(temp);
        }

        //sets the adjacent nodes in the array list
        public void setAdj(List<QuadNode> nodeMap)
        {
            int XX, ZZ;
            double LD;
            
            for (int i = 0; i < nodeMap.Count; i++)
                for (int j = 0; j < nodeMap.Count; j++)
                {
                    if (i != j)
                    {
                        XX = nodeMap[i].X - nodeMap[j].X;
                        ZZ = nodeMap[i].Z - nodeMap[j].Z;

                        if (nodeMap[i].level < nodeMap[j].level)
                            LD = 512 * Math.Sqrt(2) / (Math.Pow(2, nodeMap[i].level));
                        else
                            LD = 512 * Math.Sqrt(2) / (Math.Pow(2, nodeMap[j].level));

                        if ( Math.Sqrt( Math.Pow(XX,2) + Math.Pow(ZZ,2) ) < LD )
                            if( reachable(0, nodeMap[i].X, nodeMap[i].Z, nodeMap[j].X, nodeMap[j].Z ) )
                                nodeMap[i].adjacent.Add(nodeMap[j]);
                    }
                }
        }

        //checks to see if 2 nodes can reach eachother for setAdj()
        private bool reachable(int level, int X1, int Z1, int X2, int Z2)
        {
            if ((Math.Abs(X1 - X2) < 2) && (Math.Abs(Z1 - Z2) < 2))
                return true;

            foreach (Object3D obj in stage.Collidable)
            {
                if (obj.UseForGraph)
                    if (collideded(new Point((X1 + X2) / 2, (Z1 + Z2) / 2), new Point((int)obj.Translation.X / 150, (int)obj.Translation.Z / 150), stage.Collidable[0].ObjectBoundingSphereRadius / 100, obj.ObjectBoundingSphereRadius / 150))
                        return false;
            }

            if ( !reachable(level + 1, X1, Z1, (X1 + X2) / 2, (Z1 + Z2) / 2) )
                return false;
            if ( !reachable(level + 1, (X1 + X2) / 2, (Z1 + Z2) / 2, X2, Z2) )
                return false;

            return true;
        }



        //a*
        public List<NavNode> aStar(NavNode begin, NavNode end, List<QuadNode> nodeMap)
        {
            List<NavNode> pathpathNav = new List<NavNode>(); //for diplaying
            List<NavNode> PathNav = new List<NavNode>(); //for returning

            List<QuadNode> openPath = new List<QuadNode>(); //all inner
            List<QuadNode> closedPath = new List<QuadNode>(); //all outer
            List<QuadNode> takenPath = new List<QuadNode>(); //result path

            QuadNode start = new QuadNode();
            QuadNode finish = new QuadNode();
            QuadNode cur = new QuadNode();

            int index;
            bool keepGoing = true;

            foreach (QuadNode QN in nodeMap)
            {
                QN.used = false;
                
                if ((QN.X == (int)begin.Translation.X / 150) && (QN.Z == (int)begin.Translation.Z / 150))
                {
                    start = QN;
                    start.used = true;
                    start.cost = 0;
                    start.predecessor = null;
                }
                if ((QN.X == (int)end.Translation.X / 150) && (QN.Z == (int)end.Translation.Z / 150))
                {
                    finish = QN;
                }
            }

            openPath.Add(start);

            while( (openPath.Count > 0) && keepGoing )
            {
                cur = openPath[0];
                for (index = 1; index < openPath.Count; index++)
                    if (openPath[index].cost < cur.cost)
                        cur = openPath[index];

                if ((cur.X == finish.X) && (cur.Z == finish.Z))
                    keepGoing = false;
                else
                {
                    openPath.Remove(cur);
                    closedPath.Add(cur);

                    for (int i = 0; i < cur.adjacent.Count; i++ )
                    {
                        if (!cur.adjacent[i].used)
                        {
                            cur.adjacent[i].used = true;
                            cur.adjacent[i].distanceToSource = getDistance(cur.adjacent[i].X, cur.adjacent[i].Z, start.X, start.Z);
                            cur.adjacent[i].DistanceToGoal = getDistance(cur.adjacent[i].X, cur.adjacent[i].Z, finish.X, finish.Z);
                            cur.adjacent[i].cost = cur.adjacent[i].distanceToSource + cur.adjacent[i].DistanceToGoal;
                            //cur.adjacent[i].predecessor = new List<QuadNode>();
                            cur.adjacent[i].predecessor = cur;
                            openPath.Add(cur.adjacent[i]);
                        }
                    }
                }
            }

            cur = cur.predecessor;

            while (cur != null)
            {
                openPath.Remove(cur);
                takenPath.Add(cur);

                PathNav.Add(new NavNode(new Vector3(cur.X * spacing, stage.Terrain.surfaceHeight(cur.X, cur.Z), cur.Z * spacing), NavNode.NavNodeEnum.PATH));
                cur = cur.predecessor;
            }
            PathNav.Reverse();
            PathNav.Add(new NavNode(new Vector3(finish.X * spacing, stage.Terrain.surfaceHeight(finish.X, finish.Z), finish.Z * spacing), NavNode.NavNodeEnum.WAYPOINT));

            foreach (QuadNode obj in openPath)
                pathpathNav.Add(new NavNode(new Vector3(obj.X * spacing, stage.Terrain.surfaceHeight(obj.X, obj.Z), obj.Z * spacing),NavNode.NavNodeEnum.OPEN));
            
            foreach (QuadNode obj in closedPath)
                pathpathNav.Add(new NavNode(new Vector3(obj.X * spacing, stage.Terrain.surfaceHeight(obj.X, obj.Z), obj.Z * spacing), NavNode.NavNodeEnum.CLOSED));

            foreach (QuadNode obj in takenPath)
                pathpathNav.Add(new NavNode(new Vector3(obj.X * spacing, stage.Terrain.surfaceHeight(obj.X, obj.Z), obj.Z * spacing), NavNode.NavNodeEnum.PATH));

            pathpathNav.Add(new NavNode(new Vector3(finish.X * spacing, stage.Terrain.surfaceHeight(finish.X, finish.Z), finish.Z * spacing), NavNode.NavNodeEnum.WAYPOINT));

            stage.Components.Remove(pathpath);
            pathpath = new Path(stage, pathpathNav, Path.PathType.SINGLE);
            stage.Components.Add(pathpath);


            return PathNav;
            //List<NavNode> ans = new List<NavNode>();
            //NavNode ttt = null;
            ////ans.Add(ttt);
            //ans.Add(begin);
            //ans.Add(end);
            //return ans;
        }







        //this is for personal testing. ignore this... (Creates a txt file of my navigatable nodes)
        public void testtest(List<QuadNode> nodeMap)
        {
            int range = 512;

            QuadNode[,] nodez;
            nodez = new QuadNode[range, range];

            for (int i = 0; i < range; i++)
                for (int j = 0; j < range; j++)
                {
                    nodez[i, j] = new QuadNode();
                    nodez[i, j].X = i;
                    nodez[i, j].Z = j;
                    nodez[i, j].level = -1;
                    nodez[i, j].adjacent = new List<QuadNode>();
                }

            for (int i = 0; i < nodeMap.Count; i++)
                nodez[nodeMap[i].X, nodeMap[i].Z].level = nodeMap[i].level;

            String writerrr = "";
            for (int j = 0; j < range; j++)
            {
                for (int i = 0; i < range; i++)
                {
                    if (nodez[i, j].level != -1)
                        writerrr += nodez[i, j].level + " ";
                    else
                        writerrr += ". ";
                }
                writerrr += "\r\n";
                System.IO.File.WriteAllText(@"testing_map.txt", writerrr);
            }
        }
    }
}
