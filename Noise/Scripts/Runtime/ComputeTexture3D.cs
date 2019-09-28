using System.Collections;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("Noise/Compute Texture 3D")]
public class ComputeTexture3D : ComputeTexture {
    public ComputeShader texture3DSlicer;

    //-------------------------------------------------------------------------------------------------------------------
	// Generator Functions
	//-------------------------------------------------------------------------------------------------------------------
    public override void GenerateTexture(){
        int kernel = computeShader.FindKernel(kernelName);
        computeShader.Dispatch(kernel, 
            squareResolution/computeThreads.x, 
            squareResolution/computeThreads.y, 
            squareResolution/computeThreads.z);
    }

    public override void CreateRenderTexture(){
        //0 is the bits of the depth buffer, not the resolution of the z-direction
        //We use 0 because Unity currently does not support depth buffers in 3D textures
        RenderTexture rt = new RenderTexture(squareResolution, squareResolution, 0, renderTextureFormat);
        rt.enableRandomWrite = true;
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        rt.volumeDepth = squareResolution;
        rt.Create();
        rwTexture.rt = rt;
    }

    //-------------------------------------------------------------------------------------------------------------------
	// Save/Utility Functions
	//-------------------------------------------------------------------------------------------------------------------
    RenderTexture Copy3DSliceToRenderTexture(int layer){
        RenderTexture render = new RenderTexture(squareResolution, squareResolution, 0, renderTextureFormat);
		render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
		render.enableRandomWrite = true;
		render.wrapMode = TextureWrapMode.Clamp;
		render.Create();

        int kernelIndex = texture3DSlicer.FindKernel("CSMain");
        texture3DSlicer.SetTexture(kernelIndex, "noise", rwTexture.rt);
        texture3DSlicer.SetInt("layer", layer);
        texture3DSlicer.SetTexture(kernelIndex, "Result", render);
        texture3DSlicer.Dispatch(kernelIndex, squareResolution, squareResolution, 1);

        return render;
    }
   
    public override void SaveAsset(){
        //for readability
        int dim = squareResolution;
        //Slice 3D Render Texture to individual layers
        RenderTexture[] layers = new RenderTexture[squareResolution];
        for(int i = 0; i < squareResolution; i++)
            layers[i] = Copy3DSliceToRenderTexture(i);
        //Write RenderTexture slices to static textures
        Texture2D[] finalSlices = new Texture2D[squareResolution];
        for(int i = 0; i < squareResolution; i++)
            finalSlices[i] = ConvertFromRenderTexture(layers[i], assetTextureFormat);
        //Build 3D Texture from 2D slices
        Texture3D output = new Texture3D(dim, dim, dim, assetTextureFormat, true);
        output.filterMode = FilterMode.Trilinear;
        Color[] outputPixels = output.GetPixels();
        for(int k = 0; k < dim; k++){
            Color[] layerPixels = finalSlices[k].GetPixels();
            for(int i = 0; i < dim; i++){
                for(int j = 0; j < dim; j++){
                    outputPixels[i + j * dim + k * dim * dim] = layerPixels[i+j*dim];
                }
            }
        }

        output.SetPixels(outputPixels);
        output.Apply();

        string path = "Assets/" + assetName + ".asset";
        AssetDatabase.CreateAsset(output, path);
        Debug.Log("Wrote 3D Texture "+output+" to "+path);
    }

    public void SaveSliceAsset(int slice){
        //for readability
        int dim = squareResolution;
        //Slice 3D Render Texture to individual layers
        RenderTexture layer = Copy3DSliceToRenderTexture(slice);
        //Write RenderTexture slices to static textures
        Texture2D finalSlice =  ConvertFromRenderTexture(layer, assetTextureFormat);
        //Build 3D Texture from 2D slices
        Texture2D output = finalSlice;

        string path = "Assets/" + assetName + ".asset";
        AssetDatabase.CreateAsset(output, path);
        Debug.Log("Wrote 2D Slice Texture "+output+" to "+path);
    }
    public void GenerateSlice(int slice)
	{  
		CreateRenderTexture();
		SetParameters();
		SetTexture();
		GenerateTexture();
		SaveSliceAsset(slice);
	}
}
