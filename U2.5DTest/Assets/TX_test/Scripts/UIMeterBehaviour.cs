using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VikingCrewTools{
	public class UIMeterBehaviour : MonoBehaviour {

		private float originalWidth;
		public RectTransform imageTransform;

		public float initialFillRatio = 1.0f;
		public bool removeIfZero = true;
        public bool removeIfFull = true;

        void Awake(){
			originalWidth = imageTransform.rect.width;
			SetFillRatio(initialFillRatio);
		}
		// Use this for initialization
		void Start () {

		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetFillRatio(float newRatio){
            newRatio = Mathf.Clamp01(newRatio);
            gameObject.SetActive(true);
            if (removeIfZero && newRatio <= 0){
				gameObject.SetActive(false);
			}
            if (removeIfFull && newRatio >= 1) {
                gameObject.SetActive(false);
            }
            imageTransform.sizeDelta = new Vector2(originalWidth * newRatio, imageTransform.rect.height);
		}
	}
}