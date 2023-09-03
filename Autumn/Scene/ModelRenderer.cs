﻿using Autumn.IO;
using Autumn.Scene.Area;
using Autumn.Scene.DefaultCube;
using Autumn.Scene.H3D;
using Autumn.Storage;
using Autumn.Storage.StageObjs;
using SceneGL;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Autumn.Scene;

internal static class ModelRenderer
{
    private static CommonSceneParameters? s_commonSceneParams;
    private static CommonMaterialParameters? s_defaultCubeMaterialParams;
    private static CommonMaterialParameters? s_areaMaterialParams;

    private static Matrix4x4 s_viewMatrix = Matrix4x4.Identity;
    private static Matrix4x4 s_projectionMatrix = Matrix4x4.Identity;

    public static void Initialize(GL gl)
    {
        DefaultCubeRenderer.Initialize(gl);
        AreaRenderer.Initialize(gl);

        s_commonSceneParams = new(gl);
        s_defaultCubeMaterialParams = new(gl, new(1, 0.5f, 0, 1), new(1, 1, 0));
        s_areaMaterialParams = new(gl, new(0, 1, 0, 1), new(1, 1, 0));
    }

    public static void UpdateMatrices(in Matrix4x4 view, in Matrix4x4 projection)
    {
        if (s_commonSceneParams is null)
            throw new InvalidOperationException(
                $@"{nameof(ModelRenderer)} must be initialized before any calls to {nameof(UpdateMatrices)}"
            );

        s_viewMatrix = view;
        s_projectionMatrix = projection;

        s_commonSceneParams.ViewProjection = view * projection;
    }

    public static void Draw(GL gl, SceneObj sceneObj)
    {
        if (
            s_commonSceneParams is null
            || s_defaultCubeMaterialParams is null
            || s_areaMaterialParams is null
        )
            throw new InvalidOperationException(
                $@"{nameof(ModelRenderer)} must be initialized before any calls to {nameof(Draw)}"
            );

        IStageObj stageObj = sceneObj.StageObj;
        ActorObj actorObj = sceneObj.ActorObj;

        if (stageObj is AreaStageObj || stageObj is CameraAreaStageObj)
        {
            // TO-DO: Change color based on the name here.

            s_areaMaterialParams.Selected = sceneObj.Selected;

            gl.CullFace(CullFaceMode.Back);

            AreaRenderer.Render(gl, s_commonSceneParams, s_areaMaterialParams);
            return;
        }

        if (actorObj.IsNoModel)
        {
            s_commonSceneParams.Transform = sceneObj.Transform;
            s_defaultCubeMaterialParams.Selected = sceneObj.Selected;

            gl.CullFace(CullFaceMode.Back);

            DefaultCubeRenderer.Render(gl, s_commonSceneParams, s_defaultCubeMaterialParams);
        }
        else
        {
            if (actorObj.RenderableModels.Length <= 0)
                H3DRenderingGenerator.GenerateMaterialsAndModels(gl, actorObj);

            for (int i = 0; i < actorObj.RenderingMaterials.Length; i++)
            {
                RenderableModel model = actorObj.RenderableModels[i];
                H3DRenderingMaterial material = actorObj.RenderingMaterials[i];

                material.SetMatrices(s_projectionMatrix, sceneObj.Transform * 0.01f, s_viewMatrix);

                material.TryUse(gl, out ProgramUniformScope scope);

                using (scope)
                {
                    if (material.CullFaceMode == 0)
                        gl.Disable(EnableCap.CullFace);
                    else
                        gl.CullFace(material.CullFaceMode);

                    model.Draw(gl);

                    gl.Enable(EnableCap.CullFace);
                }
            }
        }

        // if(!s_modelCache.TryGetValue(name, out var model)) {
        //     SPICA.Formats.CtrH3D.H3D? h3D = RomFSHandler.RequestModel(name);

        //     if(h3D is null)
        //         model = null;
        //     else
        //         model = new(h3D, name, gl);

        //     s_modelCache.Add(name, model);
        // }

        //if(name == "FirstStepASideView")
        //    return;

        // if(model is null) {
        //     s_defaultCubeSceneParams.Transform = transform;

        //     DefaultCubeRenderer.Render(gl, s_defaultCubeMaterial, s_defaultCubeSceneParams);
        // } else { // Render H3D:
        //     model.Render(gl, s_viewMatrix, s_projectionMatrix, transform);
        // }
    }

    // public static void CleanUp(GL gl)
    // {
    //     DefaultCubeRenderer.CleanUp(gl);

    //     foreach (var (_, model) in s_modelCache)
    //         model?.CleanUp(gl);
    // }
}
