using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class HealthComponent
    {
        public int maxHealth;
        public int currentHealth;
        public bool isAlive;
        public float lerpValue => (float)currentHealth / maxHealth;

        // For when health is the same as maxhealth
        public HealthComponent(int _maxHealth)
        {
            maxHealth = _maxHealth;
            currentHealth = _maxHealth;
            isAlive = true;
        }

        //for if you want health to be different from maxhealth
        public HealthComponent(int _maxHealth, int _currentHealth)
        {
            maxHealth = _maxHealth;
            currentHealth = _currentHealth;
            isAlive = true;
        }

        //Entity has a damge thing. do you want to do it here?
        public bool Damage(Entity cause, int _damage)
        {
            if (currentHealth - _damage > 0)
            {
                currentHealth -= _damage;
                return true;
            }

            if (currentHealth - _damage <= 0)
            {
                currentHealth = 0;
                isAlive = false;

            }
            return false;
        }


    }
}
