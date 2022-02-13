using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using Raindrop;
using Raindrop.Media;
using Raindrop.Rendering;
using Raindrop.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Tests.Raindrop
{
    [TestFixture()]
    public class QuaternionAndVectorConversionTests
    {


        [Test]
        public void UnitQuaterion_AreSame()
        {
            var unity_quat = UnityEngine.Quaternion.identity;
            var omv_quat = OpenMetaverse.Quaternion.Identity;
            Assert.True(AreSameQuaternions(unity_quat, omv_quat));
        }
        
        [Test]
        public void RotatingQuaternionsAgree_yaw()
        {
            var fwd_unity = UnityEngine.Vector3.forward;
            var fwd_sl = OpenMetaverse.Vector3.UnitX;

            for (int i = 0; i <= 360; i += 10) //degrees
            {
                var rotated = UnityEngine.Quaternion.Euler(0, i, 0) * fwd_unity ;
                var rotated_sl = fwd_sl * OpenMetaverse.Quaternion.CreateFromEulers(0,0,- i * Mathf.Deg2Rad);

                Assert.True(AreSameVectors(rotated, rotated_sl) , "Failed Angle degrees : " + i);
            }
            
            Assert.Pass();
        }
        [Test]
        public void RotatingQuaternionsAgree_pitch()
        {
            var fwd_unity = UnityEngine.Vector3.forward;
            var fwd_sl = OpenMetaverse.Vector3.UnitX;

            for (int i = 0; i <= 360; i += 10) //degrees
            {
                var rotated = UnityEngine.Quaternion.Euler(i, 0, 0) * fwd_unity ; //pitch down
                var rotated_sl = fwd_sl * OpenMetaverse.Quaternion.CreateFromEulers(0, i * Mathf.Deg2Rad,0); //pitch down

                Assert.True(AreSameVectors(rotated, rotated_sl) , "Failed Angle degrees : " + i);
            }
            
            Assert.Pass();
        }
        [Test]
        public void RotatingQuaternionsAgree_roll()
        {
            var fwd_unity = UnityEngine.Vector3.forward;
            var fwd_sl = OpenMetaverse.Vector3.UnitX;

            for (int i = 0; i <= 360; i += 10) //degrees
            {
                var rotated = UnityEngine.Quaternion.Euler(0, 0, 10) * fwd_unity ; //roll to the left
                var rotated_sl = fwd_sl * OpenMetaverse.Quaternion.CreateFromEulers( - i * Mathf.Deg2Rad,0,0); //roll to the left

                Assert.True(AreSameVectors(rotated, rotated_sl) , "Failed Angle degrees : " + i);
            }
            
            Assert.Pass();
        }

        [Test]
        public void UnitForwardsVector_AreSame()
        {
            var unity_v3 = UnityEngine.Vector3.forward;
            var omv_v3 = OpenMetaverse.Vector3.UnitX; //fwd
            Assert.True(AreSameVectors(unity_v3, omv_v3));
        }  
        
        [Test]
        public void UnitRightVector_AreSame()
        {
            var unity_v3 = UnityEngine.Vector3.right;
            var omv_v3 = - OpenMetaverse.Vector3.UnitY; //left
            Assert.True(AreSameVectors(unity_v3, omv_v3));
        }
        
        [Test]
        public void UnitUpVector_AreSame()
        {
            var unity_v3 = UnityEngine.Vector3.up;
            var omv_v3 = OpenMetaverse.Vector3.UnitZ; //up
            Assert.True(AreSameVectors(unity_v3, omv_v3));
        }

        [Test]
        public void DifferentVector_AreDifferent()
        {
            var unity_v3 = UnityEngine.Vector3.right;
            var omv_v3 = OpenMetaverse.Vector3.UnitY; //left
            Assert.True(! AreSameVectors(unity_v3, omv_v3));
        }

        private bool AreSameVectors(Vector3 unityV3, OpenMetaverse.Vector3 omvV3)
        {
            var v1 = RHelp.TKVector3(omvV3);
            var b1 =(v1 == (unityV3)); //approx. equality
            var v2 = RHelp.OMVVector3(unityV3);
            var b2 =(v2.ApproxEquals(omvV3, 0.02f));
            return b1 & b2;
        }
      

        private bool AreSameQuaternions(Quaternion unityQuat, OpenMetaverse.Quaternion omvQuat)
        {
            var omv_Converted_to_ue = RHelp.TKQuaternion4(omvQuat);
            bool b1 =  omv_Converted_to_ue== (unityQuat);
            var ue_Converted_to_omv = RHelp.OMVQuaternion4(unityQuat);
            bool b2 = ue_Converted_to_omv.ApproxEquals(omvQuat, 0.1f);

            return b1 & b2;
        }

        [Test]
        public void UnitQuaterion_backforthconversion_Unity2OMV()
        {
            var unity_norot = UnityEngine.Quaternion.identity;
            var omv_converted = RHelp.OMVQuaternion4(unity_norot);
            var unity_norot_convertedback = RHelp.TKQuaternion4(omv_converted);
            Assert.True(unity_norot.Equals(unity_norot_convertedback));
        }
        
        [Test]
        public void UnitVector3_backforthconversion_Unity2OMV()
        {
            var unity_identity = UnityEngine.Vector3.forward;
            var omv_converted = RHelp.OMVVector3(unity_identity);
            var unity_identity_convertedback = RHelp.TKVector3(omv_converted);
            Assert.True(unity_identity.Equals( unity_identity_convertedback));
        }
        
        [Test]
        public void ApplyRotation_OMV_facingleft()
        {
            var omv_fwd = OpenMetaverse.Vector3.UnitX;
            var omv_rotated_left = omv_fwd * OpenMetaverse.Quaternion.CreateFromEulers(0,0,90 * Mathf.Deg2Rad);
            
            Assert.True(omv_rotated_left.ApproxEquals(OpenMetaverse.Vector3.UnitY, 0.1f));
        }
        
        [Test]
        public void FromOrigin_RotateRight90deg_Success()
        {
            //starting at origin...
            var origin = UnityEngine.Vector3.zero;
            var originSL = OpenMetaverse.Vector3.Zero;
            //starting at facing forward...
            var orientation = UnityEngine.Vector3.forward;
            var orientationSL = OpenMetaverse.Vector3.UnitX;

            //get quaternion for "rotate right 90 deg".
            var howToRotate = UnityEngine.Quaternion.AngleAxis(90, Vector3.up);
            var howToRotateSL = OpenMetaverse.Quaternion.CreateFromEulers(0,0,- 90 * Mathf.Deg2Rad);
            Assert.True(AreSameQuaternions(howToRotate,howToRotateSL), 
                "error: different quaternions : " + howToRotate.ToString() + " " + howToRotateSL.ToString() );
            
            //do the rotate...
            var final_orientation = howToRotate * orientation ;
            var final_orientation_SL = orientationSL * howToRotateSL;
            //check if final orientations are the same.
            Assert.True(AreSameVectors(final_orientation,final_orientation_SL),
                "error: different vectors: " + final_orientation.ToString() + " " + final_orientation_SL.ToString());

            var modelOrientation = UnityEngine.Vector3.right;
            Assert.True(AreSameVectors(modelOrientation,final_orientation_SL));
        }
        
        [Test]
        public void FromOrigin_RotateRight90degAndMoveForward_Success()
        {
            //starting at origin...
            var pos = UnityEngine.Vector3.zero;
            var posSL = OpenMetaverse.Vector3.Zero;
            //starting at facing forward...
            var orientation = UnityEngine.Vector3.forward;
            var orientationSL = OpenMetaverse.Vector3.UnitX;

            //get quaternion for "rotate right 90 deg".
            var rot = UnityEngine.Quaternion.AngleAxis(90, Vector3.up);
            var rotSL = OpenMetaverse.Quaternion.CreateFromEulers(0,0,- 90 * Mathf.Deg2Rad);
            Assert.True(AreSameQuaternions(rot,rotSL), 
                "error: different quaternions : " + rot.ToString() + " " + rotSL.ToString() );
            
            //do the rotate...
            orientation = rot * orientation ;
            orientationSL = orientationSL * rotSL;
            //check if final orientations are the same.
            Assert.True(AreSameVectors(orientation,orientationSL),
                "error: different vectors: " + orientation.ToString() + " " + orientationSL.ToString());
  
            //move forward by 1 unit...
            pos = pos + rot * Vector3.forward;
            posSL = posSL + OpenMetaverse.Vector3.UnitX * rotSL;
            
            //check final pos. and orientation
            Assert.True(AreSameVectors(pos,posSL),
                "error: different pos v3 : " + pos.ToString() + " " + posSL.ToString());
            Assert.True(AreSameVectors(orientation,orientationSL),
                "error: different orientation v3 : " + orientation.ToString() + " " + orientationSL.ToString());
            
        }
    }
}