using UnityEngine;

namespace Roguelike.Core
{
    public enum ActionResult
    {
        // For illegal actions
        Blocked,
        
        Moved,
        Attacked,
        Waited,
        Drank,
        Pushed
    }
}

