using UnityEngine;

namespace Src.Frontend
{
    public class PlayerMove : MonoBehaviour
    {
        private Transform _bodyTransform;
        private Transform _cameraTransform;
    
        public float speed = 10.0f;
    
        private void Awake()
        {
            _bodyTransform = GetComponent<Transform>();
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
            else Debug.LogError("No main camera found");
        }

        private readonly bool[] _keys = new bool[5];
        private void Update()
        {
            _keys[0] = Input.GetKey(KeyCode.W);
            _keys[1] = Input.GetKey(KeyCode.A);
            _keys[2] = Input.GetKey(KeyCode.S);
            _keys[3] = Input.GetKey(KeyCode.D);
            _keys[4] = Input.GetKey(KeyCode.Space);
        }
    
        private void FixedUpdate()
        {
            var x = 0.0f;
            var y = 0.0f;
            var z = 0.0f;
        
            if (_keys[0])
            {
                z = 1.0f;
            }
            if (_keys[1])
            {
                x = -1.0f;
            }
            if (_keys[2])
            {
                z = -1.0f;
            }
            if (_keys[3])
            {
                x = 1.0f;
            }

            if (_keys[4])
            {
                y = 1.0f;
            }
            
            var move = new Vector3(x, y, z);
            if (_keys[4])
            {
                move += Vector3.up * (speed * Time.deltaTime);
            }
        
            _bodyTransform.Translate(move * (speed * Time.deltaTime), _cameraTransform);
        }
    }
}
