//Maksim Sadovkiy - maksim.sadovskiy.658@my.csun.edu
//Comp 565 Project 1 - Terrain/AGMGSK

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AGMGSKv6
{
    public class Treasure : Model3D
    {

        protected Model model0 = null;
        protected Model model1 = null;
//        public bool active = true;

        public List<bool> active;
        public List<Point> locations;
        public int total;

        public Treasure(Stage theStage, string label, string fileOfModel0, string fileOfModel1)
            : base(theStage, label, fileOfModel0)
        {
            model0 = stage.Content.Load<Model>(fileOfModel0);
            model1 = stage.Content.Load<Model>(fileOfModel1);

            active = new List<bool>();
            locations = new List<Point>();
            total = 0;
        }

        public new Object3D addObject(Vector3 position, Vector3 orientAxis, float radians)
        {
            bool temp = true;
            active.Add(temp);
            total++;
            locations.Add(new Point((int)position.X/150, (int)position.Z/150));
            return base.addObject(position, orientAxis, radians);
        }

        public override void  Draw(GameTime gameTime)
        {
            int i = 0;
            Matrix[] modelTransforms0 = new Matrix[model0.Bones.Count];
            Matrix[] modelTransforms1 = new Matrix[model1.Bones.Count];

            foreach (Object3D obj3d in instance)
            {
                if (active[i])
                    DrawHelper(model0, gameTime, modelTransforms0, obj3d);
                else
                    DrawHelper(model1, gameTime, modelTransforms1, obj3d);
                i++;

                
            }
        }

        public void capture(int i)
        {
            active[i] = false;
        }
    }

}