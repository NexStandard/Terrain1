using NexYaml.Parser;
using NexYaml.Serialization;
using NexYaml;
using Stride.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain1.YamlExtensions;
[System.CodeDom.Compiler.GeneratedCode("NexVYaml", "1.0.0.0")]
public struct Vector2Factory : IYamlSerializerFactory
{

    public void Register(IYamlSerializerResolver resolver)
    {
        resolver.Register(this, typeof(Stride.Core.Mathematics.Vector2), typeof(Stride.Core.Mathematics.Vector2));
        resolver.RegisterTag($"Stride.Core.Mathematics.Vector2,Terrain1", typeof(Stride.Core.Mathematics.Vector2));
        var serializer = new NexSourceGenerated_Terrain1_YamlExtensionsVector2();
        resolver.RegisterSerializer(serializer);

        resolver.Register(this, typeof(Stride.Core.Mathematics.Vector2), typeof(System.ValueType));


    }
    public YamlSerializer Instantiate(Type type)
    {
        if (type == typeof(System.ValueType)) { return new NexSourceGenerated_Terrain1_YamlExtensionsVector2(); }

        return new NexSourceGenerated_Terrain1_YamlExtensionsVector2();
    }
}
file sealed class ExternWrapper
{

}
[System.CodeDom.Compiler.GeneratedCode("NexVYaml", "1.0.0.0")]
file sealed class NexSourceGenerated_Terrain1_YamlExtensionsVector2 : YamlSerializer<Stride.Core.Mathematics.Vector2>
{

    private static readonly byte[] UTF8X = new byte[] { 88 };
    private static readonly byte[] UTF8Y = new byte[] { 89 };
    private static readonly byte[] UTF8Z = new byte[] { 90 };
    protected override DataStyle Style { get; } = DataStyle.Compact;

    public override void Write(IYamlWriter stream, Stride.Core.Mathematics.Vector2 value, DataStyle style = DataStyle.Any)
    {
        style = style == DataStyle.Any ? Style : style;
        stream.BeginMapping(style);
        stream.WriteTag("!Stride.Core.Mathematics.Vector2,Terrain1");
        stream.Write("X", value.X, style);
        stream.Write("Y", value.Y, style);

        stream.EndMapping();
    }

    public override void Read(IYamlReader stream, ref Stride.Core.Mathematics.Vector2 value, ref ParseResult parseResult)
    {
        var __TEMP__X = default(int);
        ParseResult __TEMP__RESULT__X = new();
        var __TEMP__Y = default(int);
        ParseResult __TEMP__RESULT__Y = new();

        stream.ReadMapping((key) => {
            if (
                !stream.TryRead(ref __TEMP__X, ref key, UTF8X, ref __TEMP__RESULT__X) &&
                !stream.TryRead(ref __TEMP__Y, ref key, UTF8Y, ref __TEMP__RESULT__Y)
            )
            {
                stream.SkipRead();
            }


        });

        var __TEMP__RESULT = new Stride.Core.Mathematics.Vector2
        {
            X = __TEMP__X,
            Y = __TEMP__Y
        };

        value = __TEMP__RESULT;
    }
}
