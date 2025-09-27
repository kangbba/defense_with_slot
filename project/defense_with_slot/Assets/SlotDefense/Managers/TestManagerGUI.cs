using UnityEngine;

public class TestManagerGUI : MonoBehaviour
{
    private bool _showUI = true;
    private Rect _windowRect;

    private void Start()
    {
        // ✅ 화면 좌측 하단 기준 배치 (폭 200, 높이 500)
        float width = 200f;
        float height = 700f;
        _windowRect = new Rect(10, Screen.height - height - 10, width, height);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, Screen.height - 60, 120, 50), _showUI ? "Hide UI" : "Show UI"))
        {
            _showUI = !_showUI;
        }

        if (_showUI)
        {
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, "Hero Spawner");
        }
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUI.skin.button.fontSize = 20; // ✅ 글씨 크게
        var buttonOption = GUILayout.Height(70); // ✅ 세로 길쭉하게
        var buttonWidth = GUILayout.Width(160);  // ✅ 가로는 짧게

        if (GUILayout.Button("🔥 Fire", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Fire);

        if (GUILayout.Button("🌿 Element", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Element);

        if (GUILayout.Button("⚡ Lightning", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Lightning);

        if (GUILayout.Button("❄ Ice", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Ice);

        if (GUILayout.Button("☠ Poison", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Poison);

        if (GUILayout.Button("🪨 Rock", buttonOption, buttonWidth))
            SpawnSpecificHero(HeroType.Rock);

        if (GUILayout.Button("🎲 Random", buttonOption, buttonWidth))
            OnSpawnRandomHeroClicked();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }

    private void SpawnSpecificHero(HeroType type)
    {
        var bf = FieldManager.Instance.CurBattleField;
        var cell = bf?.CellManager?.GetRandomEmptyCell();
        if (cell == null) return;

        HeroManager.Instance.SpawnHeroAtCell(type, 1, cell);
    }

    private void OnSpawnRandomHeroClicked()
    {
        HeroManager.Instance.MakeRandomHeroOnRandomCell();
    }
}
