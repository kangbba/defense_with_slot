using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BattleField : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CellManager _cellManager;
    [SerializeField] private BattleFieldPath _path;
    [SerializeField] private EnemyManager _enemyManager;

    [Header("Enemy Spawn Rule")]
    [SerializeField, Min(0.05f)] private float _spawnInterval = 0.5f;

    private CancellationTokenSource _spawnCts;

    public CellManager CellManager => _cellManager;
    public BattleFieldPath Path => _path;
    public EnemyManager EnemyManager => _enemyManager;

    private void OnDisable() => StopEnemySpawn();
    private void OnDestroy() => StopEnemySpawn();

    public void Init(int width, int height)
    {
        _cellManager.Init(width, height);
    }

    public void StartEnemySpawn()
    {
        StopEnemySpawn();
        _spawnCts = new CancellationTokenSource();
        SpawnLoopAsync(_spawnCts.Token).Forget();
    }

    public void StopEnemySpawn()
    {
        if (_spawnCts == null) return;
        if (!_spawnCts.IsCancellationRequested) _spawnCts.Cancel();
        _spawnCts.Dispose();
        _spawnCts = null;
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(
            () => _path != null && _path.GetPath()?.Count >= 2,
            cancellationToken: token);

        var delay = System.TimeSpan.FromSeconds(_spawnInterval);
        while (!token.IsCancellationRequested)
        {
            _enemyManager.SpawnEnemy(this);
            await UniTask.Delay(delay, cancellationToken: token);
        }
    }
}
