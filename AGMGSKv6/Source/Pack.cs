/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file Pack.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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
/// Pack represents a "flock" of MovableObject3D's Object3Ds.
/// Usually the "player" is the leader and is set in the Stage's LoadContent().
/// With no leader, determine a "virtual leader" from the flock's members.
/// Model3D's inherited List<Object3D> instance holds all members of the pack.
/// 
/// 2/1/2015 last changed
/// </summary>
public class Pack : MovableModel3D {   
   Object3D leader;
   public int packPercent;
   private double packChance;

/// <summary>
/// Construct a pack with an Object3D leader
/// </summary>
/// <param name="theStage"> the scene </param>
/// <param name="label"> name of pack</param>
/// <param name="meshFile"> model of a pack instance</param>
/// <param name="xPos, zPos">  approximate position of the pack </param>
/// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
   public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
      : base(theStage, label, meshFile) {
          packPercent = 0;
          packChance = 0;
       isCollidable = true;
		random = new Random();
      leader = theLeader;
		int spacing = stage.Spacing;
		// initial vertex offset of dogs around (xPos, zPos)
        int[,] position = { { -5, -5 }, { -5, 0 }, { -5, 5 }, { 0, -5 }, { 0, 0 }, { 0, 5 }, { 5, -5 }, { 5, 0 }, { 5, 5 } };
		for( int i = 0; i < position.GetLength(0); i++) {
			int x = xPos + position[i, 0];
			int z = zPos + position[i, 1];
			float scale = (float)(0.5 + random.NextDouble());
			addObject( new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
						  new Vector3(0, 1, 0), 0.0f,
						  new Vector3(scale, scale, scale));
			}
      }

   /// <summary>
   /// Each pack member's orientation matrix will be updated.
   /// Distribution has pack of dogs moving randomly.  
   /// Supports leaderless and leader based "flocking" 
   /// </summary>      
   public override void Update(GameTime gameTime) {
      // if (leader == null) need to determine "virtual leader from members"
       float angle;
      foreach (Object3D obj in instance) {
          obj.Yaw = 0;
          if (random.NextDouble() < 0.07)
          {
              if (random.NextDouble() < packChance)
              {
                  angle = 0.1f;
                  if (packPacking(obj))
                      obj.Yaw += angle;
                  else
                      obj.Yaw -= angle;
              }
              else
              {
                  angle = 0.3f;
                  if (random.NextDouble() < 0.5)
                      obj.Yaw -= angle; // turn left
                  else
                      obj.Yaw += angle; // turn right
              }
          }
          

             
         obj.updateMovableObject();
         stage.setSurfaceHeight(obj);
         }
      base.Update(gameTime);  // MovableMesh's Update(); 
      }


   public Object3D Leader {
      get { return leader; }
      set { leader = value; }}

   public void toggle()
   {
       if (packPercent == 0)
           packPercent = 33;
       else if (packPercent == 33)
           packPercent = 66;
       else if (packPercent == 66)
           packPercent = 99;
       else if (packPercent == 99)
           packPercent = 0;
       else
           packPercent = 0;

       packChance = packPercent / 100.0;
   }

    private bool packPacking( Object3D OO )
   {

       Vector3 packSeparationTemp = new Vector3(0,0,0);
       foreach (Object3D obj in instance) {
           if(OO.Translation != obj.Translation)
               packSeparationTemp += (OO.Translation - obj.Translation) * (1 / Vector3.Distance(OO.Translation, obj.Translation));
       }

       Vector3 separationTemp = OO.Translation - stage.player.AgentObject.Translation;
       Vector3 alignmentTemp = stage.player.AgentObject.Forward;
       Vector3 cohesionTemp = stage.player.AgentObject.Translation - OO.Translation;

       Vector3 packSeparation = Vector3.Normalize(new Vector3(packSeparationTemp.X, 0, packSeparationTemp.Z));
       Vector3 separation = Vector3.Normalize(new Vector3(separationTemp.X, 0, separationTemp.Z));
       Vector3 alignment = Vector3.Normalize(new Vector3(alignmentTemp.X, 0, alignmentTemp.Z));
       Vector3 cohesion = Vector3.Normalize(new Vector3(cohesionTemp.X, 0, cohesionTemp.Z));

       Vector3 packVector;
        
       float S, A, C;
       float P = 0.5f;

       float ansF;
       bool ansB;

       double dis = Vector3.Distance(OO.Translation, stage.player.AgentObject.Translation);

        if(dis <= 400)
        {
            S = 1;
            A = 0;
            C = 0;
        }
        else if( dis < 1000)
        {
            S = 1 - ((float)dis - 400) / 600;
            A = ((float)dis - 400) / 600;
            C = 0;
        }
        else if( dis <= 2000)
        {
            S = 0;
            A = 1;
            C = 0;
        }
        else if (dis < 3000)
        {
            S = 0;
            A = 1 - ((float)dis - 2000) / 1000;
            C = ((float)dis - 2000) / 1000;
        }
        else
        {
            S = 0;
            A = 0;
            C = 1;
        }

        packVector = (S * separation) + (A * alignment) + (C * cohesion) + (P * packSeparation);

        float f1 = Vector3.Dot(Vector3.Normalize(OO.Forward), Vector3.Normalize(packVector));
        Vector3 v1 = Vector3.Cross(Vector3.Normalize(OO.Forward), Vector3.Normalize(packVector));

        if (f1 > 1)
            f1 = 1;
        if (f1 < -1)
            f1 = -1;

        if(v1.Y > 0)
            ansF = (float)Math.Acos(f1);
        else
            ansF = -(float)Math.Acos(f1);

        if (ansF > 0)
            ansB = true;
        else
            ansB = false;

        return ansB;
   }

   }
}
