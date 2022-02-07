using UnityEngine;

//Shows parcel information in a simple, confined view.
/*  Image
 *  Image
 *  Image
 * 
 *  PARCEL NAME
 *  SIMNAME
 *  SIM Coordinates
 *  ParcelDESCription
 */


namespace Raindrop.UI.map.Parcels
{
    [RequireComponent(typeof(ParcelInfoController))]
    public class ParcelInfoView : MonoBehaviour
    {
        private ParcelInfoController controller;
        void Awake()
        {
            controller = new ParcelInfoController(this);

        }

    
    
    }
}