using Lean.Gui;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Presenters;
using UnityEngine;

[RequireComponent(typeof(UpdateMovementBackend))]
[RequireComponent(typeof(PlayerFacingInput))]
// character will move in same direction as the joystick in the camera's current view direction.
public class JoystickCameraRelativeCharacterControl : MonoBehaviour
{
    //public float playerRotationspeed = 0.02f;
    public LeanJoystick js;
    public Transform cam;
    public Vector2 joyinput;
    public Vector3 agent_DirectionOfMovement;
    public Transform player;
    private Vector3 playerLookAt;
    public float camAngle;
    public float joyAngle;
    private float joyThreshold = 0.1f;

    public UpdateMovementBackend WASDInput;
    public PlayerFacingInput playerHeadingInput;

    public AgentPresenter agents;
    public bool debug;

    private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
    //private RaindropNetcom netcom { get { return instance.Netcom; } }
    bool Active => instance.Client.Network.Connected;

    void Start()
    {
        js = this.GetComponent<LeanJoystick>();
        if (js == null)
        {
            Debug.LogError("bad hook up!");
        }

        WASDInput = this.GetComponent<UpdateMovementBackend>();
        if (WASDInput == null)
        {
            Debug.LogError("bad hook up!");
        }
        playerHeadingInput = this.GetComponent<PlayerFacingInput>();
        if (playerHeadingInput == null)
        {
            Debug.LogError("bad hook up!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!Active && !debug)
        {
            return;
        }
        
        if (player == null)
        {
            if (agents.agentReference != null) //todo: ok we should do a event driven way instead
            {
                player = agents.agentReference.transform;
            } else
            {
                return;
            }
        }
        //1. get camera's heading as a euler around y.
        camAngle = cam.transform.localEulerAngles.y; //ACW-angle from "north"
        
        //2. get joy's heading as a 2d vector in screen space. (x,y)
        joyinput = js.ScaledValue;
        if (joyinput.magnitude < joyThreshold)
        {
            StopMovement();
            return;
        }
        
        joyinput.Normalize();

        //2b. get 2Djoy's angle from north as a Radians float (since north is our UI's frame of reference; the 0 degrees)
        //var joyForward = Vector2.up;
        //var theta = Vector2.SignedAngle(joyForward, joyinput);

        //3. rotate camera's forward vector on Y axis by this 2Djoy angle.
        //agent_DirectionOfMovement = Quaternion.Euler(0, theta, 0) * 
        //    Quaternion.Euler(0, camEulerY,0 ) * 
        //    (Vector3.forward);

        //2 ok, so berkley is very smart and concise:
        joyAngle = Mathf.Atan2(joyinput.x, joyinput.y) * Mathf.Rad2Deg; //CW-angle from "north"
        float finalAngle = joyAngle + camAngle; //degrees.

        agent_DirectionOfMovement = Quaternion.Euler(0, finalAngle, 0) * Vector3.forward;

        OrientPlayer(agent_DirectionOfMovement);
        MoveForwardInDirection();
    }

    private void OrientPlayer(Vector3 agentFacingDirection)
    {
        //var newEuler = Vector3.Lerp(playerEuler, agent_DirectionOfMovement, Time.deltaTime * playerRotationspeed);
        playerLookAt = player.transform.position + agentFacingDirection;
        player.LookAt(playerLookAt, Vector3.up);

        var heading_lefthanded = player.eulerAngles.y;
        playerHeadingInput.OnHeadingSet(heading_lefthanded);
    }

    private void MoveForwardInDirection()
    {
        WASDInput.OnWASDSet(true, false, false, false);
    }
    private void StopMovement()
    {
        WASDInput.OnWASDSet(false, false, false, false);
    }

    public void OnDrawGizmos()
    {
        if (player){
            Gizmos.DrawLine(player.transform.position ,player.transform.position + agent_DirectionOfMovement*100);
        }
    }
}
