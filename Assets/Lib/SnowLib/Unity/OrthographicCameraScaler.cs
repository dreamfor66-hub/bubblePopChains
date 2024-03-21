using System;
using UnityEngine;

namespace SnowLib.Unity
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class OrthographicCameraScaler : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private int referenceWidth;
        [SerializeField] private int referenceHeight;
        [SerializeField] private float ySize;

        void Reset()
        {
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                ySize = cam.orthographicSize;
            }
        }

        private int prevWidth;
        private int prevHeight;

        // Start is called before the first frame update
        void Start()
        {
            UpdateSize();
        }

        void Update()
        {
            if (prevWidth != Screen.width || prevHeight != Screen.height)
                UpdateSize();
        }

        void UpdateSize()
        {
            if (cam == null)
                throw new Exception("null camera");
            if (!cam.orthographic)
                throw new Exception("camera is not orthographic");


            var screenAspectRatio = (float)Screen.width / Screen.height;
            var referenceAspectRatio = (float)referenceWidth / referenceHeight;

            if (screenAspectRatio >= referenceAspectRatio)
                cam.orthographicSize = ySize;
            else
                cam.orthographicSize = ySize * referenceAspectRatio / screenAspectRatio;

            prevWidth = Screen.width;
            prevWidth = Screen.height;
        }
    }
}
