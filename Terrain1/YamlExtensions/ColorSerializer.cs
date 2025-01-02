using NexYaml.Parser;
using NexYaml.Serialization;
using NexYaml;
using Stride.Core;
using System;
using Stride.Core.Mathematics;

namespace Terrain1.YamlExtensions;

[System.CodeDom.Compiler.GeneratedCode("NexVYaml", "1.0.0.0")]
public struct ColorFactory : IYamlSerializerFactory
{

    public void Register(IYamlSerializerResolver resolver)
    {
        resolver.Register(this, typeof(Color), typeof(Color));
        resolver.RegisterTag($"Terrain.Color,Terrain", typeof(Color));
        var serializer = new NexSourceGenerated_Terrain_Tools_UiColor();
        resolver.RegisterSerializer(serializer);
        resolver.Register(this, typeof(Color), typeof(System.ValueType));
    }
    public YamlSerializer Instantiate(Type type)
    {
        if (type == typeof(System.ValueType)) { return new NexSourceGenerated_Terrain_Tools_UiColor(); }

        return new NexSourceGenerated_Terrain_Tools_UiColor();
    }
}
file sealed class ExternWrapper
{

}
[System.CodeDom.Compiler.GeneratedCode("NexVYaml", "1.0.0.0")]
file sealed class NexSourceGenerated_Terrain_Tools_UiColor : YamlSerializer<Color>
{

    private static readonly byte[] UTF8R = new byte[] { 82 };
    private static readonly byte[] UTF8G = new byte[] { 71 };
    private static readonly byte[] UTF8B = new byte[] { 66 };
    private static readonly byte[] UTF8A = new byte[] { 65 };

    protected override DataStyle Style { get; } = DataStyle.Compact;
    public override void Write(IYamlWriter stream, Color value, DataStyle style = DataStyle.Compact)
    {
        style = DataStyle.Any == style? Style : style;
        stream.BeginMapping(style);
        stream.WriteTag("!Terrain.Color,Terrain");
        stream.Write("R", value.R, style);
        stream.Write("G", value.G, style);
        stream.Write("B", value.B, style);
        stream.Write("A", value.A, style);

        stream.EndMapping();
    }

    public override void Read(IYamlReader stream, ref Color value, ref ParseResult parseResult)
    {
        var __TEMP__R = default(byte);
        ParseResult __TEMP__RESULT__R = new();
        var __TEMP__G = default(byte);
        ParseResult __TEMP__RESULT__G = new();
        var __TEMP__B = default(byte);
        ParseResult __TEMP__RESULT__B = new();
        var __TEMP__A = default(byte);
        ParseResult __TEMP__RESULT__A = new();

        stream.ReadMapping((key) => {
            if (
                !stream.TryRead(ref __TEMP__R, ref key, UTF8R, ref __TEMP__RESULT__R) &&
                !stream.TryRead(ref __TEMP__G, ref key, UTF8G, ref __TEMP__RESULT__G) &&
                !stream.TryRead(ref __TEMP__B, ref key, UTF8B, ref __TEMP__RESULT__B) &&
                !stream.TryRead(ref __TEMP__A, ref key, UTF8A, ref __TEMP__RESULT__A)
            )
            {
                stream.SkipRead();
            }


        });

        var __TEMP__RESULT = new Color
        {
            R = __TEMP__R,
            G = __TEMP__G,
            B = __TEMP__B,
            A = __TEMP__A
        };
        value = __TEMP__RESULT;
    }
}
