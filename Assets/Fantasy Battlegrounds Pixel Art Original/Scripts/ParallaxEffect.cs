using UnityEngine;

namespace FantasyBattlegroundsPixelArtOriginal
{
    public class ParallaxEffect : MonoBehaviour
    {
        private Transform mainCamera;
        private Transform player;

        public float parallaxIntensityX;
        public float parallaxIntensityY;
        public float independantSpeed;

        private float cameraSize;
        private float spriteWidth;
        private Vector2 initialPos;
        private float translationOffset = 0;

        private void Start()
        {
            // SAFE CAMERA LOOKUP: Try Camera.main first, fallback to FindFirstObjectByType
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = FindFirstObjectByType<Camera>();
            }

            if (cam != null)
            {
                mainCamera = cam.transform;
                cameraSize = cam.orthographicSize;
            }
            else
            {
                Debug.LogError("ParallaxEffect: No Camera found in scene!");
                return;
            }

            // SAFE PLAYER LOOKUP: Try tag "Player" first, fallback to finding PlayerMovement script
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null) playerObj = pm.gameObject;
            }

            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("ParallaxEffect: No Player found in scene!");
                return;
            }

            // SAFE SPRITE RENDERER CHECK
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                spriteWidth = sr.bounds.size.x / 3;
            }

            transform.position = new Vector2(mainCamera.position.x, player.position.y - 1f);
            initialPos = transform.position;
        }

        private void LateUpdate()
        {
            // Don't execute movement if references are missing
            if (mainCamera == null || player == null) return;

            translationOffset += independantSpeed * Time.deltaTime * parallaxIntensityX;

            float parallaxOffsetX = (mainCamera.position.x * (1 - (parallaxIntensityX / 2))) + translationOffset;
            float parallaxOffsetY = ((mainCamera.position.y / cameraSize) / 0.7f) * (1 - parallaxIntensityY);

            transform.position = new Vector2(initialPos.x + parallaxOffsetX, initialPos.y + parallaxOffsetY);

            float cameraOffsetX = mainCamera.position.x - transform.position.x;

            if (cameraOffsetX > spriteWidth / 2)
                initialPos.x += spriteWidth;
            else if (cameraOffsetX < -spriteWidth / 2)
                initialPos.x -= spriteWidth;
        }
    }
}