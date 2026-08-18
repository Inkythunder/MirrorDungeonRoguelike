using UnityEngine;

namespace Roguelike.Game
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Camera camera;
        [SerializeField] float smooth_time = 0.12f;

        Transform target;
        Vector3 velocity;

        int map_width;
        int map_height;
        bool has_bounds;
        
        void Awake()
        {
            if (camera == null) camera = GetComponent<Camera>();
        }

        public void SetTarget(Transform target) => this.target = target;

        public void SetMapBounds(int width, int height)
        {
            map_width = width;
            map_height = height;
            has_bounds = true;
        }

        // Camera following in Update causes visible 1-frame lag.
        // LateUpdate runs after every Update instead.
        private void LateUpdate()
        {
            if (target == null || camera == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

            if (has_bounds)
            {
                float half_height = camera.orthographicSize;
                float half_width = half_height * camera.aspect;

                // If map is narrower than the view, centre it instead of clamping.
                desired.x = map_width >= half_width * 2
                    ? Mathf.Clamp(desired.x, half_width, map_width - half_width) : map_width / 2f;
                
                desired.y = map_height >= half_height * 2
                    ? Mathf.Clamp(desired.y, half_height, map_height - half_height) : map_height / 2f;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref velocity, smooth_time);
        }
    }
}