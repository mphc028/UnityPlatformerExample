using UnityEngine;

namespace Platformer2DSystem
{
    public interface IRaycastFilter
    {
        bool IsFiltering(RaycastHit2D hit, Vector2 direction);
    }
}
