﻿using Autumn.Scene.Area;
using Autumn.Scene.DefaultCube;
using Autumn.Scene.H3D;
using Autumn.Storage;
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

    private static Matrix4x4 s_h3DScale = Matrix4x4.CreateScale(0.01f);

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

        StageObj stageObj = sceneObj.StageObj;
        ActorObj actorObj = sceneObj.ActorObj;

        if (
            stageObj.Type == StageObjType.Area
            || stageObj.Type == StageObjType.CameraArea
            || stageObj.Type == StageObjType.AreaChild
        )
        {
            // TO-DO: Change color based on the name here.

            s_commonSceneParams.Transform = sceneObj.Transform;
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

            DefaultCubeRenderer.Render(
                gl,
                s_commonSceneParams,
                s_defaultCubeMaterialParams,
                sceneObj.PickingId
            );
        }
        else
        {
            if (actorObj.RenderableModels.Length <= 0)
                H3DRenderingGenerator.GenerateMaterialsAndModels(gl, actorObj);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < actorObj.RenderingMaterials.Length; j++)
                {
                    RenderableModel model = actorObj.RenderableModels[j];
                    H3DRenderingMaterial material = actorObj.RenderingMaterials[j];

                    if ((int)material.Layer != i)
                        continue;

                    material.SetSelectionColor(new(1, 1, 0, sceneObj.Selected ? 0.4f : 0));

                    material.SetMatrices(
                        s_projectionMatrix,
                        s_h3DScale * sceneObj.Transform,
                        s_viewMatrix
                    );

                    material.TryUse(gl, out ProgramUniformScope scope);

                    using (scope)
                    {
                        if (material.CullFaceMode == 0)
                            gl.Disable(EnableCap.CullFace);
                        else
                            gl.CullFace(material.CullFaceMode);

                        if (material.BlendingEnabled)
                        {
                            gl.Enable(EnableCap.Blend);

                            gl.BlendColor(
                                material.BlendingColor.X,
                                material.BlendingColor.Y,
                                material.BlendingColor.Z,
                                material.BlendingColor.W
                            );

                            gl.BlendEquationSeparate(
                                material.ColorBlendEquation,
                                material.AlphaBlendEquation
                            );

                            gl.BlendFuncSeparate(
                                material.ColorSrcFact,
                                material.ColorDstFact,
                                material.AlphaSrcFact,
                                material.AlphaDstFact
                            );
                        }

                        material.Program.TryGetUniformLoc("uPickingId", out int location);
                        gl.Uniform1(location, sceneObj.PickingId);

                        model.Draw(gl);

                        gl.Enable(EnableCap.CullFace);
                        gl.Disable(EnableCap.Blend);
                    }
                }
        }
    }
}
