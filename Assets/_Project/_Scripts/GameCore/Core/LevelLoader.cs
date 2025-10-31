using UnityEngine;

namespace GameCore.Core
{
    // Простой загрузчик уровня: хранит текущий уровень, общее количество уровней и сохраняет последний выбранный
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private int _totalLevels = 9;
        [SerializeField] private int _currentLevel = 0;

        public int CurrentLevel => _currentLevel;

        private void Awake()
        {
            // Если нужно взять последний выбранный при старте сцены
            _currentLevel = PlayerPrefs.GetInt(GameConstants.LAST_SELECTED_LEVEL_KEY, 0);
            _currentLevel = Mathf.Clamp(_currentLevel, 0, Mathf.Max(0, _totalLevels - 1));
        }

        public int TotalLevels()
        {
            return Mathf.Max(0, _totalLevels);
        }

        public void LoadLevel(int levelIndex)
        {
            _currentLevel = Mathf.Clamp(levelIndex, 0, Mathf.Max(0, _totalLevels - 1));
            // Сохраняем последний выбранный
            PlayerPrefs.SetInt(GameConstants.LAST_SELECTED_LEVEL_KEY, _currentLevel);
            PlayerPrefs.Save();
        }

        // Проверка — есть ли следующий уровень
        public bool HasNextLevel()
        {
            return _currentLevel + 1 < _totalLevels;
        }
    }
}