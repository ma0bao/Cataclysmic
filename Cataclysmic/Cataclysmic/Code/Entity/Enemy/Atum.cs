using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Atum : Enemy
    {

        Player target;

        const int WIDTH = 150;
        const int HEIGHT = 250;

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

        //Bossbar
        const float TIME_TO_DAMAGE = 1.75f;
        int damageToApply;
        float damageIncrement = 1;
        EventTimer damageWindow;
        Rectangle staticBarRect;
        Rectangle HealthbarRect;
        Rectangle currentHealthRect => new Rectangle(
                                        HealthbarRect.X, 
                                        HealthbarRect.Y, 
                                        (int)(HealthbarRect.Width * healthData.lerpValue),
                                        HealthbarRect.Height);
        Vector2 barShake = new Vector2(1.5f, 1.5f);
        EventTimer shakeTimer = new EventTimer(.2f);
        float currentIntensity;
        float shakeIntensity = 10f;

        AttackStates nextAttackState;
        

        
        public Atum(Vector2 position) : base(Game1.texture_Atum, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), WIDTH, HEIGHT)
        {
            target = Game1.player;
            healthData = new HealthComponent(250);

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
            staggerResistance = 0.0f;

            bloodData.baseSize = 16;

            //Health Bar
            HealthbarRect = new Rectangle(Game1.BOUNDS.X, 50, Game1.BOUNDS.Width, 50);
            damageWindow = new EventTimer(TIME_TO_DAMAGE);
            staticBarRect = HealthbarRect;
        }

        public override void DrawEx(float opacity)
        {
            Game1.self.spriteBatch.Draw(Game1.texture_square, HealthbarRect, Color.DarkRed);
            Game1.self.spriteBatch.Draw(Game1.texture_square, currentHealthRect, Color.Orange);
            if (damageToApply > 0)
                Game1.self.spriteBatch.Draw(Game1.texture_blank, GetCurrentDamageRect(), Color.White);

            base.DrawEx(opacity);
        }

        public Rectangle GetCurrentDamageRect()
        {
            Rectangle rect = new Rectangle(currentHealthRect.Right, HealthbarRect.Y, 0, HealthbarRect.Height);
            rect.Width = (int) ((damageToApply / (float)healthData.maxHealth) * HealthbarRect.Width);
            return rect;
        }

        public override void Damage(Entity cause, int amount, BloodHit bloodHit)
        {
            if (!healthData.invincible)
            {
                damageWindow.Restart();
                damageToApply += amount;
                shakeTimer.Restart();
                currentIntensity = shakeIntensity;
            }
            base.Damage(cause, amount, bloodHit);
        }

        public override void Stagger(float secondsToStagger, bool UseResistance = true)
        {
            if (currentState == AttackStates.Follow || currentState == AttackStates.Teleport) currentState = AttackStates.Center;
            base.Stagger(secondsToStagger, UseResistance);
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
            UpdateTimers();
            damageWindow.Update();

            if (damageWindow.Done && damageToApply > 0)
            {
                damageIncrement += 0.5f;
                damageToApply -= (int)damageIncrement;
            }
            else
                damageIncrement = 1;

            if (shakeTimer.IsRunning())
            {
                shakeTimer.Update();

                barShake = new Vector2(
                    (float)(Game1.rand.NextDouble() * 2 - 1) * currentIntensity,
                    (float)(Game1.rand.NextDouble() * 2 - 1) * currentIntensity);
                currentIntensity = MathHelper.Lerp(shakeIntensity, 0, shakeTimer.lerpValue);

                HealthbarRect.X += (int)barShake.X;
                HealthbarRect.Y += (int)barShake.Y;

                if (shakeTimer.Done)
                    HealthbarRect = staticBarRect;
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
                if (Game1.KB.IsKeyDown(Keys.K) && !Game1.oldKB.IsKeyDown(Keys.K))
                {
                    RenderComponent render = new RenderComponent(Game1.texture_SunFire, new Rectangle(400, 400, 100, 100));
                    render.color.A = 50;
                    Game1.visuals.Add(new Visual(render, 5f));
                }

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
                        SetStateToGeyser(50, 0.02f);
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
            Game1.sfx_boom.Play(Game1.volume, Game1.rand.NextFloat(), 0);
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
                projectiles.Add(new WaveShot(Game1.texture_SunFire, renderData.Position, rotatedVelocity));
            }
        }

        public void FireCircle(float amt = 40)
        {
            Game1.sfx_boom.Play(Game1.volume, Game1.rand.NextFloat(), 0);
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
                projectiles.Add(new WaveShot(Game1.texture_SunFire, renderData.Position, rotatedVelocity, new Point(40, 40)));
            }
        }

        public void FireSpread(float amt = 1)
        {
            Game1.sfx_boom.Play(Game1.volume, Game1.rand.NextFloat(), 0);
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
                projectiles.Add(new WaveShot(Game1.texture_SunFire, renderData.Position, rotatedVelocity));
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
        float damageTimeLerp = .8f;
        bool isLight = true;

        public EventTimer life = new EventTimer(3);

        public Geyser(Rectangle destRect)
        {
            renderData = new RenderComponent(Game1.texture_YellowCircle, new Rectangle(destRect.X, destRect.Y, 2, 2));
            collisionData = CollisionComponent.CreateCircle(renderData.Position, 20);

            maxWidth = destRect.Width;
            maxHeight = destRect.Height;
        }

        public void Draw() {
            renderData.DefualtDraw(); 
            collisionData.DrawDebug();  
        }

        public void Update(GameTime gameTime) 
        {
            if (isLight)
            {
                renderData.SetWidth((int)(life.lerpValue * maxWidth));
                renderData.SetHeight((int)(life.lerpValue * maxHeight));
                renderData.color.A = (byte)(255 * life.lerpValue);
                if (life.lerpValue > damageTimeLerp)
                {
                    isLight = false;
                    Game1.sfx_fireIgnite.Play(Game1.volume*0.5f, (float)Game1.rand.NextDouble(), 0);
                    renderData = new RenderComponent(Game1.texture_SunFire, renderData.DestRect);
                }
            }


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

        float SPEED = 400;


        public WaveShot(Texture2D texture, Vector2 pos, Vector2 direction)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * SPEED;
            renderData = new RenderComponent(texture, new Rectangle((int)pos.X, (int)pos.Y, 50, 50));
            collisionData = CollisionComponent.CreateRect(pos, 30, 30);
        }

        public WaveShot(Vector2 pos, Vector2 direction)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * SPEED;
            renderData = new RenderComponent(Game1.texture_player, new Rectangle((int)pos.X, (int)pos.Y, 50, 50));
            collisionData = CollisionComponent.CreateRect(pos, 30, 30);
        }

        public WaveShot(Vector2 pos, Vector2 direction, float speed)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * speed;
            renderData = new RenderComponent(Game1.texture_player, new Rectangle((int)pos.X, (int)pos.Y, 50, 50));
            collisionData = CollisionComponent.CreateRect(pos, 30, 30);
            SPEED = speed;
        }

        public WaveShot(Vector2 pos, Vector2 direction, float speed, Texture2D texture, Point scale)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * speed;
            renderData = new RenderComponent(texture, new Rectangle((int)pos.X, (int)pos.Y, scale.X, scale.Y));
            collisionData = CollisionComponent.CreateRect(pos, scale.X, scale.Y);
            SPEED = speed;
        }

        public WaveShot(Texture2D texture, Vector2 pos, Vector2 direction, Point scale)
        {
            moveData = new MoveComponent();
            moveData.velocity = direction * SPEED;
            renderData = new RenderComponent(texture, new Rectangle((int)pos.X, (int)pos.Y, scale.X, scale.Y));
            collisionData = CollisionComponent.CreateRect(pos, scale.X, scale.Y);
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
            collisionData.DrawDebug();
            renderData.DefualtDraw();
        }
    }
}
