using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIMindSystem
{
    public abstract class Goal_Base
    {
        public abstract float calculateUrgency();
        public abstract float getUrgency();
        public abstract string getName();
        public abstract void setParent(Goal_Base parent);
        public abstract Goal_Base getParent();
    }
}
