using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace BSLedShow.Utils
{
    public class DelayedCoroutine
    {
        Action exitfn;
        float limit;
        public DelayedCoroutine(Action exitfn, float limit = 0.5f)
        {
            this.exitfn = exitfn;
            this.limit = limit;
        }

        bool wasRecentlyExecuted = false;
        bool queuedFallingEdge = false;

        public IEnumerator Call()
        {
            if (!wasRecentlyExecuted)
            {
                wasRecentlyExecuted = true;
                yield return CallNow();
            }
            else
            {
                queuedFallingEdge = true;
            }
        }

        public IEnumerator CallNextFrame()
        {
            yield return null;
            yield return Call();
        }

        IEnumerator CallNow()
        {
            exitfn();

            yield return new WaitForSeconds(limit);

            if (queuedFallingEdge)
            {
                queuedFallingEdge = false;
                yield return CallNow();
            }
            else
            {
                wasRecentlyExecuted = false;
            }
        }
    }
}
