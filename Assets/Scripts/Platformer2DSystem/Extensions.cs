using UnityEngine;

namespace Platformer2DSystem
{
    public static class Extensions
    {
        public static ContactFilter2D ToContactFilter(this LayerMask mask)
        {
            ContactFilter2D filter = new();
            filter.SetLayerMask(mask);
            return filter;
        }
    }
}
