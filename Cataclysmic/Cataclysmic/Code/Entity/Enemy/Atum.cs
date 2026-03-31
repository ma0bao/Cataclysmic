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

        //Center
        EventTimer AttackTickTimer;

        //Geyser Ability
        EventTimer geyserCooldown;
        int geyserAmount;
        Queue<Geyser> geysers;

        //Teleport Ability
        const int TELEPORT_MAX_DISTANCE = 200;
        EventTimer TeleportTimer;
        RenderComponent teleportPosOne, teleportPosTwo;

        //Attack Ability
        const int ATTACK_RANGE = 100;
        IWeapon weapon;


        AttackStates nextAttackState;

        
        public Atum(Rectangle destRect, Player player) : base(Game1.texture_player, destRect)
        {
            target = player;

            //Geyser
            geyserCooldown = new EventTimer(.2f);
            geyserAmount = 75;
            geysers = new Queue<Geyser>(geyserAmount);
            //geyserCooldown.Loop(true);

            //Teleport
            TeleportTimer = new EventTimer(2f);
            TeleportTimer.Done = true;

            SetStateToGeyser(75);
        }

        private void SetStateToCenter()
        {
            currentState = AttackStates.Center;
        }

        private void SetStateToCooldown(float time, AttackStates nextState)
        {
            renderData.color = Color.Red;
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

        public void SetStateToFollow()
        {
            moveData.maxSpeed = 500f;
            turnSpeed = 2000f;
            currentState = AttackStates.Follow;
        }

        public void SetStateToSwing()
        {
            weapon = new Scythe(renderData.Position);
          
            weapon.Attack();
            SetStateToCooldown(.8f, AttackStates.Center);
        }

        public void Teleport()
        {
            teleportPosOne = new RenderComponent(Game1.self.texture_firePortal, new Rectangle((int)renderData.Position.X, (int)renderData.Position.Y, 50, 50));
            Vector2 newPos = target.renderData.Position;
            newPos.X += Game1.rand.Next(-TELEPORT_MAX_DISTANCE, TELEPORT_MAX_DISTANCE);
            newPos.Y += Game1.rand.Next(-TELEPORT_MAX_DISTANCE, TELEPORT_MAX_DISTANCE);
            renderData.Position = newPos;
            teleportPosTwo = new RenderComponent(Game1.self.texture_firePortal, new Rectangle((int)renderData.Position.X, (int)renderData.Position.Y, 50, 50));
            collision.Update(renderData.Position, renderData.rotation);

            TeleportTimer.Restart();
            SetStateToCooldown(.8f, AttackStates.Follow);   
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State
            if (currentState == AttackStates.Center)
            {
                SetNewTargetPosition(Game1.BOUNDS.Center);
            }
            else if (currentState == AttackStates.Follow)
            {
                SetNewTargetPosition(target.renderData.Position);
            }
            #endregion

            #region Update Based On State
            if (currentState == AttackStates.Center)
            {
                base.Update(gameTime);
                if (AttackTickTimer == null)
                    AttackTickTimer = new EventTimer(Game1.rand.Next(2, 8));

                if (AttackTickTimer.Done)
                {
                    int randChoice = Game1.rand.Next(10);
                    if (randChoice < 7) // 0, 1, 3, 4, 5, 6
                        SetStateToGeyser();
                    else if (randChoice < 9) //7, 8
                        Teleport();
                    AttackTickTimer.Restart(Game1.rand.Next(2, 8));
                }

                AttackTickTimer.Update();
            }
            else if (currentState == AttackStates.Cooldown)
            {
                if (cooldownTimer.Done)
                    SwitchState(nextAttackState);
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
            else if (currentState == AttackStates.Follow)
            {
                if (renderData.GetDistanceToTarget(targetPos) <= ATTACK_RANGE)
                    SetStateToSwing();
                base.Update(gameTime);
            }
            #endregion

            foreach (Geyser g in geysers)
                g.Update(gameTime);

            while (geysers.Count > 0 && !geysers.Peek().IsAlive())
            {
                geysers.Dequeue();
            }

            if (weapon != null)
            {
                weapon.Update(gameTime);
                if (weapon.IsDone())
                    weapon = null;
            }
        }

        private void SwitchState(AttackStates state)
        {
            switch (state)
            {
                case AttackStates.Center:
                    SetStateToCenter();
                    break;
                case AttackStates.Geyser:
                    SetStateToGeyser();
                    break;
                case AttackStates.Follow:
                    SetStateToFollow();
                    break;
                default:
                    throw new NotImplementedException();
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

            if (!TeleportTimer.Done)
            {
                teleportPosOne.DefualtDraw();
                teleportPosTwo.DefualtDraw();
                teleportPosOne.rotation+= 10;
                teleportPosTwo.rotation+= 10;
                TeleportTimer.Update();
            }

            if (weapon != null)
                weapon.Draw();

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

    interface IWeapon
    {
        void Attack();
        void Update(GameTime gameTime);
        void Draw();
        bool IsDone();
    }

    class Scythe : IWeapon
    {
        RenderComponent renderData;
        CollisionComponent hitbox;
        bool isAttacking;

        public Scythe(Vector2 pos)
        {
            renderData = new RenderComponent(Game1.texture_player, new Rectangle((int)pos.X, (int)pos.Y, 100, 250));
            hitbox = CollisionComponent.CreateRect(renderData.Position, renderData.DestRect.Width, renderData.DestRect.Height);
        }
        public void Attack()
        {
            isAttacking = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!isAttacking)
                return;
           renderData.rotation += 15;
        }

        public void Draw()
        {
            renderData.DefualtDraw();
        }

        public bool IsDone()
        {
            return renderData.rotation > 360;
        }
    }
}
