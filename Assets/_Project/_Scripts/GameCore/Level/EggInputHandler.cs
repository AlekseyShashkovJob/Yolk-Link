using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Misc.Services;

namespace GameCore.Level
{
    public class EggInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public EggGridManager gridManager;
        public Core.GameManager gameManager;
        public GraphicRaycaster raycaster;

        public GameObject lineImagePrefab;
        public Sprite[] lineSprites;
        public Transform lineLayerParent;

        private List<GameObject> activeLineImages = new List<GameObject>();

        private List<Egg> selectedEggs = new List<Egg>();
        private EggColor? currentColor = null;
        private bool isDragging = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            foreach (var e in selectedEggs)
                e.SetHighlighted(false);

            selectedEggs.Clear();
            currentColor = null;
            TrySelectEggUnderPointer(eventData);
            UpdateChainLines();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            TrySelectEggUnderPointer(eventData);
            UpdateChainLines();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            TrySelectEggUnderPointer(eventData);

            if (selectedEggs.Count >= 2)
            {
                foreach (var e in selectedEggs)
                    e.SetHighlighted(false);

                gameManager.CollectEggs(new List<Egg>(selectedEggs));
            }
            else
            {
                foreach (var e in selectedEggs)
                    e.SetHighlighted(false);
            }

            selectedEggs.Clear();
            currentColor = null;
            UpdateChainLines();
        }

        private void TrySelectEggUnderPointer(PointerEventData eventData)
        {
            Egg egg = RaycastEgg(eventData);
            if (egg == null) return;

            if (selectedEggs.Count >= 2 && selectedEggs[selectedEggs.Count - 2] == egg)
            {
                var last = selectedEggs[selectedEggs.Count - 1];
                last.SetHighlighted(false);

                selectedEggs.RemoveAt(selectedEggs.Count - 1);
                UpdateChainLines();
                return;
            }

            if (selectedEggs.Contains(egg))
                return;

            if (selectedEggs.Count == 0)
            {
                currentColor = egg.color;
                selectedEggs.Add(egg);

                VibroManager.Vibrate();
                egg.SetHighlighted(true);
            }
            else
            {
                Egg lastEgg = selectedEggs[selectedEggs.Count - 1];
                if (egg.color == currentColor && IsAdjacent(egg, lastEgg))
                {
                    selectedEggs.Add(egg);

                    VibroManager.Vibrate();
                    egg.SetHighlighted(true);
                }
            }
            UpdateChainLines();
        }

        private bool IsAdjacent(Egg a, Egg b)
        {
            var diff = a.gridPosition - b.gridPosition;
            return (Mathf.Abs(diff.x) == 1 && diff.y == 0) || (Mathf.Abs(diff.y) == 1 && diff.x == 0);
        }

        private Egg RaycastEgg(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            foreach (var r in results)
            {
                var egg = r.gameObject.GetComponent<Egg>();
                if (egg != null)
                    return egg;
            }
            return null;
        }

        private void UpdateChainLines()
        {
            foreach (var go in activeLineImages)
            {
                if (go != null)
                    Destroy(go);
            }
            activeLineImages.Clear();

            if (selectedEggs.Count < 2)
                return;

            int colorIndex = currentColor.HasValue ? (int)currentColor.Value : 0;
            Sprite lineSprite = lineSprites.Length > colorIndex ? lineSprites[colorIndex] : null;

            for (int i = 0; i < selectedEggs.Count - 1; i++)
            {
                Egg a = selectedEggs[i];
                Egg b = selectedEggs[i + 1];

                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;

                GameObject lineGO = Instantiate(lineImagePrefab, lineLayerParent != null ? lineLayerParent : a.transform.parent);
                Image img = lineGO.GetComponent<Image>();
                if (img != null && lineSprite != null)
                    img.sprite = lineSprite;

                RectTransform rt = lineGO.GetComponent<RectTransform>();
                rt.position = (posA + posB) / 2f;

                Vector2 dir = (posB - posA).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                rt.rotation = Quaternion.Euler(0, 0, angle);

                float dist = Vector2.Distance(posA, posB);
                rt.sizeDelta = new Vector2(dist, rt.sizeDelta.y);

                activeLineImages.Add(lineGO);
            }
        }
    }
}