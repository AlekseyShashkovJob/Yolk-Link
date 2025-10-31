using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Level
{
    [RequireComponent(typeof(Image))]
    public class Egg : MonoBehaviour
    {
        public EggColor color;
        public Image eggImage;
        [HideInInspector] public Vector2Int gridPosition;

        private Sprite normalSprite;
        private Sprite highlightedSprite;

        public void SetColor(EggColor col, Sprite normal, Sprite highlighted)
        {
            color = col;
            if (eggImage == null)
                eggImage = GetComponent<Image>();

            normalSprite = normal;
            highlightedSprite = highlighted;

            eggImage.sprite = normalSprite;
        }

        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (eggImage == null)
                eggImage = GetComponent<Image>();

            eggImage.sprite = highlighted ? (highlightedSprite ?? normalSprite) : (normalSprite ?? highlightedSprite);
        }
    }
}