﻿using System.Diagnostics;
using Autumn.IO;
using Autumn.Storage;
using ImGuiNET;

namespace Autumn.GUI.Editors;

internal class StageWindow
{
    private const ImGuiTableFlags _stageTableFlags =
        ImGuiTableFlags.ScrollY
        | ImGuiTableFlags.RowBg
        | ImGuiTableFlags.BordersOuter
        | ImGuiTableFlags.BordersV
        | ImGuiTableFlags.Resizable;

    public static void Render(MainWindowContext context)
    {
        if (!ImGui.Begin("Stages"))
            return;

        // Stage table:
        if (ImGui.BeginTable("stageTable", 2, _stageTableFlags))
        {
            ImGui.TableSetupScrollFreeze(0, 1); // Makes top row always visible.
            //ImGui.TableSetupColumn("Position"); // (Relative to world map)
            ImGui.TableSetupColumn("Stage");
            ImGui.TableSetupColumn("Scenario", ImGuiTableColumnFlags.None, 0.35f);
            ImGui.TableHeadersRow();

            foreach (Stage stage in ProjectHandler.ActiveProject.Stages)
            {
                Debug.Assert(stage.Name is not null);

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                if (
                    ImGui.Selectable(stage.Name, false, ImGuiSelectableFlags.AllowDoubleClick)
                    && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)
                )
                {
                    Scene.Scene? scene;
                    scene = context.Scenes.Find(scene => scene.Stage == stage);

                    if (scene is not null)
                        context.CurrentScene = scene;
                    else
                    {
                        scene = new(stage);

                        if (!scene.IsReady)
                            context.BackgroundManager.Add(
                                $"Loading stage \"{stage.Name + stage.Scenario}\"...",
                                () =>
                                {
                                    if (!stage.Loaded)
                                        StageHandler.LoadProjectStage(stage);

                                    scene.GenerateSceneObjects(ref context.BackgroundManager.StatusMessageSecondary);
                                }
                            );

                        stage.Saved = true;

                        context.Scenes.Add(scene);
                    }

                    ImGui.SetWindowFocus("Scene");
                }

                ImGui.TableNextColumn();

                ImGui.Text(stage.Scenario.ToString() ?? string.Empty);
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}
