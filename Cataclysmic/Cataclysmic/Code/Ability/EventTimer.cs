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
            duration = (int)(seconds * 60);
            countdown = duration;
            paused = false;
            if (duration <= 0)
                Done = true;
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
