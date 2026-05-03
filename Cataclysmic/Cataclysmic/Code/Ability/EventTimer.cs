using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class EventTimer
    {
        private bool loop = false;
        private int duration;
        private int countdown;
        private bool paused;

        /// <summary>
        /// Gets a value that smoothly goes from 0.0 - 1.0f
        /// </summary>
        public float lerpValue => 1f - (countdown / (float)duration);


        public bool Done;


        public EventTimer()
        {
            duration = 0;
            countdown = 0;
            Done = true;
        }

        public EventTimer(float seconds)
        {
            duration = (int)(seconds * 60.0);
            countdown = duration;
            paused = false;
            if (duration <= 0)
                Done = true;
        }

        /// <summary>
        /// Gets a number that smoothly goes from 0 to maxValue
        /// </summary>
        /// <param name="maxValue"> The maximum value that can be returned </param>
        /// <returns>Value from 0 to maxValue based on the timer</returns>
        public float GetInput(float maxValue)
        {
            return (1f - (countdown / (float)duration)) * maxValue;
        }

        public int GetTime()
        {
            return countdown;
        }

        public int GetDuration()
        {
            return duration;
        }

        public void Update()
        {
            if (paused)
                return;

            Done = false;


            countdown--;
            if (countdown <= 0)
            {
                Done = true;
                if (!loop)
                    paused = true;
                else
                    countdown = duration;
            }
        }

        public void SetTime(float seconds)
        {
            duration = (int)(seconds * 60);
        }

        public void Unpause()
        {
            paused = false;
        }

        public void Pause()
        {
            paused = true;
        }

        public bool IsRunning()
        {
            return !paused;
        }

        public void Reset()
        {
            countdown = duration;
            paused = true;
            Done = false;
        }

        public void Restart()
        {
            countdown = duration;
            paused = false;
            Done = false;
        }

        public void Restart(float seconds)
        {
            duration = (int)(seconds * 60);
            countdown = duration;
            Done = false;
            paused = false;
        }

        public void Loop(bool t)
        {
            loop = t;
        }
    }
}
