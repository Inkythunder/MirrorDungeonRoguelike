using UnityEngine;

namespace Roguelike.Game
{
    public class EdgeMarker : MonoBehaviour
    {
        [SerializeField] Camera camera;
        [SerializeField] RectTransform arrow;
        
        [Header("Tuning")]
        [Tooltip("How far in from the edge the arrow sits. (in px)")]
        [SerializeField] int edge_padding = 48;

        [Tooltip("Extra rotation to make the art line up. -90 = point up.")] [SerializeField]
        private float sprite_angle_offset = -90f;
        
        // The cell we're pointing at.
        public Vector2Int target;
        
        // Nothing to point at until Show() is called
        private bool has_target;
        
        // The rect that anchoredPosition is measured against.
        private RectTransform parent_rect;
        
        // Overlay canvases want a null camera when converting screen points
        Camera canvas_camera;

        void Awake()
        {
            parent_rect = arrow.parent as RectTransform;

            Canvas canvas = arrow.GetComponentInParent<Canvas>();
            canvas_camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            
            arrow.gameObject.SetActive(false);
        }

        public void Show(Vector2Int cell)
        {
            target = cell;
            has_target = true;
        }

        public void Hide()
        {
            has_target = false;
            arrow.gameObject.SetActive(false);
        }
        
        // LateUpdate so the camera has already moved this frame.
        // Avoids lag.
        void LateUpdate()
        {
            if (!has_target) return;
            
            // Makes the arrow aim at the middle of the tile.
            Vector3 world = EntityView.CellToWorld(target);
            Vector2 screen_point = camera.WorldToScreenPoint(world);

            Vector2 centre = new Vector2(Screen.width, Screen.height) * 0.5f;
            
            // The box that the arrow sits in
            Vector2 half = centre - Vector2.one * edge_padding;
            
            // Target is already on screen = hide arrow
            if (Mathf.Abs(screen_point.x - centre.x) < half.x &&
                Mathf.Abs(screen_point.y - centre.y) < half.y)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            arrow.gameObject.SetActive(true);

            Vector2 direction = screen_point - centre;

            // Push the direction out until it hits an edge of the box the arrow sits in.
            float scale = Mathf.Min(
                half.x / Mathf.Abs(direction.x),
                half.y / Mathf.Abs(direction.y)
            );
            Vector2 edge_point = centre + direction * scale;
            
            // Screen pixels = the canvas's own coordinates so this can be scaled.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent_rect, edge_point, canvas_camera, out Vector2 local))
            {
                arrow.anchoredPosition = local;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.localEulerAngles = new Vector3(0f, 0f, angle + sprite_angle_offset);
        }
    }
}