using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;

public static partial class AsyncImageLoader {
    public class ImageExporter : IDisposable
    {
#if UNITY_2020_1_OR_NEWER
        // Maximum texture size supported is 16K
        const int MAX_TEXTURE_DIMENSION = 16384;
        const int MAX_MIPMAP_COUNT = 15;
#else
    // Maximum texture size supported is 8K
    const int MAX_TEXTURE_DIMENSION = 8192;
    const int MAX_MIPMAP_COUNT = 14;
#endif
        
        static readonly ProfilerMarker ConstructorMarker = new ProfilerMarker("ImageExporter.Constructor");
        static readonly ProfilerMarker ExportFromTextureMarker = new ProfilerMarker("ImageExporter.ExportFromTexture");
        static readonly ProfilerMarker ProcessRawTextureDataMarker = new ProfilerMarker("ImageImporter.ProcessRawTextureData");

        LoaderSettings _loaderSettings;
        IntPtr _bitmap; //the freeimage bitmap - the FIBITMAP 
        
        // input image parameters
        private TextureFormat _textureFormat;
        private int _pixelSize;
        private int _width;
        private int _height;
        
        //buffer size for intermediary output (the encode part).
        private int bufferSize; 

        JobHandle _finalJob;
        public ImageExporter(Texture2D texture, LoaderSettings loaderSettings)
        {
            Assert.IsTrue(texture.mipmapCount == 1);
            
            using (ConstructorMarker.Auto())
            {
                _loaderSettings = loaderSettings;
                _bitmap = IntPtr.Zero;

                if (_loaderSettings.format == FreeImage.Format.FIF_UNKNOWN)
                {
                    throw new Exception("no image format specified for export!");
                }

                try
                {
                    // 0. extract input image's parameters.
                    getAndSet_image_parameters_from_T2D(texture);

                    // 1. get input image bytes.
                    using (NativeArray<byte> imagebitmap = texture.GetRawTextureData<byte>())
                    {
                        unsafe
                        {
                            // convert RGB (texture2d.getrawtexturedata ) to BGR (FreeIMage.ConvertFromRawBits require Format).
                            ProcessRawTextureData(imagebitmap, texture.mipmapCount);   
                            _finalJob.Complete();

                            //2. get pointer to input image's first texel.
                            IntPtr in_t2d_ptr =
                                (IntPtr) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                                    imagebitmap); // void* -> IntPtr explicit conversion.

                            //2. obtain a FIbitmap from the input image's bits
                            _bitmap = FreeImage.ConvertFromRawBits((IntPtr) in_t2d_ptr,
                                texture.width,
                                texture.height,
                                _pixelSize * CalculateMipmapDimensions(0).x,
                                _textureFormat == TextureFormat.RGBA32 ? 32u : 24u,
                                0, 0, 0, false
                            );
                        }
                    }


                }
                catch (Exception e)
                {
                    Debug.LogError("constructor error");
                }
            }
        }

        int2 CalculateMipmapDimensions(int mipmapLevel) {
            // Base level
            if (mipmapLevel == 0) {
                return int2(_width, _height);
            } else {
                var mipmapFactor = Mathf.Pow(2f, -mipmapLevel);
                var mipmapWidth = Mathf.Max(1, Mathf.FloorToInt(mipmapFactor * _width));
                var mipmapHeight = Mathf.Max(1, Mathf.FloorToInt(mipmapFactor * _height));
                return int2(mipmapWidth, mipmapHeight);
            }
        }

        
        private void getAndSet_image_parameters_from_T2D(Texture2D t2d)
        {
            var type = t2d.format;
            switch (type) {
                case TextureFormat.RGB24:
                    _textureFormat = TextureFormat.RGB24;
                    _pixelSize = 3;
                    break;
                case TextureFormat.RGBA32:
                    _textureFormat = TextureFormat.RGBA32;
                    _pixelSize = 4;
                    break;
                default:
                    throw new Exception($"Image type not supported: {type}");
            }

            _width = t2d.width;
            _height = t2d.height;

            bufferSize = _pixelSize * _width * _height;
        }


