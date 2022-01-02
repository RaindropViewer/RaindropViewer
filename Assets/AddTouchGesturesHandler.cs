using Raindrop.UI.Views;
using Raindrop.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Add the panning behavior to the current GO.
public class AddTouchGesturesHandler : MonoBehaviour
{
	[SerializeField]
	private MapUI mapUI;
	public GameObject mapUIGO;

	[SerializeField]
	public GameObject cameraGO; 
	private Camera camera; 
	[SerializeField]
	public GameObject pannableFrame; 
	[SerializeField]
	public float panningSpeed;


	public float prevCameraSize;
	public Vector2 startPoint;

	private void Awake()
    {
		camera = cameraGO.GetComponent<Camera>();

		//mapUI = Raindrop.Globals.GetUI().Get  //mapUIGO.GetComponent<MapUI>();
	}

    // Start is called before the first frame update
    void Start()
    {

		if (true)
		{
			var recognizer = new TKPanRecognizer(0.5f);

            // when using in conjunction with a pinch or rotation recognizer setting the min touches to 2 smoothes movement greatly
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                recognizer.minimumNumberOfTouches = 2;

			Rect rect = pannableFrame.GetComponent<RectTransform>().rect;
			recognizer.boundaryFrame = new TKRect(rect.x, rect.y, rect.width, rect.height);


			recognizer.gestureRecognizedEvent += (r) =>
			{
				Vector2 start = recognizer.startPoint;
				startPoint = start;
				if (false)
				{
					//do nothing.
				} else { 
					camera.transform.position -= new Vector3(recognizer.deltaTranslation.x, 0, recognizer.deltaTranslation.y) / panningSpeed * camera.orthographicSize;
				}
                Debug.Log("pan recognizer fired: " + r.ToString());
            };

			// continuous gestures have a complete event so that we know when they are done recognizing
			recognizer.gestureCompleteEvent += r =>
			{
				//Debug.Log("pan gesture complete");
			};
			TouchKit.addGestureRecognizer(recognizer);

		}


		if (true)
		{
			var recognizer = new TKPinchRecognizer();
			recognizer.gestureRecognizedEvent += (r) =>
			{
				camera.orthographicSize = prevCameraSize / recognizer.accumulatedScale;
				//Debug.Log("pinch recognizer fired: " + r);
			};
			TouchKit.addGestureRecognizer(recognizer);

			recognizer.gestureCompleteEvent += (r) =>
			{
				prevCameraSize = camera.orthographicSize;
				//Debug.Log("pinch gesture complete");
			};
		}

		if (true)
		{
			var recognizer = new TKTapRecognizer();
			recognizer.gestureRecognizedEvent += (r) =>
			{
				Vector3 worldPoint = camera.ScreenToWorldPoint((Vector2)recognizer.touchLocation());
				ulong regionCoords = Coverters.getRegionFromWorldPoint(worldPoint);
				mapUI.getPresenter().OnMapClick(regionCoords);
				//Debug.Log("tap recognizer fired: " + r);
			};
			TouchKit.addGestureRecognizer(recognizer);

		}


	}

}
