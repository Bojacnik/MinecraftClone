using System;
using UnityEngine;

namespace Src.Frontend
{
    public class CamFollowBody : MonoBehaviour
    {
        [SerializeField]
        private Transform parentTransform;

        private void FixedUpdate()
        {
            transform.position = parentTransform.position;
        }
    }
}
