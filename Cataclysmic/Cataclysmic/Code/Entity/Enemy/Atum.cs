using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Atum : Enemy
    {

        Player target;

        //States
        enum AttackStates
        {
            Center,
            Teleport,
            Geyser,
            Flame,
            Follow,
            Cooldown,
            Swing
        }

        AttackStates currentState;
        EventTimer cooldownTimer;

        //Geyser Ability
        EventTimer geyserCooldown;
        int geyserAmount;
        Queue<Geyser> geysers;



        AttackStates nextAttackState;

        
        public Atum(Rectangle destRect, Player player) : base(Game1.texture_player, destRect)
        {
            target = player;

            //Geyser
            geyserCooldown = new EventTimer(.2f);
            geyserAmount = 75;
            geysers = new Queue<Geyser>(geyserAmount);
            //geyserCooldown.Loop(true);

            SetStateToGeyser(75);
        }

        private void SetStateToCenter()
        {
            currentState = AttackStates.Center;
        }

        private void SetStateToCooldown(float time, AttackStates nextState)
        {
            cooldownTimer = new EventTimer(time);
            nextAttackState = nextState;
            currentState = AttackStates.Cooldown;
        }

        public void SetStateToGeyser(int amt = 75, float time = .04f)
        {
            geyserCooldown.Restart(time);
            geyserAmount = amt;
            currentState = AttackStates.Geyser;
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State
            if (currentState == AttackStates.Center)
            {
                SetNewTargetPosition(Game1.BOUNDS.Center);
            }
            #endregion

            #region Update Based On State
            if (currentState == AttackStates.Center)
            {
                base.Update(gameTime);
                if (Game1.rand.Next(0, 1000) == 0) //Replace with good cooldown code
                    SetStateToGeyser();
            }
            else if (currentState == AttackStates.Cooldown)
            {
                if (cooldownTimer.Done)
                    currentState = nextAttackState; //Replace to call methods. Switch case 
                cooldownTimer.Update();
            }
            else if (currentState == AttackStates.Geyser)
            {
                if (geyserCooldown.Done)
                {
                    geyserCooldown.Restart();
                    CreateGeyser();
                    geyserAmount--;
                }

                if (geyserAmount == 0)
                    SetStateToCenter();

                geyserCooldown.Update();
            }
            #endregion

            foreach (Geyser g in geysers)
                g.Update(gameTime);

            while (geysers.Count > 0 && !geysers.Peek().IsAlive())
            {
                geysers.Dequeue();
            }
        }

        public void CreateGeyser()
        {
            bool valid = true;

            Rectangle rect = new Rectangle((int)renderData.GetRandomPoint().X, (int)renderData.GetRandomPoint().Y, 60, 60);
            Geyser g = new Geyser(rect);

            foreach (Geyser geyser in geysers)
            {
                if (g.collisionData.Intersects(geyser.collisionData, out var _, out var _))
                {
                    valid = false;
                    break;
                }

            }
            if (valid)
                geysers.Enqueue(g);
            else
                CreateGeyser();
        }

        public override void Draw(float opacity)
        {
            foreach (Geyser g in geysers)
                g.Draw();

            base.Draw(opacity);
        }
    }

    class Geyser
    {
        RenderComponent renderData;
        public CollisionComponent collisionData;
        int maxWidth;
        int maxHeight;

        EventTimer life = new EventTimer(3);

        public Geyser(Rectangle destRect)
        {
            renderData = new RenderComponent(Game1.texture_player, new Rectangle(destRect.X, destRect.Y, 2, 2));
            collisionData = CollisionComponent.CreateCircle(renderData.Position, 30);

            maxWidth = destRect.Width;
            maxHeight = destRect.Height;
        }

        public void Draw() { renderData.DefualtDraw(); collisionData.DrawDebug();  }

        public void Update(GameTime gameTime) 
        {
            renderData.SetWidth((int) (life.lerpValue * maxWidth));
            renderData.SetHeight((int) (life.lerpValue * maxHeight));
            life.Update();
        }

        public bool IsAlive()
        {
            return !life.Done;
        }
    }
}
