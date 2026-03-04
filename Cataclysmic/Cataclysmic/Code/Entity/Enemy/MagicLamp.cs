using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class MagicLamp : Enemy
    {
        class Sand
        {
            EventTimer death = new EventTimer(5);
            RenderComponent renderData;
            MoveComponent moveData;
            Vector2 randomOffset;
            const int MAX_OFFSET = 3;
            const int SIZE = 5;

            public Sand(float frictionMultiplier, Vector2 Position)
            {
                renderData = new RenderComponent(Game1.texture_blank, new Rectangle((int)Position.X, (int)Position.Y, SIZE, SIZE));
                moveData = new MoveComponent(300, 2000, 100*frictionMultiplier);
            }

            public void IsColliding() //Will take environment as a parameter
            {
                throw new NotImplementedException();
            }

            public void OnCollision(Entity cause) 
            {
                throw new NotImplementedException();
            }

            public void Update()
            {
                renderData.color.A = (byte)MathHelper.Lerp(255, 0, death.lerpValue);
                randomOffset.X = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                randomOffset.Y = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                if(moveData.velocity.Length() > 3)
                    moveData.velocity += randomOffset;

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
            }
        }

        enum AttackState
        {
            Wander = 0,
            Follow = 1,
            Charge = 2,
            Spray = 3,
            Run = 4
        }

        Player player; //Make Array of players for proper targeting

        AttackState currentState = AttackState.Wander;
        public override void Update(GameTime gameTime)
        {
            if (currentState == AttackState.Wander)
            {
                targetPos = renderData.GetRandomPoint();
            }
            else if (currentState == AttackState.Follow)
            {
                
            }

            base.Update(gameTime);
        }

        public override void Draw(float opacity)
        {
            base.Draw(opacity);
        }
    }
}
