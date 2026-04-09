using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class Sun : Enemy
    {
        const   int WIDTH = 150;
        const   int HEIGHT = 150;


        public enum  AttackState {
            Wander,
            Spray,
            Circle,
        }
        
        Player target;

        AttackState  currentState;
        Rectangle half
        {
            get {
                if (target.renderData.Position.X < Game1.BOUNDS.Center.X)
                    return new Rectangle(); //Finsih
                }
        }

        public Sun(Vector2 position) : base(Game1.texture_player, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), WIDTH, HEIGHT)
        {
            healthData = new HealthComponent(200);
            target = Game1.player;
        }

        public void SetStateToWander()
        {
            SetNewTargetPosition(renderData.GetRandomPoint(half));
            currentState = AttackState.Wander;
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State
            if (currentState == AttackState.Wander)
            {
                if(IsAtTarget() || targetPos == Vector2.Zero)
                    SetNewTargetPosition(renderData.GetRandomPoint(half));
            }
            #endregion
        }
    }

    class Sand
    {
        EventTimer death = new EventTimer(8);
        RenderComponent renderData;
        MoveComponent moveData;
        CollisionComponent sandHitbox;
        Vector2 randomOffset;
        const int MAX_OFFSET = 10;
        const int SIZE = 5;

        public Sand(float frictionMultiplier, Vector2 Position, Vector2 velocity)
        {
            sandHitbox = CollisionComponent.CreateRect(Position, SIZE, SIZE);
            renderData = new RenderComponent(Game1.texture_blank, new Rectangle((int)Position.X, (int)Position.Y, SIZE, SIZE));
            moveData = new MoveComponent(300, 2000, 150 * frictionMultiplier);
            renderData.color = new Color(119, 96, 66);
            moveData.velocity = velocity;
            randomOffset.X = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
            randomOffset.Y = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
            if (moveData.velocity.Length() > 10)
                moveData.velocity += randomOffset;
        }

        public void IsColliding() //Will take environment as a parameter
        {
            throw new NotImplementedException();
        }

        public void OnCollision(Entity cause)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            renderData.color.A = (byte)MathHelper.Lerp(255, 200, death.lerpValue);
            sandHitbox.UpdatePosition(renderData.Position);
            float depth;
            Vector2 normal;
            if (sandHitbox.Intersects(Game1.player.Hitbox, out depth, out normal))
            {
                Game1.player.Damage(null, 5);
            }

            renderData.Position += moveData.velocity * moveData.deltaTime;
            moveData.ApplyFriction();

            death.Update();
        }

        public bool IsAlive()
        {
            return !death.Done;
        }

        public void Draw()
        {
            renderData.DefualtDraw();
            sandHitbox.DrawDebug();
        }
    }
}
