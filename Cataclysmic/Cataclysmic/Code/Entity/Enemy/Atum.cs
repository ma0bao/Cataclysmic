using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        //Projectiles
        List<IProjectile> projectiles = new List<IProjectile>();
        EventTimer fireTimer;

        AttackStates nextAttackState;

        
        public Atum(Rectangle destRect, Player player) : base(Game1.texture_player, destRect, destRect.Width, destRect.Height)
        {
            target = player;
            healthData = new HealthComponent(100);

            //Geyser
            geyserCooldown = new EventTimer(.2f);
            geyserAmount = 75;
            geysers = new Queue<Geyser>(geyserAmount);
            //geyserCooldown.Loop(true);

            //Teleport
            TeleportTimer = new EventTimer(2f);
            TeleportTimer.Done = true;

            //Projectiles
            fireTimer = new EventTimer();
            fireTimer.Done = true;

            SetStateToCenter();
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
            teleportPosOne = new RenderComponent(Game1.texture_firePortal, new Rectangle((int)renderData.Position.X, (int)renderData.Position.Y, 50, 50));
            Vector2 newPos = target.renderData.Position;
            newPos.X += Game1.rand.Next(-TELEPORT_MAX_DISTANCE, TELEPORT_MAX_DISTANCE);
            newPos.Y += Game1.rand.Next(-TELEPORT_MAX_DISTANCE, TELEPORT_MAX_DISTANCE);
            renderData.Position = newPos;
            teleportPosTwo = new RenderComponent(Game1.texture_firePortal, new Rectangle((int)renderData.Position.X, (int)renderData.Position.Y, 50, 50));
            collision.Update(renderData.Position, renderData.rotation);

            TeleportTimer.Restart();
            SetStateToCooldown(.8f, AttackStates.Follow);   
        }

        public override void Update(GameTime gameTime)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                FireWave();
            }

            #region Get Target Based On State
            if (currentState == AttackStates.Center)
            {
                //SetNewTargetPosition(Game1.BOUNDS.Center);
                float time = (float)gameTime.TotalGameTime.TotalSeconds;

                Vector2 center = new Vector2(Game1.BOUNDS.Center.X, Game1.BOUNDS.Center.Y);

                Vector2 offset = new Vector2(
                    (float)Math.Sin(time * 2f) * 120f,
                    (float)Math.Cos(time * 1.5f) * 80f
                );

                SetNewTargetPosition(center + offset);
            }
            else if (currentState == AttackStates.Follow)
            {
                SetNewTargetPosition(target.renderData.Position);
            }
            else if (currentState == AttackStates.Geyser)
            {
                SetNewTargetPosition(Game1.BOUNDS.Center);
            }
            #endregion

            #region Update Based On State
            if (currentState == AttackStates.Center || currentState == AttackStates.Geyser)
            {
                renderData.color = Color.Lerp(Color.White, Color.Red, (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3f) * 0.5f + 0.5f);

                base.Update(gameTime);
                if (AttackTickTimer == null)
                    AttackTickTimer = new EventTimer(Game1.rand.Next(2, 8));

                if (AttackTickTimer.Done)
                {
                    int randChoice = Game1.rand.Next(10);
                    if (randChoice == 0)
                        SetStateToGeyser();
                    else if (randChoice == 1)
                        Teleport();
                    else if (randChoice == 2)
                        SetStateToGeyser(100);
                    else if (randChoice == 3)
                        FireWave(10);
                    else if (randChoice == 4)
                        FireWave(12);
                    else if (randChoice == 5)
                        FireCircle(30);
                    else if (randChoice == 6)
                        FireCircle(40);
                    else if (randChoice == 7)
                        fireTimer.Restart(.2f);
                    else if (randChoice == 8)
                        fireTimer.Restart(.4f);
                    else if (randChoice == 9)
                        fireTimer.Restart(.6f);
                    AttackTickTimer.Restart(Game1.rand.Next(1, 4));
                }

                AttackTickTimer.Update();
            }
            else if (currentState == AttackStates.Cooldown)
            {
                if (cooldownTimer.Done)
                    SwitchState(nextAttackState);
                cooldownTimer.Update();
            }
            else if (currentState == AttackStates.Follow)
            {
                if (renderData.GetDistanceToTarget(targetPos) <= ATTACK_RANGE)
                    SetStateToSwing();
                base.Update(gameTime);
            }

            if (currentState == AttackStates.Geyser)
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
            {
                g.Update(gameTime);
                if (g.life.lerpValue > .8f)
                {
                    if (g.collisionData.Intersects(target.Hitbox, out _, out _))
                        target.Damage(this, 5); 
                }
            }

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

            foreach (IProjectile p in projectiles)
            {
                p.Update(gameTime);
                if (p is WaveShot w)
                {
                    if (w.collisionData.Intersects(target.Hitbox, out _, out _))
                        target.Damage(this, 3);
                }
            }

            projectiles.RemoveAll(p => !p.IsAlive());

            if (!fireTimer.Done)
            {
                FireSpread();
                fireTimer.Update();
            }
        }

        public void FireWave(float amt = 10)
        {
            for (float i = 0; i < amt; i++)
            {
                Vector2 direction = renderData.GetDirectionToTarget(target.renderData.Position);

                if (direction.LengthSquared() > 0)
                    direction.Normalize();

                float coneSize = MathHelper.ToRadians(180);

                float randomAngle = (float)(i / amt * coneSize) - (coneSize / 2f);
                //float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    direction.X * cos - direction.Y * sin,
                    direction.X * sin + direction.Y * cos
                    );
                projectiles.Add(new WaveShot(renderData.Position, rotatedVelocity));
            }
        }

        public void FireCircle(float amt = 50)
        {
            for (float i = 0; i < amt; i++)
            {
                Vector2 direction = renderData.GetDirectionToTarget(target.renderData.Position);

                if (direction.LengthSquared() > 0)
                    direction.Normalize();

                float coneSize = MathHelper.ToRadians(360);

                float randomAngle = (float)(i / amt * coneSize) - (coneSize / 2f);
                //float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    direction.X * cos - direction.Y * sin,
                    direction.X * sin + direction.Y * cos
                    );
                projectiles.Add(new WaveShot(renderData.Position, rotatedVelocity));
            }
        }

        public void FireSpread(float amt = 1)
        {
            for (float i = 0; i < amt; i++)
            {
                Vector2 direction = renderData.GetDirectionToTarget(target.renderData.Position);

                if (direction.LengthSquared() > 0)
                    direction.Normalize();

                float coneSize = MathHelper.ToRadians(120);

                float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);
                //float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    direction.X * cos - direction.Y * sin,
                    direction.X * sin + direction.Y * cos
                    );
                projectiles.Add(new WaveShot(renderData.Position, rotatedVelocity));
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

            foreach (IProjectile p in projectiles)
            {
                p.Draw();
                if (p is WaveShot w)
                    w.collisionData.DrawDebug();
            }
            

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

        public EventTimer life = new EventTimer(3);

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

    interface IProjectile
    {
        bool IsAlive();
        void Update(GameTime gameTime);
        void Draw();
    }

    class WaveShot : IProjectile
    {
        public RenderComponent renderData;
        public MoveComponent moveData;
        public CollisionComponent collisionData;

        const float SPEED = 400;

        public WaveShot(Vector2 pos, Vector2 direction)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * SPEED;
            renderData = new RenderComponent(Game1.texture_player, new Rectangle((int)pos.X, (int)pos.Y, 50, 50));
            collisionData = CollisionComponent.CreateRect(pos, 30, 30);
        }

        public bool IsAlive()
        {
            return renderData.IsOnScreen();
        }

        public void Update(GameTime gameTime)
        {
            renderData.Position += moveData.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            collisionData.Update(renderData.Position, renderData.rotation);
        }

        public void Draw()
        {
            renderData.DefualtDraw();
        }
    }
}
