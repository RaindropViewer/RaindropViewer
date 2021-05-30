﻿/**
 * Radegast Metaverse Client
 * Copyright(c) 2009-2014, Radegast Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Radegast is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */

using OpenMetaverse.Rendering;

namespace Raindrop.Rendering
{
    public static class RenderSettings
    {

        #region Occlusion queries
        /// <summary>Should we try to optimize by not drawing objects occluded behind other objects</summary>
        public static bool OcclusionCullingEnabled;
        //public static bool CoreQuerySupported;
        //public static bool ARBQuerySupported;
        #endregion Occlusion queries

        public static bool HasMultiTexturing;
        public static bool UseFBO;
        public static bool HasMipmap;
        public static bool HasShaders;
        public static DetailLevel PrimRenderDetail = DetailLevel.High;
        public static DetailLevel SculptRenderDetail = DetailLevel.High;
        public static DetailLevel MeshRenderDetail = DetailLevel.Highest;
        public static bool AllowQuickAndDirtyMeshing = true;
        public static int MeshesPerFrame = 16;
        public static int TexturesToDownloadPerFrame = 2;
        /// <summary>Should we try to make sure that large prims that are > our draw distance are in view when we are standing on them</summary>
        public static bool HeavierDistanceChecking = true;
        /// <summary>Minimum time between rebuilding terrain mesh and texture</summary>
        public static float MinimumTimeBetweenTerrainUpdated = 15f;
        /// <summary>Are textures that don't have dimensions that are powers of two supported</summary>
        public static bool TextureNonPowerOfTwoSupported;

        /// <summary>
        /// Render avatars
        /// </summary>
        public static bool AvatarRenderingEnabled = true;

        /// <summary>
        /// Render prims
        /// </summary>
        public static bool PrimitiveRenderingEnabled = true;

        /// <summary>
        /// Show avatar skeloton
        /// </summary>
        public static bool RenderAvatarSkeleton = false;

        /// <summary>
        /// Enable shader for shiny
        /// </summary>
        public static bool EnableShiny = false;

        #region Water
        public static bool WaterReflections = false;
        #endregion Water
    }
}
