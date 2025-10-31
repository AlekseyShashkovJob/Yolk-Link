using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameCore.Level;

namespace GameCore.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        [SerializeField] private View.UIScreen _winScreen;
        [SerializeField] private View.UIScreen _loseScreen;
        [SerializeField] private Misc.SceneManagment.SceneLoader _sceneLoader;
        [SerializeField] private TMP_Text _greenText;
        [SerializeField] private TMP_Text _yellowText;
        [SerializeField] private TMP_Text _purpleText;
        [SerializeField] private TMP_Text _pinkText;
        [SerializeField] private TMP_Text _orangeText;
        [SerializeField] private TMP_Text _blueText;
        [SerializeField] private EggGridManager _eggGridManager;
        [SerializeField] private LevelLoader _levelLoader;

        private int[] eggGoals;
        private int[] eggCollected;
        private int moves = 0;
        private bool isGameFinished = false;

        private int CurrentLevelIndex => _levelLoader != null ? _levelLoader.CurrentLevel : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadPerLevelBestForCurrentOrDefault();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            Time.timeScale = 1.0f;
            StartLevel(CurrentLevelIndex);
        }

        public void StartLevel(int levelIndex)
        {
            CurrentScore = 0;
            moves = 0;
            isGameFinished = false;

            if (_levelLoader != null)
                _levelLoader.LoadLevel(levelIndex);

            int enumColorCount = Enum.GetNames(typeof(Level.EggColor)).Length;
            int spriteColorCount = (_eggGridManager != null && _eggGridManager.eggSprites != null) ? _eggGridManager.eggSprites.Length : enumColorCount;
            int colorCount = Mathf.Max(1, Mathf.Min(enumColorCount, spriteColorCount));

            eggGoals = new int[colorCount];
            eggCollected = new int[colorCount];

            int goalPerColor = Mathf.Clamp(levelIndex + 2, 1, 999);
            for (int i = 0; i < colorCount; i++)
                eggGoals[i] = goalPerColor;

            BestScore = PlayerPrefs.GetInt($"{GameConstants.LEVEL_BEST_SCORE_KEY}_{CurrentLevelIndex}", 0);

            if (_eggGridManager != null)
            {
                _eggGridManager.GenerateGrid();
            }

            UpdateScoreUI();
        }

        public void CollectEggs(List<Level.Egg> eggs)
        {
            if (isGameFinished) return;
            if (eggs == null || eggs.Count < 2) return;

            moves++;

            EnsureArraysInitialized();

            int[] collectPerColor = new int[eggCollected.Length];
            for (int i = 0; i < eggs.Count; i++)
            {
                int idx = (int)eggs[i].color;
                if (idx < 0 || idx >= collectPerColor.Length) continue;
                collectPerColor[idx]++;
            }

            for (int i = 0; i < eggCollected.Length; i++)
            {
                eggCollected[i] += collectPerColor[i];
                if (eggCollected[i] > eggGoals[i])
                    eggCollected[i] = eggGoals[i];
            }

            int scoringEggs = 0;
            for (int i = 0; i < eggs.Count; i++)
            {
                int idx = (int)eggs[i].color;
                if (idx < 0 || idx >= eggCollected.Length) continue;
                if (eggCollected[idx] - collectPerColor[idx] < eggGoals[idx])
                    scoringEggs++;
            }

            int comboBonus = scoringEggs >= 5 ? Mathf.RoundToInt(10 * (scoringEggs - 4) * 0.8f) : 0;
            int baseScore = Mathf.RoundToInt(scoringEggs * 10 * 0.8f);
            int moveModifier = Mathf.Max(1, 20 - moves);

            AddScore(baseScore * moveModifier + comboBonus);

            if (_eggGridManager != null)
            {
                _eggGridManager.RemoveEggs(eggs);
                _eggGridManager.CollapseAndRefill();
            }

            UpdateScoreUI();

            Debug.Log($"eggCollected: {StringifyArray(eggCollected)}, eggGoals: {StringifyArray(eggGoals)}");

            if (IsWin())
            {
                FinishGame();
            }
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            if (CurrentScore > BestScore)
                BestScore = CurrentScore;

            SaveBestForCurrentLevel();
            UpdateScoreUI();
        }

        public void RestartGame()
        {
            SaveBestForCurrentLevel();
            _sceneLoader.ChangeScene(Misc.Data.SceneConstants.GAME_SCENE);
        }

        public void FinishGame()
        {
            isGameFinished = true;
            Time.timeScale = 0.0f;

            if (CurrentScore > BestScore)
                BestScore = CurrentScore;

            SaveBestForCurrentLevel();
            UnlockNextLevel(CurrentLevelIndex);

            _winScreen?.StartScreen();

            CurrentScore = 0;
        }

        public void LoseGame()
        {
            isGameFinished = true;
            Time.timeScale = 0.0f;

            if (CurrentScore > BestScore)
                BestScore = CurrentScore;

            SaveBestForCurrentLevel();
            _loseScreen?.StartScreen();

            CurrentScore = 0;
        }

        public void UpdateScoreUI()
        {
            if (eggCollected == null || eggGoals == null) return;

            if (_greenText != null && eggCollected.Length > 0) _greenText.text = $"{eggCollected[0]}/{eggGoals[0]}";
            if (_yellowText != null && eggCollected.Length > 1) _yellowText.text = $"{eggCollected[1]}/{eggGoals[1]}";
            if (_purpleText != null && eggCollected.Length > 2) _purpleText.text = $"{eggCollected[2]}/{eggGoals[2]}";
            if (_pinkText != null && eggCollected.Length > 3) _pinkText.text = $"{eggCollected[3]}/{eggGoals[3]}";
            if (_orangeText != null && eggCollected.Length > 4) _orangeText.text = $"{eggCollected[4]}/{eggGoals[4]}";
            if (_blueText != null && eggCollected.Length > 5) _blueText.text = $"{eggCollected[5]}/{eggGoals[5]}";
        }

        // Возвращает ссылку на LevelLoader (для экранов UI)
        public LevelLoader GetLevelLoader() => _levelLoader;

        public void OnNextLevel()
        {
            int next = CurrentLevelIndex + 1;
            int total = _levelLoader != null ? _levelLoader.TotalLevels() : 9;
            if (next >= total)
            {
                FinishGame();
            }
            else
            {
                StartLevel(next);
            }
        }

        private void SaveBestForCurrentLevel()
        {
            string key = $"{GameConstants.LEVEL_BEST_SCORE_KEY}_{CurrentLevelIndex}";
            int saved = PlayerPrefs.GetInt(key, 0);
            if (BestScore > saved)
            {
                PlayerPrefs.SetInt(key, BestScore);
                PlayerPrefs.Save();
            }
        }

        private void UnlockNextLevel(int currentLevel)
        {
            int lastUnlocked = PlayerPrefs.GetInt(GameConstants.LAST_UNLOCKED_LEVEL_KEY, 0);
            if (currentLevel >= lastUnlocked)
            {
                int next = currentLevel + 1;
                int total = _levelLoader != null ? _levelLoader.TotalLevels() : 9;
                if (next < total)
                {
                    PlayerPrefs.SetInt(GameConstants.LAST_UNLOCKED_LEVEL_KEY, next);
                    PlayerPrefs.Save();
                }
            }
        }

        private void LoadPerLevelBestForCurrentOrDefault()
        {
            int selected = PlayerPrefs.GetInt(GameConstants.LAST_SELECTED_LEVEL_KEY, 0);
            BestScore = PlayerPrefs.GetInt($"{GameConstants.LEVEL_BEST_SCORE_KEY}_{selected}", 0);
        }

        private bool IsWin()
        {
            if (eggCollected == null || eggGoals == null) return false;
            int len = Mathf.Min(eggCollected.Length, eggGoals.Length);
            for (int i = 0; i < len; i++)
            {
                if (eggCollected[i] < eggGoals[i])
                    return false;
            }
            return true;
        }

        private void EnsureArraysInitialized()
        {
            if (eggGoals != null && eggCollected != null) return;

            int enumColorCount = Enum.GetNames(typeof(EggColor)).Length;
            int spriteColorCount = (_eggGridManager != null && _eggGridManager.eggSprites != null) ? _eggGridManager.eggSprites.Length : enumColorCount;
            int colorCount = Mathf.Max(1, Mathf.Min(enumColorCount, spriteColorCount));

            eggGoals = new int[colorCount];
            eggCollected = new int[colorCount];
            int selectedLevel = _levelLoader != null ? _levelLoader.CurrentLevel : 0;
            int goalPerColor = Mathf.Clamp(selectedLevel + 2, 1, 999);
            for (int i = 0; i < colorCount; i++)
                eggGoals[i] = goalPerColor;
        }

        private string StringifyArray(int[] arr)
        {
            if (arr == null) return "null";
            return "[" + string.Join(", ", Array.ConvertAll(arr, x => x.ToString())) + "]";
        }
    }
}