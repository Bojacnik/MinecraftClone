using UnityEngine;

namespace Src.Frontend
{
    public class PlayerCamera : MonoBehaviour
    {
        private Transform _parentTransform;
        public float smoothSpeed = 0.125f;
        public Vector3 offset;
        public float mouseSensitivity = 100f;
        private float xRotation = 0f;
        private float yRotation = 0f;

        private void Awake()
        {
            _parentTransform = GetComponentInParent<Transform>();
            offset = transform.position - _parentTransform.position;
            Cursor.lockState = CursorLockMode.Locked; // Optionally lock the cursor to the center of the screen
        }

        private const float lowerTreshold = -90f;
        private const float upperTreshold = 90f;
        private void Update()
        {
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation += mouseY;
            yRotation += mouseX;
            
            var xQuat = Quaternion.AngleAxis(yRotation, Vector3.up);
            var yQuat = Quaternion.AngleAxis(xRotation, Vector3.left);

            transform.localRotation = xQuat * yQuat;
            
            /*
            if (yRotation < lowerTreshold)
            {
                _parentTransform.Rotate
            }
            */
            
            //yRotation = Mathf.Clamp(yRotation, lowerTreshold, upperTreshold);

            if (Input.GetKeyDown(KeyCode.B))
            {
                transform.rotation = Quaternion.Inverse(transform.rotation);
            }
            
            // Adjust the camera offset based on user input
            // Optional: Clamp the offset values to prevent the camera from moving too far from the player
        }
    }
}