        public void ExportFromTexture(ref byte[] jp2)
        {
            using (ExportFromTextureMarker.Auto())
            {
                unsafe
                {
                    IntPtr hmem = IntPtr.Zero;

                    using (NativeArray<byte> FIMemory_Output = new NativeArray<byte>(
                               bufferSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
                    {
                            
                        try {
                                
                            //this part is create output array 
                            //3. create (over-sized) space to store the encoded bytes.
                            // byte[] out_jp2 = new byte[texture.width * texture.height];
                            //get IntPtr to the buffer array.
                            // IntPtr out_jp2_ptr =
                            //     (IntPtr) Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(
                            //         FIMemory_Output); // void* -> IntPtr explicit conversion.

                            //4. open a memory stream; FIMEMORY
                            IntPtr FI_MS = IntPtr.Zero;
                            hmem = FreeImage.OpenMemory(
                                FI_MS, 0);
                                // out_jp2_ptr,
                                // (uint) FIMemory_Output.Length);
                                        
                            //5. encode and save image to memory.
                            bool success = FreeImage.SaveToMemory(
                                _loaderSettings.format,
                                _bitmap,
                                hmem,
                                0
                            );
                            
                            //6. grab the final data in memory
                            var data = IntPtr.Zero;
                            int size = 0;
                            FreeImage.AcquireMemory( hmem, ref data, ref size );

                            // 7. memcpy from intptr into final result jp2
                            jp2 = new byte[size];
                            Marshal.Copy(data, jp2,0, size);
                            
                        }
                        finally
                        {
                            if (hmem != IntPtr.Zero) FreeImage.CloseMemory(hmem);
                        }
                    }

                }
            }
        }
        
        [BurstCompile(CompileSynchronously = true)]
        struct BGRToRGBJob : IJobParallelFor {
            public delegate void BGRToRGBDelegate(ref NativeSlice<byte> textureData, int index);

            public static readonly FunctionPointer<BGRToRGBDelegate> BGR24ToRGB24FP = BurstCompiler.CompileFunctionPointer<BGRToRGBDelegate>(BGR24ToRGB24);
            public static readonly FunctionPointer<BGRToRGBDelegate> BGRA32ToRGBA32FP = BurstCompiler.CompileFunctionPointer<BGRToRGBDelegate>(BGRA32ToRGBA32);

            [BurstCompile(CompileSynchronously = true)]
            static void BGR24ToRGB24(ref NativeSlice<byte> textureData, int index) {
                var temp = textureData[mad(3, index, 0)];
                textureData[mad(3, index, 0)] = textureData[mad(3, index, 2)];
                textureData[mad(3, index, 2)] = temp;
            }

            [BurstCompile(CompileSynchronously = true)]
            static void BGRA32ToRGBA32(ref NativeSlice<byte> textureData, int index) {
                var temp = textureData[mad(4, index, 0)];
                textureData[mad(4, index, 0)] = textureData[mad(4, index, 2)];
                textureData[mad(4, index, 2)] = temp;
            }

            [NativeDisableParallelForRestriction]
            public NativeSlice<byte> textureData;
            public FunctionPointer<BGRToRGBDelegate> processFunction;

            public void Execute(int index) => processFunction.Invoke(ref textureData, index);
        }
        
        public void ProcessRawTextureData(NativeArray<byte> rawTextureView, int mipmapCount) {
            using (ProcessRawTextureDataMarker.Auto()) {
                var mipmapDimensions = CalculateMipmapDimensions(0);
                var mipmapSize = mipmapDimensions.x * mipmapDimensions.y;
                var mipmapSlice = new NativeSlice<byte>(rawTextureView, 0, _pixelSize * mipmapSize);
                var mipmapIndex = _pixelSize * mipmapSize;

                _finalJob = new BGRToRGBJob {
                    textureData = mipmapSlice,
                    processFunction = _textureFormat == TextureFormat.RGBA32 ?
                        BGRToRGBJob.BGRA32ToRGBA32FP : BGRToRGBJob.BGR24ToRGB24FP
                }.Schedule(mipmapSize, 8192);

                // no need to generatte mip maps
                // for (int mipmapLevel = 1; mipmapLevel < mipmapCount; mipmapLevel++) {
                //     var nextMipmapDimensions = CalculateMipmapDimensions(mipmapLevel);
                //     mipmapSize = nextMipmapDimensions.x * nextMipmapDimensions.y;
                //     var nextMipmapSlice = new NativeSlice<byte>(rawTextureView, mipmapIndex, _pixelSize * mipmapSize);
                //     mipmapIndex += _pixelSize * mipmapSize;
                //
                //     _finalJob = new FilterMipmapJob {
                //         inputMipmap = mipmapSlice,
                //         inputDimensions = mipmapDimensions,
                //         outputMipmap = nextMipmapSlice,
                //         outputDimensions = nextMipmapDimensions,
                //         processFunction = _textureFormat == TextureFormat.RGBA32 ?
                //             FilterMipmapJob.FilterMipmapRGBA32FP : FilterMipmapJob.FilterMipmapRGB24FP
                //     }.Schedule(mipmapSize, 1024, _finalJob);
                //
                //     mipmapDimensions = nextMipmapDimensions;
                //     mipmapSlice = nextMipmapSlice;
                // }
            }
        }

        public void Dispose()
        {
            if (_bitmap != IntPtr.Zero)
            {
                FreeImage.Unload(_bitmap);
                _bitmap = IntPtr.Zero;
            }
        }

    }
}