using System.Numerics;
using ImGuiNET;
using raylib_cs_utils.Core;
using Raylib_cs;
using rlImGui_cs;

namespace raylib_cs_utils;

internal static class Program
{
    private static readonly Vector2 WindowSize = new Vector2(800, 600);
    private const string WindowTitle = "raylib_cs_imgui_template";
    
    [System.STAThread]
    public static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        
        Raylib.InitWindow((int)WindowSize.X, (int)WindowSize.Y, WindowTitle);
        Raylib.SetWindowMinSize(320, 240);
        rlImGui.Setup(true);
        const string texturePath = "assets/test.png";

        var texture = AssetManager.Load<Texture2D>(texturePath);
        var sameTexture = AssetManager.Load<Texture2D>(texturePath);
        
        Console.WriteLine(texture.Id == sameTexture.Id);
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawTexture(texture, 100, 100, Color.White);
            
            rlImGui.Begin();
            ImGui.Begin("Asset Debug");
            ImGui.Text("Texture ID:");
            ImGui.Text(texture.Id.ToString());
            if (ImGui.Button("Reload Texture"))
            {
                AssetManager.Unload<Texture2D>(texturePath);
                texture = AssetManager.Load<Texture2D>(texturePath);
            }
            ImGui.End();
            rlImGui.End();
            
            Raylib.EndDrawing();
        }
        AssetManager.UnloadAll();
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}