using UnityEngine;
using System.Collections.Generic;

public interface IEnemyProvider
{
    List<Enemy> ActiveEnemies { get; }

    Enemy FindNearest(Vector3 from, float range);
    Enemy FindRandomInRange(Vector3 from, float range);
    Enemy LockOnOne(Vector3 from, float range, Enemy currentTarget);
}
