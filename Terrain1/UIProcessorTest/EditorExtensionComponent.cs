using Stride.Core.Serialization;
using Stride.Engine.Design;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;

namespace Terrain1.UIProcessorTest;

[DataContract]
[DefaultEntityComponentProcessor(typeof(EditorExtensionProcessor), ExecutionMode = ExecutionMode.Editor)]
public class EditorExtensionComponent : EntityComponent
{
    [DataMember(10)]
    public UrlReference<Prefab> BoxPrefab { get; set; }
    [DataMember(11)]
    public int PrefabNextYPosition { get; set; }

    [DataMember(20)]
    public int[] InternalData { get; set; } = new[]
    {
            1, 2, 3, 4, 5, 6
        };
}
