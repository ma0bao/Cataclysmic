using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class ManaComponent
    {
        public float maxMana;
        public float currentMana;
        public float lerpValue => (float)currentMana / maxMana;


        // For when health is the same as maxhealth
        public ManaComponent(int _maxMana)
        {
            maxMana = _maxMana;
            currentMana = _maxMana;
        }

        //for if you want health to be different from maxhealth
        public ManaComponent(int _maxMana, int _currentMana)
        {
            maxMana = _maxMana;
            currentMana = _currentMana;
        }

        public void Add(float amount)
        {
            currentMana += amount;
            if (currentMana > maxMana)
            {
                currentMana = maxMana;
            }
        }

        public void Decrease(float amount)
        {
            currentMana -= amount;
        }

        public void SetCurrent(float newCurrent)
        {
            currentMana = newCurrent;
        }

        public void SetMax(float newMax)
        {
            maxMana = newMax;
        }

        public void Set(float newMax, float newCurrent)
        {
            currentMana = newCurrent;
            newMax = newMax;
        }

        public void Update()
        {

        }
        

    }
}
