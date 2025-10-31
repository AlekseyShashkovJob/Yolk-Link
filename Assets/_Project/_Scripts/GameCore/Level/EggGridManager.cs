using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Level
{
    public class EggGridManager : MonoBehaviour
    {
        public int width = 8;
        public int height = 8;
        public GameObject eggPrefab;
        public Transform gridParent;

        public Sprite[] eggSprites;
        public Sprite[] eggSpritesHighlighted;

        public EggInputHandler eggInputHandler;

        private Egg[,] eggs;

        private void Start()
        {
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            eggs = new Egg[width, height];
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var eggGO = Instantiate(eggPrefab, gridParent);
                eggGO.SetActive(true);
                Egg egg = eggGO.GetComponent<Egg>();
                var color = (EggColor)Random.Range(0, eggSprites.Length);

                Sprite normal = eggSprites[(int)color];
                Sprite highlighted = (eggSpritesHighlighted != null && eggSpritesHighlighted.Length > (int)color)
                    ? eggSpritesHighlighted[(int)color]
                    : eggSprites[(int)color]; // fallback

                egg.SetColor(color, normal, highlighted);
                egg.SetGridPosition(new Vector2Int(x, y));
                eggs[x, y] = egg;
            }
        }

        // При "удалении" яиц (например, при сборе) мы просто меняем цвет/спрайты на новые.
        public void RemoveEggs(List<Egg> toRemove)
        {
            foreach (var egg in toRemove)
            {
                var color = (EggColor)Random.Range(0, eggSprites.Length);
                Sprite normal = eggSprites[(int)color];
                Sprite highlighted = (eggSpritesHighlighted != null && eggSpritesHighlighted.Length > (int)color)
                    ? eggSpritesHighlighted[(int)color]
                    : eggSprites[(int)color];

                egg.SetColor(color, normal, highlighted);
                egg.SetHighlighted(false);
            }
        }

        public void CollapseAndRefill()
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (eggs[x, y] != null)
                    eggs[x, y].SetGridPosition(new Vector2Int(x, y));
            }
        }
    }

    public enum EggColor
    {
        Red,
        Green,
        Blue,
        Yellow,
        Orange,
        Purple,
        Pink
    }
}