using UnityEngine;

public class TestManagerGUI : MonoBehaviour
{
    private bool _showUI = true;
    private Rect _windowRect;

    private readonly float _windowWidth = 220f;  // ✅ 고정 크기
    private readonly float _windowHeight = 600f;

    private void Start()
    {
        // 좌측 하단에 고정 배치
        _windowRect = new Rect(10, Screen.height - _windowHeight - 10, _windowWidth, _windowHeight);
    }

    private void OnGUI()
    {
        // Show / Hide 버튼
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
        GUI.skin.button.fontSize = 18; 

        GUILayout.BeginVertical();

        // 버튼 높이 통일 (윈도우 높이 나누기 개수)
        float buttonHeight = (_windowHeight - 40f) / 7f; // 7개 버튼 기준
        var buttonOption = GUILayout.Height(buttonHeight);

        if (GUILayout.Button("🔥 Fire", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Fire);

        if (GUILayout.Button("🌿 Element", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Element);

        if (GUILayout.Button("⚡ Lightning", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Lightning);

        if (GUILayout.Button("❄ Ice", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Ice);

        if (GUILayout.Button("☠ Poison", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Poison);

        if (GUILayout.Button("🪨 Rock", buttonOption, GUILayout.ExpandWidth(true)))
            SpawnSpecificHero(HeroType.Rock);

        if (GUILayout.Button("🎲 Random", buttonOption, GUILayout.ExpandWidth(true)))
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
