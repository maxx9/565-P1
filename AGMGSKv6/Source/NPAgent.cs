//Maksim Sadovkiy - maksim.sadovskiy.658@my.csun.edu
//Comp 565 Project 1 - Terrain/AGMGSK

/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
    XNA 4 refresh to MonoGames 3.2.  

    AGMGSKv6 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if MONOGAMES //  true, build for MonoGames
   using Microsoft.Xna.Framework.Storage; 
#endif
#endregion

namespace AGMGSKv6 {

/// <summary>
/// A non-playing character that moves.  Override the inherited Update(GameTime)
/// to implement a movement (strategy?) algorithm.
/// Distribution NPAgent moves along an "exploration" path that is created by the
/// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
/// Paths can also be specified in text files of Vector3 values, see alternate
/// Path class constructors.
/// 
/// 12/31/2014 last changed
/// </summary>
public class NPAgent : Agent {

    private bool seekMode, firstTurn;
    private NavNode TreasureGoal, storedGoal;

    private NavNode nextGoal, nextNodeToGoal;
   private Path explorePath, pathToGoal;

   private List<NavNode> nodesToGoal;

   private int snapDistance = 20;  // this should be a function of step and stepSize
     //If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array

    //public int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
    //                                     {435, 505}, {425, 500}, {420, 490},  // bottom, middle
    //                                     {420, 450}, {425, 440}, {435, 435},  // middle, middle
    //                           {490, 435}, {500, 430}, {505, 420},  // middle, right
    //                                     {505, 105}, {500,  95}, {490,  90},  // top, right
    //                           {110,  90}, {100,  95}, { 95, 105},  // top, left
    //                                     { 95, 480}, {100, 490}, {110, 495},  // bottom, left
    //                                     {400, 500} };								  // loop return

   public int[,] pathNode = { {400, 400}, {400, 500}, {500, 500}, {470, 475}, {470, 425},   //around the treasures
                                        {505, 105}, {500,  95}, {490,  90},  // top, right
                              {110,  90}, {100,  95}, { 95, 105},  // top, left
                                        { 95, 480}, {100, 490}, {110, 495},  // bottom, left
                                        {500, 400} };								  // loop return
   /// <summary>
   /// Create a NPC. 
   /// AGXNASK distribution has npAgent move following a Path.
   /// </summary>
   /// <param name="theStage"> the world</param>
   /// <param name="label"> name of </param>
   /// <param name="pos"> initial position </param>
   /// <param name="orientAxis"> initial rotation axis</param>
   /// <param name="radians"> initial rotation</param>
   /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
   public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
      float radians, string meshFile)
      : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "npFirst";
      follow.Name = "npFollow";
      above.Name =  "npAbove";
      // path is built to work on specific terrain, make from int[x,z] array pathNode
      explorePath = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
      //stage.Components.Add(explorePath);
      
       nodesToGoal = new List<NavNode>();
       nextNodeToGoal = new NavNode(pos);
       nextGoal = new NavNode(pos);

		// set snapDistance to be a little larger than step * stepSize
		snapDistance = (int) (1.5 * (agentObject.Step * agentObject.StepSize));

        firstTurn = true;
        seekMode = false;
      }   

    private void setPath( bool getNext )
   {
       if(getNext)
       {
           if (seekMode)
               nextGoal = TreasureGoal;
           else
               nextGoal = explorePath.NextNode;
       }

       nodesToGoal = stage.QT.aStar(nextNodeToGoal, nextGoal, stage.nodeMap);
       pathToGoal = new Path(stage, nodesToGoal, Path.PathType.SINGLE);
       nextNodeToGoal = pathToGoal.NextNode;
       agentObject.turnToFace(nextNodeToGoal.Translation);
   }

    public void TreasureSeek(Vector3 target)
   {
       seekMode = true;
       TreasureGoal = new NavNode(target);
       storedGoal = nextGoal;
       setPath(true);
   }


   /// <summary>
   /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
   /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
   /// continue making steps towards the nextGoal.
   /// </summary>
   public override void Update(GameTime gameTime) {

       if (firstTurn)
       {
           setPath(true);
           firstTurn = false;
       }

       if(!seekMode)
           detectTresure();

       agentObject.turnToFace(nextNodeToGoal.Translation);  // adjust to face nextGoal every move
       // See if at or close to nextGoal, distance measured in 2D xz plane
       
       float distanceToGoal = Vector3.Distance(
           new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
           new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));

       float distanceToNextNode = Vector3.Distance(
           new Vector3(nextNodeToGoal.Translation.X, 0, nextNodeToGoal.Translation.Z),
           new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
       
       stage.setInfo(15,
          string.Format("npAvatar:  location ({0:f0}, {1:f0}, {2:f0})  looking at ({3:f2}, {4:f2}, {5:f2})",
             agentObject.Translation.X, agentObject.Translation.Y, agentObject.Translation.Z,
             agentObject.Forward.X, agentObject.Forward.Y, agentObject.Forward.Z));
       stage.setInfo(16,
             string.Format("npAvatar:  nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                 nextGoal.Translation.X, nextGoal.Translation.Y, nextGoal.Translation.Z, distanceToGoal));

       if (distanceToNextNode <= snapDistance)
       {
           if (!pathToGoal.Done)
               nextNodeToGoal = pathToGoal.NextNode;
           else
           {
                if (seekMode)
                {
                    seekMode = false;
                    nextGoal = storedGoal;
                    setPath(false);
                }
                else
                {
                    setPath(true);
                }
           }  
       }

       base.Update(gameTime);  // Agent's Update();
   }

    private void detectTresure()
   {
       for (int i = 0; i < stage.treasures.total; i++)
           if (stage.treasures.active[i])
               if (distance(stage.treasures.Instance[i].Translation, AgentObject.Translation) < 4000)
                  TreasureMode();
   }

   public void TreasureMode()
   {
       int tempi = -1;
       int tempd = Int32.MaxValue;

       for (int i = 0; i < stage.treasures.total; i++)
       {
           if (stage.treasures.active[i])
           {
               if (distance(stage.treasures.Instance[i].Translation, AgentObject.Translation) < tempd)
               {
                   tempi = i;
                   tempd = (int)distance(stage.treasures.Instance[i].Translation, AgentObject.Translation);
               }
           }
       }

       if (tempi != -1)
       {
           TreasureSeek(stage.treasures.Instance[tempi].Translation);
       }

   }

   private float distance(Vector3 m1, Vector3 m2)
   {
       return (float)Math.Sqrt(((m1.X - m2.X) * (m1.X - m2.X)) + ((m1.Z - m2.Z) * (m1.Z - m2.Z)));
   }


  } 
}
