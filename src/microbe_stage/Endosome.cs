﻿using System;
using Godot;
using Newtonsoft.Json;

/// <summary>
///   Visuals of engulfing something and encasing it in a "membrane" bubble
/// </summary>
public partial class Endosome : Node3D, IEntity
{
    private readonly StringName tintParameterName = new("tint");

    [JsonProperty]
    private Color tint = Colors.White;

    [JsonProperty]
    private int renderPriority;

    [JsonIgnore]
    public MeshInstance3D? Mesh { get; private set; }

    [JsonIgnore]
    public Color Tint
    {
        get => tint;
        set
        {
            // EngulfingSystem always updates the property values so we skip applying this to the shader if the value
            // didn't change
            if (tint == value)
                return;

            tint = value;
            ApplyTint();
        }
    }

    [JsonIgnore]
    public int RenderPriority
    {
        get => renderPriority;
        set
        {
            renderPriority = value;
            ApplyRenderPriority();
        }
    }

    [JsonIgnore]
    public Node3D EntityNode => this;

    [JsonIgnore]
    public AliveMarker AliveMarker { get; } = new();

    public override void _Ready()
    {
        Mesh = GetNode<MeshInstance3D>("EngulfedObjectHolder") ?? throw new Exception("mesh node not found");

        var material = Mesh!.MaterialOverride as ShaderMaterial;

        if (material == null)
            GD.PrintErr("Material is not found from the EngulfedObjectHolder mesh for Endosome");

        // TODO: check if this could now be done in Godot 4
        // This has to be done here because setting this in Godot editor truncates
        // the number to only 3 decimal places.
        material?.SetShaderParameter("jiggleAmount", 0.0001f);

        ApplyTint();
        ApplyRenderPriority();
    }

    public void OnDestroyed()
    {
        AliveMarker.Alive = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            tintParameterName.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ApplyTint()
    {
        var material = Mesh?.MaterialOverride as ShaderMaterial;
        material?.SetShaderParameter(tintParameterName, tint);
    }

    private void ApplyRenderPriority()
    {
        if (Mesh == null)
            return;

        var material = Mesh.MaterialOverride;
        material.RenderPriority = renderPriority;
    }
}
