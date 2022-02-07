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
	private MapUIView mapUIView;

	[SerializeField]
	public DownwardOrthoCameraView camera; 
	[SerializeField]
	public GameObject pannableFrame; 
	[SerializeField]
	public float panningSpeed;


	public float prevCameraZoom
	{
		get => prevcamzoom;
		set { prevcamzoom = Mathf.Clamp(value, 1, 10); }
	}

	private float prevcamzoom;

	private void Awake()
    {

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
				var worldpointStart = camera.camera.ScreenToWorldPoint(start);
				
				if (false)
				{
					//do nothing.
				}
				else
				{
					//since delta translation is probably in pixels, we might want to move the camera in units of pixles.
					var pixelstomove = new Vector3(recognizer.deltaTranslation.x, recognizer.deltaTranslation.y, 0);
					//float half_vertical_visibleheight = camera.orthographicSize;
					var DPI = Screen.dpi;
					camera.transform.position -= (pixelstomove / DPI) * camera.getZoom()  * panningSpeed ;
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
				camera.setZoom(prevCameraZoom / recognizer.accumulatedScale);
				//Debug.Log("pinch recognizer fired: " + r);
			};
			TouchKit.addGestureRecognizer(recognizer);

			recognizer.gestureCompleteEvent += (r) =>
			{
				prevCameraZoom = camera.getZoom();
				//Debug.Log("pinch gesture complete");
			};
		}

		if (true)
		{
			var recognizer = new TKTapRecognizer();
			recognizer.gestureRecognizedEvent += (r) =>
			{
				Vector3 worldPoint = camera.camera.ScreenToWorldPoint((Vector2)recognizer.touchLocation());
				ulong regionCoords = MapSpaceConverters.Vector32Handle(worldPoint);
				mapUIView.getPresenter().OnMapClick(regionCoords);
				//Debug.Log("tap recognizer fired: " + r);
			};
			TouchKit.addGestureRecognizer(recognizer);

		}


	}

}
