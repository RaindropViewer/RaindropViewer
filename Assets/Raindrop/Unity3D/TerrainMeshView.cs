using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop.Rendering;
using OpenMetaverse;
using System.Threading;
using RenderSettings = Raindrop.Rendering.RenderSettings;
using OpenMetaverse.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Raindrop.Unity3D
{
    //sets the mesh of the gameobject to match the sim's shape. 
    public class TerrainMeshView : MonoBehaviour, IMeshView
    {
        private TerrainMeshController controller;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        UnityEngine.Mesh terrainMesh;
        
        private void Awake()
        {
            controller = new TerrainMeshController(this);
            
            //1 mesh renderer component
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard")); //hopefully this not use reflection.

            //2 mesh filter component (owns the mesh)
            meshFilter = gameObject.AddComponent<MeshFilter>();
            
            //2.1 make terrain mesh of 256*256 at zero height and pass to meshfilter
            terrainMesh = new UnityEngine.Mesh(); //make the mesh.
            GetComponent<MeshFilter>().mesh = terrainMesh;         //assign this mesh to the meshfiltercomponent
        }

        public void SetTexture(Texture2D texture)
        {
            meshRenderer.material.mainTexture = texture;
        }

        public void SetMesh(Mesh mesh)
        {
            throw new NotImplementedException();
        }
    }
}
