/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;

namespace Vuforia {
    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class RecognizerScript : MonoBehaviour,
                                                ITrackableEventHandler
    {
        public Renderer backgroundRenderer;
        public Material targetMat;
        public Transform[] corners;

        #region PRIVATE_MEMBER_VARIABLES

        private TrackableBehaviour mTrackableBehaviour;
        private bool _isActive;
        private bool _isSetup;
        private Texture2D backgroundTex;
        private Vector2 _webcam2ScreenScale;
        private Vector3[] _translateUV;

        private int _lastValue;
        private int _sameValueTime;

        #endregion // PRIVATE_MEMBER_VARIABLES



        #region UNTIY_MONOBEHAVIOUR_METHODS

        void Start() {
            mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            _isActive = false;
            _isSetup = false;
            _translateUV = new Vector3[4];
            if (mTrackableBehaviour) {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }

        #endregion // UNTIY_MONOBEHAVIOUR_METHODS



        #region PUBLIC_METHODS

        /// <summary>
        /// Implementation of the ITrackableEventHandler function called when the
        /// tracking state changes.
        /// </summary>
        public void OnTrackableStateChanged(
                                        TrackableBehaviour.Status previousStatus,
                                        TrackableBehaviour.Status newStatus) {
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) {
                OnTrackingFound();
            } else {
                OnTrackingLost();
            }
        }

        #endregion // PUBLIC_METHODS



        #region PRIVATE_METHODS


        private void OnTrackingFound()
        {
            _isActive = true;
            _lastValue = -1;


            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Enable rendering:
            foreach (Renderer component in rendererComponents) {
                component.enabled = true;
            }

            // Enable colliders:
            foreach (Collider component in colliderComponents) {
                component.enabled = true;
            }

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
        }


        private void OnTrackingLost() {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Disable rendering:
            foreach (Renderer component in rendererComponents) {
                component.enabled = false;
            }

            // Disable colliders:
            foreach (Collider component in colliderComponents) {
                component.enabled = false;
            }

            _isActive = false;

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
        }

        void Update()
        {
            if (!_isActive) return;
            if (!_isSetup) Setup();

            string debugStr = "";
            for (int i = 0; i < 4; i++)
            {
                _translateUV[i] = GetWorld2WebcamTextureUV(corners[i].position);
                targetMat.SetVector("p" + i, _translateUV[i]);
            }

            var uv = GetWebcamTextureUV(new Vector2(0.12f, 0.88f));
            uv.x *= backgroundTex.width;
            uv.y *= backgroundTex.height;
            //Debug.Log(uv + "/" + backgroundTex.width + "," + backgroundTex.height + " : "  + backgroundTex.GetPixel(Mathf.RoundToInt(uv.x), Mathf.RoundToInt(uv.y)));


            var startPos = new Vector3(1.0f / 32.0f, 1 - 1.0f / 32.0f);
            var offset = new Vector3(1.0f / 16.0f, -1.0f / 16.0f);
            var pos = startPos;
            var totalValue = 0;
            var curValue = 1;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var realUV = GetWebcamTextureUV(pos);
                    var color = backgroundTex.GetPixel(Mathf.RoundToInt(realUV.x * backgroundTex.width), Mathf.RoundToInt(realUV.y * backgroundTex.height));
                    if (color.r + color.g + color.b < 1.0)
                    {
                        totalValue += curValue;
                        debugStr += "1";
                    }
                    else
                    {
                        debugStr += "0";
                    }
                    pos.x += offset.x;
                    curValue = curValue << 1;
                }
                debugStr += "\n";
                pos.x = startPos.x;
                pos.y += offset.y;
            }

            if (totalValue == _lastValue)
            {
                _sameValueTime++;
                if (_sameValueTime >= 20)
                {
                    Debug.Log("value is " + totalValue + ": \n" + debugStr);
                }
            }
            else
            {
                _sameValueTime = 0;
                _lastValue = totalValue;
            }
        }

        void Setup()
        {
            backgroundTex = (Texture2D) backgroundRenderer.material.mainTexture;
            targetMat.mainTexture = backgroundTex;

            var maxScale = Mathf.Max((float)Screen.width / backgroundTex.width, (float)Screen.height / backgroundTex.height);

            var t2sWidth = backgroundTex.width * maxScale;
            var t2sHeight = backgroundTex.height * maxScale;

            _webcam2ScreenScale.x = Screen.width / t2sWidth;
            _webcam2ScreenScale.y = Screen.height / t2sHeight;

            _isSetup = true;
        }

        Vector3 GetWorld2WebcamTextureUV(Vector3 position)
        {
            var viewPortPoint = Camera.main.WorldToViewportPoint(position);
            viewPortPoint.x = (0.5f + (viewPortPoint.x - 0.5f) * _webcam2ScreenScale.x) * viewPortPoint.z;
            viewPortPoint.y = (0.5f - (viewPortPoint.y - 0.5f) * _webcam2ScreenScale.y) * viewPortPoint.z;

            return viewPortPoint;
        }

        Vector2 GetWebcamTextureUV(Vector2 normalUV)
        {
            Vector3 result;
            if (normalUV.x + normalUV.y >= 1)
            {
                result = _translateUV[0] * (1 - normalUV.x - normalUV.y) + _translateUV[1] * normalUV.x + _translateUV[2] * normalUV.y;
            }
            else
            {
                var mid = new Vector2(1 - normalUV.y, normalUV.x + normalUV.y - 1);
                result = _translateUV[2] * (1 - mid.x - mid.y) + _translateUV[1] * mid.x + _translateUV[3] * mid.y;
            }
            return new Vector2(result.x / result.z, result.y / result.z);
        }

        #endregion // PRIVATE_METHODS
    }
}
